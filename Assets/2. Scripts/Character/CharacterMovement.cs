using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어와 적이 공유하는 이동/점프/구르기 로직
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float dodgeForce = 1.1f;
    [SerializeField] private float dodgeTime = 0.6f;
    [SerializeField] private float jumpForce = 15.0f;
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;
    private bool isDodging;
    private bool isDropping;
    private bool isFacingRight = true;

    private float currentMoveInput;
    private bool jumpRequested;

    public bool IsGrounded => isGrounded;
    public bool IsDodging => isDodging;
    public bool IsFacingRight => isFacingRight;
    public float MoveSpeed => moveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        UpdateGroundCheck();
        ApplyMovement();
        HandleJumpPhysics();
        UpdateAnimations();
    }

    private void UpdateGroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void ApplyMovement()
    {
        if (!isDodging)
        {
            rb.velocity = new Vector2(currentMoveInput * moveSpeed, rb.velocity.y);
        }

        // 자동 회전
        if (currentMoveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (currentMoveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    private void HandleJumpPhysics()
    {
        // 점프 실행
        if (jumpRequested && isGrounded && !isDodging)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.velocity += Vector2.up * jumpForce;
            jumpRequested = false;
        }

        // 낙하 시 중력 증가
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // 점프 키를 떼면 상승 속도 감소
        else if (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
    }

    private void UpdateAnimations()
    {
        if (anim != null)
        {
            anim.SetBool("Move", Mathf.Abs(currentMoveInput) > 0.01f);
            anim.SetBool("IsGround", isGrounded);
            anim.SetFloat("YSpeed", rb.velocity.y);
        }
    }

    /// <summary>
    /// 이동 입력 설정 (-1 ~ 1)
    /// </summary>
    public void SetMoveInput(float input)
    {
        if (!isDodging)
        {
            currentMoveInput = input;
        }
    }

    /// <summary>
    /// 점프 요청
    /// </summary>
    public void RequestJump()
    {
        if (!isDodging)
        {
            jumpRequested = true;
        }
    }

    /// <summary>
    /// 구르기 실행
    /// </summary>
    public void Dodge()
    {
        if (isDodging || !isGrounded) return;
        StartCoroutine(DodgeCo());
    }

    private IEnumerator DodgeCo()
    {
        isDodging = true;

        if (anim != null)
        {
            anim.SetTrigger("Dodge");
        }

        rb.velocity = new Vector2(currentMoveInput * moveSpeed * dodgeForce, rb.velocity.y);

        // Enemy와 Bullet 레이어와의 충돌 일시적으로 무시
        int enemyLayerIndex = LayerMask.NameToLayer("Enemy");
        int bulletLayerIndex = LayerMask.NameToLayer("Bullet");
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        int currentLayer = gameObject.layer;

        if (currentLayer == playerLayerIndex)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, true);
            Physics2D.IgnoreLayerCollision(playerLayerIndex, bulletLayerIndex, true);
        }

        yield return new WaitForSeconds(dodgeTime);

        isDodging = false;

        if (currentLayer == playerLayerIndex)
        {
            Physics2D.IgnoreLayerCollision(playerLayerIndex, enemyLayerIndex, false);
            Physics2D.IgnoreLayerCollision(playerLayerIndex, bulletLayerIndex, false);
        }
    }

    /// <summary>
    /// 플랫폼 아래로 떨어지기
    /// </summary>
    public void DropThroughPlatform()
    {
        if (isDropping || !isGrounded) return;
        StartCoroutine(DropThroughCo());
    }

    private IEnumerator DropThroughCo()
    {
        isDropping = true;

        int platformLayerIndex = LayerMask.NameToLayer("Platform");
        int currentLayerIndex = gameObject.layer;

        Physics2D.IgnoreLayerCollision(currentLayerIndex, platformLayerIndex, true);
        rb.velocity = new Vector2(rb.velocity.x, -3f);

        yield return new WaitForSeconds(0.5f);

        Physics2D.IgnoreLayerCollision(currentLayerIndex, platformLayerIndex, false);
        isDropping = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// 특정 방향으로 강제 회전
    /// </summary>
    public void FaceDirection(bool faceRight)
    {
        if (isFacingRight != faceRight)
        {
            Flip();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}