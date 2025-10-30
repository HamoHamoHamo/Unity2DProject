using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 플레이어를 추적하고 공격하는 적 AI
/// </summary>
public class Enemy : MonoBehaviour, IDamageable
{
    public enum EnemyType
    {
        Melee,   // 근접 적
        Ranged   // 원거리 적
    }

    [Header("AI Settings")]
    [SerializeField] private EnemyType enemyType = EnemyType.Melee;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float meleeAttackRange = 2f; // 근접 공격 범위
    [SerializeField] private float rangedAttackRange = 8f; // 원거리 공격 범위
    [SerializeField] private float meleeStopDistance = 2f; // 근접 적 정지 거리
    [SerializeField] private float rangedStopDistance = 5f; // 원거리 적 정지 거리

    [Header("Jump Detection")]
    [SerializeField] private float jumpCheckDistance = 2f;
    [SerializeField] private float jumpHeightThreshold = 1f; // 플레이어가 이 높이 이상 위에 있으면 점프
    [SerializeField] private float maxJumpHeight = 5f; // 점프로 도달 가능한 최대 높이 (이보다 높으면 점프 안함)
    [SerializeField] private float dropHeightThreshold = 1f; // 플레이어가 이 높이 이하에 있으면 아랫점프
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Behavior Timers")]
    [SerializeField] private float jumpCooldown = 1f; // 점프 쿨다운
    [SerializeField] private float dropCooldown = 0.5f; // 아랫점프 쿨다운
    [SerializeField] private float attackDelay = 0.3f; // 공격 전 딜레이 (예측 가능성)
    [SerializeField] private float attackDuration = 0.5f; // 공격 애니메이션 지속 시간

    [Header("Movement Smoothing")]
    [SerializeField] private float movementSmoothTime = 0.3f; // 이동 반응 시간 (높을수록 느리게 반응)

    [Header("References")]
    [SerializeField] private Transform player;

    private CharacterMovement movement;
    private CharacterCombat combat;
    private Animator anim;

    private float lastJumpTime;
    private float lastDropTime;
    private bool isAttacking;
    private bool isDead = false;

    // 스무딩 관련 변수
    private Vector2 smoothedTargetPosition;
    private Vector2 smoothVelocity;

    private EnemyState currentState = EnemyState.Idle;

    private enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();
        anim = GetComponent<Animator>();

        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || currentState == EnemyState.Dead) return;

        // 공격 중이 아닐 때만 상태 전환
        if (!isAttacking)
        {
            UpdateState();
        }

        // 상태별 행동
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState();
                break;
            case EnemyState.Chasing:
                HandleChasingState();
                break;
            case EnemyState.Attacking:
                HandleAttackingState();
                break;
        }
    }

    /// <summary>
    /// 상태 업데이트 (공격 중이 아닐 때만)
    /// </summary>
    private void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 타입별 공격 범위 선택
        float attackRange = (enemyType == EnemyType.Melee) ? meleeAttackRange : rangedAttackRange;

        if (distanceToPlayer > detectionRange)
        {
            currentState = EnemyState.Idle;
        }
        else if (distanceToPlayer <= attackRange && combat.CanAttack)
        {
            currentState = EnemyState.Attacking;
        }
        else
        {
            currentState = EnemyState.Chasing;
        }
    }

    private void HandleIdleState()
    {
        // 정지
        movement.SetMoveInput(0);
    }

    private void HandleChasingState()
    {
        // 타입별 정지 거리 선택
        float stopDistance = (enemyType == EnemyType.Melee) ? meleeStopDistance : rangedStopDistance;

        // 근접 적만 점프/아랫점프 시도 (원거리 적은 평지에서만 활동)
        if (enemyType == EnemyType.Melee && movement.IsGrounded)
        {
            if (ShouldJumpToReachPlayer())
            {
                movement.RequestJump();
                lastJumpTime = Time.time;
            }
            else if (ShouldDropToReachPlayer())
            {
                movement.DropThroughPlatform();
                lastDropTime = Time.time;
            }
        }

        // 플레이어 위치를 부드럽게 추적 (스무딩) - 항상 업데이트!
        smoothedTargetPosition = Vector2.SmoothDamp(
            smoothedTargetPosition,
            player.position,
            ref smoothVelocity,
            movementSmoothTime
        );

        // 공중에 있을 때는 이동 입력을 하지 않음 (관성으로만 이동)
        if (!movement.IsGrounded)
        {
            return;
        }

        // 부드럽게 처리된 타겟까지의 거리 계산
        float distanceToTarget = Vector2.Distance(transform.position, smoothedTargetPosition);

        if (distanceToTarget > stopDistance)
        {
            float direction = Mathf.Sign(smoothedTargetPosition.x - transform.position.x);
            movement.SetMoveInput(direction);

            // 플레이어 방향 바라보기
            bool shouldFaceRight = player.position.x > transform.position.x;
            movement.FaceDirection(shouldFaceRight);
        }
        else
        {
            // 원거리 적: 일정 거리 유지
            // 근접 적: 너무 가까우면 멈춤
            movement.SetMoveInput(0);
        }
    }

    private void HandleAttackingState()
    {
        // 공격 중에는 완전히 멈춤
        movement.SetMoveInput(0);

        // 아직 공격을 실행하지 않았다면
        if (!isAttacking)
        {
            StartCoroutine(ExecuteAttackSequence());
        }
    }

    /// <summary>
    /// 공격 실행 시퀀스 (방향 고정 → 딜레이 → 공격 → 대기)
    /// </summary>
    private IEnumerator ExecuteAttackSequence()
    {
        isAttacking = true;

        // 1. 공격 전 플레이어 방향으로 회전 (한 번만!)
        bool shouldFaceRight = player.position.x > transform.position.x;
        movement.FaceDirection(shouldFaceRight);

        // 2. 공격 전 딜레이 (플레이어가 회피할 시간)
        yield return new WaitForSeconds(attackDelay);

        // 3. 공격 실행
        if (player != null) // 딜레이 중 플레이어가 사라질 수도 있음
        {
            combat.AttackTowards(player);
        }

        // 4. 공격 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(attackDuration);

        // 5. 공격 완료 - 다시 움직일 수 있음
        isAttacking = false;
    }

    public void FireBullet()
    {
        combat.FireBullet();
    }

    /// <summary>
    /// 플레이어에게 도달하기 위해 점프해야 하는지 판단
    /// </summary>
    private bool ShouldJumpToReachPlayer()
    {
        // 점프 쿨다운 체크
        if (Time.time - lastJumpTime < jumpCooldown) return false;
        if (!movement.IsGrounded) return false;

        // 플레이어가 위에 있는지 확인
        float heightDifference = player.position.y - transform.position.y;

        // 플레이어가 점프 가능한 높이 범위 안에 있는지 확인
        // - 너무 낮으면 점프할 필요 없음
        // - 너무 높으면 점프해도 도달 불가
        if (heightDifference < jumpHeightThreshold || heightDifference > maxJumpHeight)
        {
            return false;
        }

        // 앞에 벽이나 장애물이 있는지 확인
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Vector2.right * direction;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, jumpCheckDistance, obstacleLayer);

        // 벽이 있으면 점프 (벽을 넘어가기 위해)
        if (hit.collider != null)
        {
            return true;
        }

        // 앞에 바닥이 없는지 확인 (낭떠러지)
        Vector2 frontGroundCheck = rayOrigin + rayDirection * 1f;
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 2f, obstacleLayer);

        if (groundHit.collider == null && heightDifference > 0)
        {
            return true; // 낭떠러지가 있고 플레이어가 위에 있으면 점프
        }

        return false;
    }

    /// <summary>
    /// 플레이어에게 도달하기 위해 아랫점프 해야 하는지 판단
    /// </summary>
    private bool ShouldDropToReachPlayer()
    {
        // 아랫점프 쿨다운 체크
        if (Time.time - lastDropTime < dropCooldown) return false;
        if (!movement.IsGrounded) return false;

        // 플레이어가 아래에 있는지 확인
        float heightDifference = transform.position.y - player.position.y;
        if (heightDifference < dropHeightThreshold) return false;

        // 발 밑이 플랫폼인지 확인 (아랫점프 가능한 지형)
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            2f,
            LayerMask.GetMask("Platform")
        );

        Debug.Log($"Down {heightDifference} {hit.collider != null}");

        return hit.collider != null;
    }

    /// <summary>
    /// 적이 죽었을 때 호출
    /// </summary>
    public void Die()
    {
        if (!isDead)
        {
            isDead = true;
            // 실행 중인 코루틴 정리
            StopAllCoroutines();
            isAttacking = false;

            currentState = EnemyState.Dead;
            movement.SetMoveInput(0);

            // TODO: 사망 애니메이션, 점수 추가, 오브젝트 풀로 반환 등
            StartCoroutine(DieCo());


        }
    }

    private IEnumerator DieCo()
    {
        anim.SetTrigger("Die");

        yield return new WaitForSeconds(1f);

        gameObject.SetActive(false);

    }

    /// <summary>
    /// 데미지를 받았을 때
    /// </summary>
    public void TakeDamage(int damage)
    {
        Die();
    }

    // Gizmos로 AI 범위 시각화
    void OnDrawGizmosSelected()
    {
        // 타입별 범위 선택
        float attackRange = (enemyType == EnemyType.Melee) ? meleeAttackRange : rangedAttackRange;
        float stopDistance = (enemyType == EnemyType.Melee) ? meleeStopDistance : rangedStopDistance;

        // 탐지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 정지 거리
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // 점프 체크 거리
        if (movement != null && movement.IsFacingRight)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * jumpCheckDistance);
        }
        else if (movement != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * jumpCheckDistance);
        }

        // 공격 상태 표시
        if (isAttacking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}