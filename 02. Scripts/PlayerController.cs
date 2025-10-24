using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�̵�����")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float jumpForce = 7.0f;

    [Header("�ٴ� üũ")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float moveInput;
    private bool jumpPressed;
    private bool isGrounded;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");//�Է�
        //������û
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressed = true;
        }
    }
    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (jumpPressed && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpPressed = false;

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (moveInput != 0f)
        {
            if (moveInput < 0.0f)
            {
                sr.flipX = true;

            }
            else
            {
                sr.flipX = false;
            }
        }


        //�ִϸ�����
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
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
