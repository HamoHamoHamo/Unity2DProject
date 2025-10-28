using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float throwForce = 15f;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;

    private bool isHeld = false;
    private bool isThrown = false;
    private Transform holdPoint;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 초기 상태: 바닥에 떨어진 아이템
        SetPhysicsState(false);

    }

    void Update()
    {
        if (isHeld)
        {
            // 플레이어가 들고 있을 때 손 위치에 고정
            transform.position = holdPoint.position;

            // 마우스 방향을 바라보도록
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90);
        }
    }

    /// <summary>
    /// 플레이어가 아이템을 습득
    /// </summary>
    public void Pickup(Transform holdPosition, PlayerController playerController)
    {
        if (isHeld) return;

        isHeld = true;
        isThrown = false;
        holdPoint = holdPosition;

        // 물리 비활성화
        SetPhysicsState(false);

        // 부모 설정 (선택사항 - 더 안정적인 추적)
        // transform.SetParent(holdPoint);

        Debug.Log($"{gameObject.name} picked up!");
    }

    /// <summary>
    /// 아이템을 던지기
    /// </summary>
    public void Throw(Vector2 direction)
    {
        if (!isHeld) return;

        isHeld = false;
        isThrown = true;
        holdPoint = null;

        // 부모 해제
        Managers.Pool.ReturnPool(this, true);

        // 물리 활성화
        SetPhysicsState(true);

        // 던지기
        rb.velocity = direction.normalized * throwForce;

        Debug.Log($"{gameObject.name} thrown!");

        // 일정 시간 후 자동 제거 (맵 밖으로 나가는 것 방지)
        // Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isThrown) return;

        // 적에게 맞았을 때
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            // 적 데미지 처리
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // enemy.TakeDamage(damage);
                Debug.Log($"Enemy hit for {damage} damage!");
            }

            // 아이템 제거 또는 튕겨나가게 하기
            Destroy(gameObject);

            // 또는 튕겨나가게 하려면:
            // isThrown = false;
            // rb.velocity *= 0.3f; // 속도 감소
        }

        // 벽에 맞았을 때
        if (other.CompareTag("Ground"))
        {
            // 바닥에 떨어뜨리기
            isThrown = false;
            SetPhysicsState(false);

            // 또는 벽에 박히게 하려면:
            // Destroy(gameObject, 3f); // 3초 후 제거
        }
    }

    private void SetPhysicsState(bool active)
    {
        if (active)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // 디버그용 - 습득 범위 표시
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }

    public bool IsHeld => isHeld;
    public bool IsThrown => isThrown;
}