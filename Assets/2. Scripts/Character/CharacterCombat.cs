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

    private Animator anim;
    private CharacterMovement movement;

    private bool canAttack = true;
    private float attackTimer;
    private GameObject fireTarget;

    public bool CanAttack => canAttack;

    void Awake()
    {
        anim = GetComponent<Animator>();
        movement = GetComponent<CharacterMovement>();
    }

    void Update()
    {
        UpdateAttackCooldown();
    }

    private void UpdateAttackCooldown()
    {
        if (!canAttack)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                canAttack = true;
            }
        }
    }

    /// <summary>
    /// 특정 방향으로 공격 실행
    /// </summary>
    public void Attack(Vector2 direction)
    {
        if (!canAttack || movement.IsDodging) return;

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
    /// Animation Event에서 호출 - 슬래시 이펙트 재생 또는 Bullet 발사
    /// </summary>
    public void OnSlashFrame()
    {
        if (slashEffect != null)
        {
            slashEffect.PlaySlashEffect();
        }

        // 근접 공격 판정
        PerformAttackDetection();
    }

    /// <summary>
    /// 공격 판정 실행 - 박스 범위 내 적 감지 및 데미지
    /// </summary>
    private void PerformAttackDetection()
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

        Debug.Log($"공격 판정: {hitTargets.Length}개 감지");

        // 감지된 대상에게 데미지
        foreach (Collider2D target in hitTargets)
        {
            // 자기 자신은 제외
            if (target.transform == transform || target.transform.root == transform.root)
                continue;

            // 구르기 중인지 확인
            CharacterMovement targetMovement = target.GetComponent<CharacterMovement>();
            if (targetMovement != null && targetMovement.IsDodging)
            {
                Debug.Log($"{target.name}이(가) 구르기 중이라 회피!");
                continue; // 데미지 무시
            }

            // IDamageable 인터페이스를 구현한 컴포넌트 찾기
            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name}이(가) {target.name}에게 {attackDamage} 데미지!");
            }
        }
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
    /// Bullet 발사 (원거리 공격)
    /// </summary>
    public void FireBullet()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[CharacterCombat] bulletPrefab이 설정되지 않았습니다!");
            return;
        }

        if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // 플레이어 찾기
            fireTarget = GameObject.FindGameObjectWithTag("Player");
        }
        else if (gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // TODO 튕겨내기
            fireTarget = null;
        }

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
        Vector3 spawnPosition = bulletSpawnPoint != null ? bulletSpawnPoint.position : transform.position;
        bullet.transform.position = spawnPosition;

        // 플레이어 방향 계산
        Vector2 direction = (fireTarget.transform.position - spawnPosition).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        // Bullet 초기화 및 발사
        bullet.Initialize(direction, attackDamage);

        Debug.Log($"[CharacterCombat] Bullet 발사! 방향: {direction}");
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
            Vector3.one  // ← 이대로 두면 됨!
        );
        Gizmos.DrawCube(Vector3.zero, new Vector3(attackBoxSize.x, attackBoxSize.y, 0.1f));

        // 박스 테두리
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackBoxSize.x, attackBoxSize.y, 0.1f));
    }
}