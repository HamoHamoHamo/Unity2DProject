using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float dodgeForce = 1.1f;
    [SerializeField] private float jumpForce = 15.0f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Flip")]
    [SerializeField] private Transform swordTransform;

    [Header("Slash Effect")]
    [SerializeField] private SlashEffect slashEffect;

    [Header("Item System")]
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float itemPickupRange = 1f;
    [SerializeField] private LayerMask throwableItemLayer;
    private ThrowableItem heldItem = null;
    private List<ThrowableItem> nearbyItems = new List<ThrowableItem>();

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isFacingRight = true;

    private bool canAttack = true;
    private float attackTimer;

    private bool isDropping;
    private bool isDodging;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        HandleInput();
        AttackCooldown();
    }

    void FixedUpdate()
    {
        if (!isDodging)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        HandleJump();

        if (moveInput > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Flip(transform);
        }
        else if (moveInput < 0 && isFacingRight)
        {
            isFacingRight = false;
            Flip(transform);
        }

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            anim.SetBool("Move", true);
        }
        else
        {
            anim.SetBool("Move", false);
        }

        anim.SetBool("IsGround", isGrounded);
        anim.SetFloat("YSpeed", rb.velocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    private void HandleInput()
    {

        // 이동
        if (!isDodging)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }

        // 점프
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && !isDodging)
        {
            jumpPressed = true;
        }

        // 공격
        if (Input.GetMouseButtonDown(0) && canAttack && !isDodging)
        {
            Debug.Log("Attack");
            Attack();
        }

        // 투사체
        if (Input.GetMouseButtonDown(1))
        {
            if (heldItem != null && !isDodging)
            {
                // 이미 들고 있으면 던지기
                ThrowHeldItem();
            }
            else
            {
                // 들고 있지 않으면 습득 시도
                TryPickupNearestItem();
            }
        }

        // 아랫점프
        if (Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump") && !isDodging)
        {
            DropThroughPlatform();
        }

        // 구르기
        else if (!isDodging && isGrounded && Input.GetKey(KeyCode.S) && moveInput != 0)
        {
            Dodge();
        }

    }

    private void HandleJump()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded)
        {
            Jump();
        }

        if (rb.velocity.y < 0)
        {
            // 낙하 시 중력 증가
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.W))
        {
            // 점프 키를 떼면 상승 속도 감소 (짧은 점프 가능)
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        jumpPressed = false;
    }

    private void Dodge()
    {
        isDodging = true;
        StartCoroutine(DodgeCo());
    }

    private IEnumerator DodgeCo()
    {
        anim.SetTrigger("Dodge");
        rb.velocity = new Vector2(moveInput * moveSpeed * dodgeForce, rb.velocity.y);

        // Enemy 레이어와의 충돌 일시적으로 무시
        // TODO 적 공격, 투사체, 적 충돌 테스트
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int playerLayerIndex = gameObject.layer;
        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);

        yield return new WaitForSeconds(0.6f);

        isDodging = false;
        // 충돌 복구
        Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);
    }

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.velocity += Vector2.up * jumpForce;
    }

    private void AttackCooldown()
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

    private void Attack()
    {
        canAttack = false;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 mouseDirection = (mousePos - transform.position).normalized;

        if (mouseDirection.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(mouseDirection.y, mouseDirection.x) * Mathf.Rad2Deg;
            Debug.Log(angle);
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        attackTimer = attackCooldown;
        anim.SetTrigger("Attack");
    }

    // Animation Event에서 호출될 메서드
    public void OnSlashFrame()
    {
        if (slashEffect != null)
        {
            slashEffect.PlaySlashEffect();
        }
    }

    private void Flip(Transform transform)
    {
        Vector3 visualScale = transform.localScale;
        visualScale.x *= -1;
        transform.localScale = visualScale;
    }

    public bool IsFacingRight => isFacingRight;

    void TryPickupNearestItem()
    {
        // 범위 내 모든 투사체 찾기
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            itemPickupRange,
            throwableItemLayer
        );

        ThrowableItem nearestItem = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D col in colliders)
        {
            ThrowableItem item = col.GetComponent<ThrowableItem>();

            if (item != null && !item.IsHeld && !item.IsThrown)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestItem = item;
                }
            }
        }

        // 가장 가까운 아이템 습득
        if (nearestItem != null)
        {
            heldItem = nearestItem;
            heldItem.Pickup(itemHoldPoint, this);
        }
    }

    void ThrowHeldItem()
    {
        if (heldItem == null) return;

        // 마우스 방향 계산
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mousePos - itemHoldPoint.position).normalized;

        // 던지기
        heldItem.Throw(throwDirection);
        heldItem = null;
    }

    private void DropThroughPlatform()
    {
        if (isDropping || !isGrounded) return;
        StartCoroutine(DropThroughCo());
    }

    private IEnumerator DropThroughCo()
    {
        isDropping = true;

        int platformLayerIndex = LayerMask.NameToLayer("Platform");
        int playerLayerIndex = gameObject.layer;

        // 플랫폼 레이어와의 충돌 일시적으로 무시
        Physics2D.IgnoreLayerCollision(playerLayerIndex, platformLayerIndex, true);
        rb.velocity = new Vector2(rb.velocity.x, -3f);


        yield return new WaitForSeconds(0.5f);
        // 충돌 복구
        Physics2D.IgnoreLayerCollision(playerLayerIndex, platformLayerIndex, false);

        isDropping = false;
    }

}