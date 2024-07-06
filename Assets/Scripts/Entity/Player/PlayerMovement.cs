using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to player movement...
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed = 5;
    [SerializeField] private ObstacleCheck obstacleCheck;
    private Player playerEntity;
    private Rigidbody2D playerRigidbody;
    private Vector2 moveVelocity;
    private Vector2 knockbackVelocity = Vector2.zero;
    private float knockbackTimer = 0;

    [System.Serializable]
    public class ObstacleCheck
    {
        public Vector2 size = new Vector2(1, 1);
        public Vector2 offset = new Vector2(0, -0.5f);
        public LayerMask layerMask = -1;
    }

    private void Awake()
    {
        playerEntity = GetComponent<Player>();
        playerRigidbody = GetComponent<Rigidbody2D>();
    }

    public void UpdateMovement()
    {
        float moveX = PlayerInput.GetAxisRaw("Horizontal");
        float moveY = PlayerInput.GetAxisRaw("Vertical");
        moveVelocity = new Vector2(moveX, moveY).normalized * movementSpeed;

        // Flip the sprite based on move direction...
        if(moveX != 0)
        {
            playerEntity.playerGFX.flipX = moveX < 0;
        }
    }

    public void ApplyKnockback(KnockbackInfo knockbackInfo, Vector2 origin = default)
    {
        if(!IsInKnockback && origin != default)
        {
            Vector2 knockbackDirection = playerEntity.CenterOfMass - origin;
            knockbackVelocity = knockbackDirection.normalized * knockbackInfo.force;
            knockbackTimer = knockbackInfo.duration;
            IsInKnockback = true;
        }
    }

    private void EndKnockback()
    {
        IsInKnockback = false;
        knockbackVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if(IsInKnockback)
        {
            knockbackTimer -= Time.fixedDeltaTime;

            if(knockbackTimer <= 0)
            {
                EndKnockback();
            }
        }

        // Combine knockback velocity with move velocity...
        Vector2 velocityToApply = moveVelocity + knockbackVelocity;
        Vector2 adjustedVelocity = AdjustVelocityForObstacles(velocityToApply);
        playerRigidbody.velocity = adjustedVelocity;
    }

    private Vector2 AdjustVelocityForObstacles(Vector2 velocity)
    {
        if(velocity == Vector2.zero)
        {
            return Vector2.zero;
        }

        Vector2 position = (Vector2)playerEntity.EntityPosition + obstacleCheck.offset;
        Vector2 adjustedVelocity = velocity;

        // Dictionary to store velocity adjustments...
        Dictionary<string, Vector2> velocityAdjustments = new Dictionary<string, Vector2>
        {
            { "horizontal", new Vector2(velocity.x, 0) },
            { "vertical", new Vector2(0, velocity.y) }
        };

        foreach(string direction in velocityAdjustments.Keys.ToList())
        {
            Vector2 checkPosition = position + velocityAdjustments[direction].normalized * obstacleCheck.size * 0.5f;

            if(Physics2D.OverlapBox(checkPosition, obstacleCheck.size, 0, obstacleCheck.layerMask) != null)
            {
                if(direction == "horizontal")
                {
                    adjustedVelocity.x = 0;
                }
                else if(direction == "vertical")
                {
                    adjustedVelocity.y = 0;
                }
            }
        }

        return adjustedVelocity;
    }

    private void OnDrawGizmos()
    {
        if(Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector2 position = (Vector2)playerEntity.EntityPosition + obstacleCheck.offset;
            Vector2 direction = moveVelocity.normalized * obstacleCheck.size * 0.5f;

            Gizmos.DrawWireCube(position, obstacleCheck.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(position + direction, obstacleCheck.size);
        }
    }

    public void UpdateGFXFlip(bool flipX)
    {
        playerEntity.playerGFX.flipX = flipX;
    }

    public bool IsInKnockback { get; set; } = false;
    public bool IsPlayerFacingRight => !playerEntity.playerGFX.flipX;
    public bool IsMoving => playerRigidbody.velocity.magnitude > 0.1f;
}
