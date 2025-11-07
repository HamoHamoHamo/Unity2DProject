using System.Collections;
using UnityEngine;

/// <summary>
/// 원거리 적이 발사하는 탄환
/// PoolManager로 관리되며 일정 시간 후 자동 반환
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 3f; // 발사 후 자동 반환까지 시간

    [Header("Deflect Settings")]
    [SerializeField] private Color deflectedColor = Color.cyan; // 튕겨진 Bullet 색상

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private int damage;
    private bool isActive;
    private bool isDeflected; // 튕겨진 상태
    private Color originalColor; // 원래 색상

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Bullet 초기화 및 발사
    /// </summary>
    /// <param name="direction">발사 방향 (정규화된 벡터)</param>
    /// <param name="bulletDamage">데미지</param>
    public void Initialize(Vector2 direction, int bulletDamage)
    {
        damage = bulletDamage;
        isActive = true;
        isDeflected = false; // 초기화 시 튕겨지지 않은 상태

        // 색상 초기화
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        // 발사 방향으로 속도 설정
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }

        // 일정 시간 후 자동 반환
        StartCoroutine(ReturnAfterLifetime());
    }

    /// <summary>
    /// Bullet을 튕겨냄 (Player가 공격으로 튕겨낼 때 호출)
    /// </summary>
    /// <param name="newDirection">새로운 방향 (정규화된 벡터)</param>
    public void Deflect(Vector2 newDirection)
    {
        if (!isActive || isDeflected) return; // 이미 튕겨진 Bullet은 다시 튕겨지지 않음

        isDeflected = true;

        // 방향 반전
        if (rb != null)
        {
            rb.velocity = newDirection.normalized * speed;
        }

        // 회전 업데이트 (Bullet이 날아가는 방향으로 회전)
        float angle = Mathf.Atan2(newDirection.y, newDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 색상 변경 (시각적 피드백)
        if (spriteRenderer != null)
        {
            spriteRenderer.color = deflectedColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // 튕겨진 Bullet의 충돌 처리
        if (isDeflected)
        {
            // Enemy와 충돌
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // 데미지 처리
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }

                ReturnToPool();
            }
            // 벽/바닥 또는 Player와 충돌 시 반환
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground") || other.CompareTag("Player"))
            {
                ReturnToPool();
            }
        }
        // 일반 Bullet의 충돌 처리
        else
        {
            // 플레이어와 충돌
            if (other.CompareTag("Player"))
            {
                Debug.Log("투사체 플레이어 적중");
                // 데미지 처리
                IDamageable damageable = other.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }

                ReturnToPool();
            }
            // 벽/바닥과 충돌
            else if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                ReturnToPool();
            }
        }
    }

    /// <summary>
    /// 일정 시간 후 자동으로 풀에 반환
    /// </summary>
    private IEnumerator ReturnAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        if (isActive)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    private void ReturnToPool()
    {
        if (!isActive) return;

        isActive = false;
        StopAllCoroutines();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        Managers.Pool.ReturnPool(this);
    }

    void OnDisable()
    {
        // 비활성화 시 코루틴 정리
        StopAllCoroutines();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
        isActive = false;
    }
}
