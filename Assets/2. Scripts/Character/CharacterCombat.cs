using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 개선된 전투 시스템 - OverlapBoxAll 사용 (박스 형태 공격 범위)
/// 플레이어와 적 모두 사용 가능
/// </summary>
public class CharacterCombat : MonoBehaviour
{
    public enum AttackType
    {
        Melee,   // 근접 공격
        Ranged   // 원거리 공격 (Bullet 발사)
    }

    [Header("Combat Settings")]
    [SerializeField] private AttackType attackType = AttackType.Melee;
    [SerializeField] private float attackCooldown = 2.0f;
    [SerializeField] private int attackDamage = 1;

    [Header("Attack Detection - Box Area (Melee Only)")]
    [SerializeField] private Transform attackArea; // 공격 범위의 중심점 (크기 정보 포함)
    [SerializeField] private Vector2 attackBoxSize = new Vector2(2f, 1f); // 박스 크기 (width, height)
    [SerializeField] private LayerMask targetLayer; // 공격 대상 레이어

    [Header("Ranged Attack Settings")]
    [SerializeField] private Bullet bulletPrefab; // Bullet 프리팹 (원거리 공격용)
    [SerializeField] private Transform bulletSpawnPoint; // Bullet 발사 위치
    [SerializeField] private float bulletSpeed = 10f; // Bullet 속도

    [Header("References")]
    [SerializeField] private SlashEffect slashEffect;

    [Header("Groggy Settings")]
    [SerializeField] private float groggyDuration = 1f; // 그로기 지속 시간

    private Animator anim;
    private CharacterMovement movement;
    private Rigidbody2D rb;

    private bool canAttack = true;
    private float attackTimer;
    private GameObject fireTarget;

    // 공격 판정 지속 관련
    private bool isPerformingAttack = false;
    private HashSet<Collider2D> hitTargetsInCurrentAttack = new HashSet<Collider2D>();
    private bool isProcessingSequentialHits = false;

    // 그로기 상태
    private bool isGroggy = false;
    private bool isPerformingGroggy = false;

    private bool isDead = false;

    // Public 프로퍼티 (외부 접근용)
    public bool CanAttack => canAttack;
    public bool IsPerformingAttack => isPerformingAttack;
    public bool IsPerformingGroggy => isPerformingGroggy;
    public bool IsGroggy => isGroggy;
    public bool IsDead => isDead;
    public Transform AttackArea => attackArea;
    public Vector2 AttackBoxSize => attackBoxSize;

    void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<CharacterMovement>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateAttackCooldown();
        UpdateAttackDetection();
    }

    private void UpdateAttackCooldown()
    {
        if (!canAttack && !isGroggy)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                canAttack = true;
            }
        }
    }

    /// <summary>
    /// 공격 판정 지속 처리 - Animation Event로 시작/종료 제어
    /// </summary>
    private void UpdateAttackDetection()
    {
        if (isPerformingAttack && !isProcessingSequentialHits)
        {
            PerformAttackDetection(hitTargetsInCurrentAttack);
        }
    }

    /// <summary>
    /// 특정 방향으로 공격 실행
    /// </summary>
    public void Attack(Vector2 direction)
    {
        if (!canAttack || movement.IsDodging || isGroggy) return;

        canAttack = false;
        attackTimer = attackCooldown;

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }
    }

    /// <summary>
    /// 타겟을 향해 공격
    /// </summary>
    public void AttackTowards(Transform target)
    {
        if (target == null) return;

        Vector2 direction = (target.position - transform.position).normalized;
        Attack(direction);
    }

    /// <summary>
    /// Animation Event에서 호출 - 슬래시 이펙트 재생 및 공격 판정 시작
    /// </summary>
    public void OnSlashFrame()
    {
        // 슬래시 이펙트 재생 (옵션)
        if (slashEffect != null)
        {
            slashEffect.PlaySlashEffect();
        }

        // 공격 판정 시작
        isPerformingAttack = true;
        hitTargetsInCurrentAttack.Clear();
    }

    /// <summary>
    /// Animation Event에서 호출 - 공격 판정 종료 및 슬래시 이펙트 종료
    /// </summary>
    public void StopAttack()
    {
        // 공격 판정 종료
        isPerformingAttack = false;
        hitTargetsInCurrentAttack.Clear();

        // 그로기 판정 종료
        isPerformingGroggy = false;

        // 슬래시 이펙트 종료 (옵션)
        if (slashEffect != null)
        {
            slashEffect.StopSlashEffect();
        }
    }

    /// <summary>
    /// Enemy Animation Event에서 호출 - 공격 판정 전 그로기 판정 시작
    /// </summary>
    public void StartPerformGroggy()
    {
        isPerformingGroggy = true;
    }

    /// <summary>
    /// 공격 판정 실행 - 박스 범위 내 적 감지 및 데미지 (중복 타격 방지)
    /// </summary>
    private void PerformAttackDetection(HashSet<Collider2D> alreadyHitTargets)
    {
        if (attackArea == null)
        {
            Debug.LogWarning("AttackArea가 설정되지 않았습니다!");
            return;
        }

        // 박스 범위 내 모든 콜라이더 감지
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(
            attackArea.position,      // 박스 중심 위치
            attackBoxSize,            // 박스 크기
            attackArea.eulerAngles.z, // 박스 회전 (캐릭터 회전과 동기화)
            targetLayer               // 대상 레이어
        );

        List<Collider2D> validTargets = new List<Collider2D>();

        // 감지된 대상에게 데미지
        foreach (Collider2D target in hitTargets)
        {
            // 자기 자신은 제외
            if (target.transform == transform || target.transform.root == transform.root)
                continue;

            // 이미 맞은 대상은 제외 (중복 타격 방지)
            if (alreadyHitTargets.Contains(target))
                continue;

            validTargets.Add(target);
        }

        // 타격할 대상이 있으면 순차 처리 시작
        if (validTargets.Count > 0 && !isProcessingSequentialHits)
        {
            StartCoroutine(ProcessSequentialHitsCo(validTargets, alreadyHitTargets));
        }

    }

    /// <summary>
    /// 순차적으로 적을 타격하며 히트스톱 적용
    /// </summary>
    private IEnumerator ProcessSequentialHitsCo(List<Collider2D> targets, HashSet<Collider2D> alreadyHitTargets)
    {
        isProcessingSequentialHits = true;

        foreach (Collider2D target in targets)
        {
            if (target == null) continue;

            // 구르기 중인지, 죽었는지 확인
            CharacterMovement targetMovement = target.GetComponent<CharacterMovement>();
            CharacterCombat targetCombat = target.GetComponent<CharacterCombat>();
            if ((targetMovement != null && targetMovement.IsDodging) || (targetCombat != null && targetCombat.IsDead))
            {
                continue; // 데미지 무시
            }

            // Player일 때만 Bullet 튕겨내기 시도
            if (gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Bullet bullet = target.GetComponent<Bullet>();
                if (bullet != null)
                {
                    // Bullet의 현재 방향을 가져와서 반대로 튕겨냄
                    Rigidbody2D bulletRb = target.GetComponent<Rigidbody2D>();
                    if (bulletRb != null)
                    {
                        Vector2 deflectDirection = -bulletRb.velocity.normalized;
                        bullet.Deflect(deflectDirection);
                        alreadyHitTargets.Add(target); // 중복 튕겨내기 방지

                        // Bullet 튕겨낼 때도 히트스톱 적용
                        if (HitEffectManager.Instance != null)
                        {
                            HitEffectManager.Instance.PlayHitEffect(0.08f, 0.1f, 0.1f, 0.15f);
                        }
                        yield return new WaitForSecondsRealtime(0.08f);

                        continue; // Bullet은 데미지 대상이 아니므로 다음으로
                    }
                }

                // Melee Enemy와의 공격 충돌 감지 (Player만)
                if (target.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    if (targetCombat != null &&
                        targetCombat.IsPerformingGroggy &&
                        !targetCombat.IsGroggy &&
                        targetCombat.AttackArea != null
                    )
                    {
                        // Enemy도 공격 판정 중이고 그로기 상태가 아님
                        // 두 공격 박스가 겹치는지 확인
                        if (CheckAttackBoxOverlap(targetCombat))
                        {
                            // 양쪽 모두 그로기 상태로
                            EnterGroggy();
                            targetCombat.EnterGroggy();

                            alreadyHitTargets.Add(target); // 중복 처리 방지

                            // 그로기 상태 진입 시 히트스톱
                            if (HitEffectManager.Instance != null)
                            {
                                HitEffectManager.Instance.PlayHitEffect(0.15f, 0.05f, 0.2f, 0.3f);
                            }
                            yield return new WaitForSecondsRealtime(0.15f);

                            continue; // 데미지는 입히지 않음
                        }
                    }
                }
            }

            if (!isGroggy)
            {
                // IDamageable 인터페이스를 구현한 컴포넌트 찾기
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    // 데미지 적용
                    damageable.TakeDamage(attackDamage);
                    alreadyHitTargets.Add(target); // 타격한 대상 기록

                    // 히트스톱 적용 (적 타격 시)
                    if (HitEffectManager.Instance != null)
                    {
                        // 플레이어 공격일 때 더 강한 히트스톱
                        if (gameObject.layer == LayerMask.NameToLayer("Player"))
                        {
                            HitEffectManager.Instance.PlayHitEffect();
                            yield return new WaitForSecondsRealtime(0.2f);
                        }
                        else
                        {
                            // 적 공격일 때는 약한 히트스톱
                            HitEffectManager.Instance.PlayHitEffect(0.1f, 0.1f, 0.1f, 0.15f);
                            yield return new WaitForSecondsRealtime(0.1f);
                        }
                    }
                }
            }
        }

        isProcessingSequentialHits = false;
    }

    /// <summary>
    /// 공격 범위 내 타겟 확인 (추적용)
    /// </summary>
    public bool IsTargetInRange(Transform target, float range)
    {
        if (target == null) return false;
        return Vector2.Distance(transform.position, target.position) <= range;
    }

    /// <summary>
    /// 두 공격 박스가 겹치는지 확인 (AABB 간단 버전)
    /// </summary>
    private bool CheckAttackBoxOverlap(CharacterCombat other)
    {
        if (attackArea == null || other.AttackArea == null) return false;

        // 두 박스의 중심점
        Vector2 myCenter = attackArea.position;
        Vector2 otherCenter = other.AttackArea.position;

        // 두 박스의 크기 (절반 크기 계산)
        Vector2 myHalfSize = attackBoxSize * 0.5f;
        Vector2 otherHalfSize = other.AttackBoxSize * 0.5f;

        // AABB 충돌 검사 (회전 무시)
        // X축 겹침 확인
        bool overlapX = Mathf.Abs(myCenter.x - otherCenter.x) < (myHalfSize.x + otherHalfSize.x);
        // Y축 겹침 확인
        bool overlapY = Mathf.Abs(myCenter.y - otherCenter.y) < (myHalfSize.y + otherHalfSize.y);
        // 두 축 모두 겹치면 충돌
        return overlapX && overlapY;
    }

    /// <summary>
    /// 그로기 상태 진입
    /// </summary>
    public void EnterGroggy()
    {
        if (isGroggy) return; // 이미 그로기 상태면 무시

        // PlayerController의 대시 코루틴 중단 (Player만)
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.StopAllCoroutines();
        }

        // 현재 공격 중단
        StopAttack();

        // 애니메이션 트리거 - 즉시 전환
        if (anim != null)
        {
            // 현재 재생 중인 모든 애니메이션 즉시 중단
            anim.ResetTrigger("Attack");
            anim.SetBool("IsGroggy", true);
            // Play를 사용하면 더 확실하게 즉시 재생됩니다
            Managers.Sound.Play("Groggy");
            anim.Play("Groggy", 0, 0f); // 레이어 0, 시작부터 재생
        }

        // 그로기 코루틴 시작
        StartCoroutine(GroggyCo());
    }

    /// <summary>
    /// 그로기 코루틴 - 일정 시간 후 그로기 해제
    /// </summary>
    private IEnumerator GroggyCo()
    {
        isGroggy = true;
        canAttack = false;

        // 이동 제한 (CharacterMovement에 알림)
        if (movement != null)
        {
            movement.CanMove(false);
            movement.Knockback();
        }

        yield return new WaitForSeconds(groggyDuration);

        // 그로기 해제
        isGroggy = false;
        anim.SetBool("IsGroggy", false);

        // 이동 제한 해제
        if (movement != null)
        {
            movement.CanMove(true);
        }
    }

    public void EnterDie()
    {
        StopAllCoroutines();
        anim.ResetTrigger("Attack");
        anim.SetBool("IsGroggy", false);

        isGroggy = false;
        canAttack = false;
        movement.SetMoveInput(0);
        movement.CanMove(false);

    }

    public void SetDeadStatus(bool dead)
    {
        isDead = dead;

        if (dead)
        {
            EnterDie();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;

        }
        else
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
        }
    }

    /// <summary>
    /// Bullet 발사 (원거리 공격)
    /// </summary>
    public void FireBullet(Transform fireTarget)
    {
        if (fireTarget == null)
            return;

        // Bullet을 풀에서 가져오기
        Bullet bullet = Managers.Pool.GetFromPool(bulletPrefab);
        if (bullet == null)
        {
            Debug.LogWarning("[CharacterCombat] Bullet 풀이 비어있습니다!");
            return;
        }

        // Bullet 위치 설정 (발사 지점이 있으면 사용, 없으면 자기 위치)
        Vector2 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        bullet.transform.position = spawnPosition;

        // 플레이어 방향 계산
        float upwardOffset = 1f;
        Vector2 targetPosition = (Vector2)fireTarget.position + Vector2.up * upwardOffset;
        Vector2 direction = (targetPosition - spawnPosition).normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Bullet 초기화 및 발사
        bullet.Initialize(direction, attackDamage);
    }

    // Gizmos로 공격 범위 시각화
    void OnDrawGizmosSelected()
    {
        if (attackArea == null) return;

        // 박스 범위 그리기
        Gizmos.color = new Color(1, 0, 0, 0.3f); // 반투명 빨강
        Gizmos.matrix = Matrix4x4.TRS(
            attackArea.position,
            attackArea.rotation,
            Vector3.one
        );
        Gizmos.DrawCube(Vector3.zero, new Vector3(attackBoxSize.x, attackBoxSize.y, 0.1f));

        // 박스 테두리
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackBoxSize.x, attackBoxSize.y, 0.1f));
    }
}