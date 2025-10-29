using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 리팩토링된 PlayerController - CharacterMovement와 CharacterCombat을 사용
/// </summary>
public class PlayerController : MonoBehaviour, IDamageable
{
    [Header("Item System")]
    [SerializeField] private Transform itemHoldPoint;
    [SerializeField] private float itemPickupRange = 1f;
    [SerializeField] private LayerMask throwableItemLayer;
    private ThrowableItem heldItem = null;

    private CharacterMovement movement;
    private CharacterCombat combat;
    private Animator anim;

    private bool isDead = false;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        combat = GetComponent<CharacterCombat>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // 이동
        float moveInput = Input.GetAxisRaw("Horizontal");
        movement.SetMoveInput(moveInput);

        // 점프
        if (Input.GetKeyDown(KeyCode.W))
        {
            movement.RequestJump();
        }

        // 공격
        if (Input.GetMouseButtonDown(0) && combat.CanAttack)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 attackDirection = (mousePos - transform.position).normalized;
            combat.Attack(attackDirection);
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
        Debug.Log($"pick up {colliders}");
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
        }
    }

    void ThrowHeldItem()
    {
        if (heldItem == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mousePos - itemHoldPoint.position).normalized;

        heldItem.Throw(throwDirection);
        heldItem = null;
    }

    private void Die()
    {
        if (!isDead)
        {
            StopAllCoroutines();

            Debug.Log("플레이어 사망");
            isDead = true;
            anim.SetTrigger("Die");

            // this.enabled = false;

            // TODO: 게임 오버
        }
    }

    // Animation Event에서 호출 (CharacterCombat으로 전달)
    public void OnSlashFrame()
    {
        combat.OnSlashFrame();
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