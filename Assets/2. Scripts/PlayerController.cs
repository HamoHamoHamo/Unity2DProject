using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;
    [SerializeField] private float attackCooldown = 2.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Flip")]
    [SerializeField] private Transform swordTransform;

    [Header("Slash Effect")]
    [SerializeField] private SlashEffect slashEffect;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isFacingRight = true;

    private bool canAttack = true;
    private float attackTimer;

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
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpPressed = false;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

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
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            Debug.Log("Attack");
            Attack();
        }
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

    // ⭐ Animation Event에서 호출될 메서드
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
}