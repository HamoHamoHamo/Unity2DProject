using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Flip")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Transform swordTransform;

    private Animator anim;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private bool isFacingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = visualTransform.GetComponent<Animator>();
        sr = visualTransform.GetComponent<SpriteRenderer>();

    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpPressed = false;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput > 0 && !isFacingRight)
        {
            isFacingRight = true;
            Flip(visualTransform);

        }
        else if (moveInput < 0 && isFacingRight)
        {
            isFacingRight = false;
            Flip(visualTransform);
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

    private void Flip(Transform transform)
    {
        Vector3 visualScale = transform.localScale;
        visualScale.x *= -1;
        transform.localScale = visualScale;
    }

    // 다른 스크립트에서 방향 확인이 필요할 때
    public bool IsFacingRight => isFacingRight;

}
