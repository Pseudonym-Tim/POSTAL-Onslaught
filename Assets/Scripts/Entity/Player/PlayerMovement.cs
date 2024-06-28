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
    [SerializeField] private float acceleration = 10;
    [SerializeField] private ObstacleCheck obstacleCheck;
    private Player playerEntity;
    private Rigidbody2D playerRigidbody;
    private Vector2 targetVelocity;

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
        targetVelocity = new Vector2(moveX, moveY).normalized * movementSpeed;

        // Flip the sprite based on move direction...
        if(moveX != 0)
        {
            playerEntity.playerGFX.flipX = moveX < 0;
        }
    }

    private void FixedUpdate()
    {
        Vector2 adjustedVelocity = AdjustVelocityForObstacles(targetVelocity);
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
            Vector2 direction = targetVelocity.normalized * obstacleCheck.size * 0.5f;

            Gizmos.DrawWireCube(position, obstacleCheck.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(position + direction, obstacleCheck.size);
        }
    }

    public void UpdateGFXFlip(bool flipX)
    {
        playerEntity.playerGFX.flipX = flipX;
    }

    public bool IsPlayerFacingRight => !playerEntity.playerGFX.flipX;
    public bool IsMoving => playerRigidbody.velocity.magnitude > 0.1f;
}
