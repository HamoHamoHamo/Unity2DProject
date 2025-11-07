using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리팩토링된 PlayerController - CharacterMovement와 CharacterCombat을 사용
/// </summary>
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Attack")]
    [SerializeField] private float attackDashForce = 7f;
    [SerializeField] private float attackDashTime = 0.3f;

    [Header("Item System")]
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float itemPickupRange = 2f;
    [SerializeField] private LayerMask throwableItemLayer;
    [SerializeField] private GameObject slashEffectDegree;

    private ThrowableItem heldItem = null;

    private CharacterMovement movement;
    private CharacterCombat combat;
    private Animator anim;
    private Rigidbody2D rb;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
    }

    void OnEnable()
    {
        combat.SetDeadStatus(false);
    }

    private void HandleInput()
    {
        if (combat.IsDead && Input.GetKey(KeyCode.R))
        {
            Managers.Game.RestartGame();
        }
        else if (combat.IsDead) return;

        // 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        movement.SetMoveInput(moveInput);

        // 점프
        if (Input.GetKeyDown(KeyCode.W))
        {
            movement.RequestJump();
        }

        // 공격
        if (Input.GetMouseButtonDown(0) && combat.CanAttack && !movement.IsDodging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 attackDirection = (mousePos - transform.position).normalized;

            // 마우스 방향에 따라 캐릭터 Flip
            if (attackDirection.x > 0)
            {
                movement.FaceDirection(true);
            }
            else if (attackDirection.x < 0)
            {
                movement.FaceDirection(false);
            }

            // Flip 상태를 고려한 SlashEffect 회전
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
            if (!movement.IsFacingRight)
            {
                angle = 180 + angle;
            }
            slashEffectDegree.transform.rotation = Quaternion.Euler(0, 0, angle);

            combat.Attack(attackDirection);
            Managers.Sound.Play("PlayerAttack");

            // 공격 시 대시 이동
            StartCoroutine(AttackDashCo(attackDirection));
        }

        // 투사체
        if (Input.GetMouseButtonDown(1))
        {
            if (heldItem != null)
            {
                ThrowHeldItem();
            }
            else
            {
                TryPickupNearestItem();
            }
        }

        // 아랫점프
        if (Input.GetKey(KeyCode.S) && Input.GetButtonDown("Jump"))
        {
            movement.DropThroughPlatform();
        }
        // 구르기
        else if (movement.IsGrounded && Input.GetKey(KeyCode.S) && moveInput != 0)
        {
            movement.Dodge();
        }

        // 시간감속
        if (!combat.IsDead && Input.GetKey(KeyCode.LeftShift))
        {
            Managers.TimeSlow.SetTimeSlow(true);
        }
        else
        {
            Managers.TimeSlow.SetTimeSlow(false);
        }
    }

    void TryPickupNearestItem()
    {
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

        if (nearestItem != null)
        {
            heldItem = nearestItem;
            heldItem.Pickup(itemHoldPoint, this);
            Managers.Sound.Play("Pickup");
        }
    }

    void ThrowHeldItem()
    {
        if (heldItem == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mousePos - itemHoldPoint.position).normalized;

        heldItem.Throw(throwDirection);
        heldItem = null;
        Managers.Sound.Play("Throw");
    }

    private IEnumerator AttackDashCo(Vector2 direction)
    {
        // 이동 제어 차단
        movement.CanMove(false);

        // 마우스 방향으로 velocity 설정
        rb.velocity = new Vector2(direction.x * attackDashForce, direction.y * attackDashForce);
        // rb.velocity += Vector2.up * attackDashForce / 2;

        yield return new WaitForSeconds(attackDashTime);

        // 이동 제어 복구
        movement.CanMove(true);
    }

    private void Die()
    {
        if (!combat.IsDead)
        {
            combat.SetDeadStatus(true);
            StopAllCoroutines();
            Managers.TimeSlow.DeactivateSlowMotion();

            Managers.Sound.Play("PlayerDie");
            anim.SetTrigger("Die");

            // TODO: 게임 오버
            Managers.Game.GameOver();
        }
    }

    /// <summary>
    /// 데미지를 받았을 때
    /// </summary>
    public void TakeDamage(int damage)
    {
        Die();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, itemPickupRange);
    }

}