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

    private Rigidbody2D rb;
    private int damage;
    private bool isActive;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

        // 발사 방향으로 속도 설정
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }

        // 일정 시간 후 자동 반환
        StartCoroutine(ReturnAfterLifetime());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;

        // 플레이어와 충돌
        if (other.CompareTag("Player"))
        {
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
