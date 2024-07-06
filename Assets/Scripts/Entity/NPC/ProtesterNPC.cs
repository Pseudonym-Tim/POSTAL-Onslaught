using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Protester NPC that runs around randomly...
/// </summary>
public class ProtesterNPC : NPC
{
    [SerializeField] private AIState currentAIState = AIState.ROAMING;
    [SerializeField] private float roamRadius = 5f;
    [SerializeField] private float roamTimeMin = 0.75f;
    [SerializeField] private float roamTimeMax = 1.5f;
    [SerializeField] private float roamSpeedMin = 1.5f;
    [SerializeField] private float roamSpeedMax = 2.25f;
    private float roamTimer = 0;

    public override void OnNavmeshBuilt()
    {
        SetupNPCNavigation(0);
        BeginRoaming();
    }

    protected override void OnEntityUpdate()
    {
        if(NPCNavigation != null)
        {
            switch(currentAIState)
            {
                case AIState.ROAMING:

                    if(IsReadyForNextRoam)
                    {
                        NPCNavigation.NavMeshAgentSpeed = Random.Range(roamSpeedMin, roamSpeedMax);
                        Vector3 destinationPos = LevelNavmesher.GetRandomPosition(EntityPosition, roamRadius);
                        NPCNavigation.SetDestination(destinationPos);
                    }

                    break;
                case AIState.KNOCKBACK:
                    break;
                case AIState.DEAD:
                    break;
            }

            EntityAnim.SetBool("isMoving", NPCNavigation.IsMoving);

            // Flip the sprite based on move direction...
            if(NPCNavigation.NavMeshAgentVelocity.x != 0)
            {
                Vector2 agentVelocity = NPCNavigation.NavMeshAgentVelocity;
                npcGFX.flipX = agentVelocity.normalized.x < 0;
            }
        }
    }

    private void BeginRoaming()
    {
        NPCNavigation.StopMoving();
        roamTimer = Random.Range(roamTimeMin, roamTimeMax);
        NPCNavigation.NavMeshAgentSpeed = Random.Range(roamSpeedMin, roamSpeedMax);
        currentAIState = AIState.ROAMING;
    }

    protected override void OnKnockbackStart()
    {
        currentAIState = AIState.KNOCKBACK;
    }

    protected override void OnKnockbackEnd()
    {
        BeginRoaming();
    }

    protected override void OnDeath()
    {
        
    }

    protected override void OnTakeDamage(DamageInfo damageInfo)
    {
        // TODO: Begin running away from player...
    }

    private bool IsReadyForNextRoam
    {
        get
        {
            if(roamTimer > 0)
            {
                roamTimer = Mathf.Max(roamTimer - Time.deltaTime, 0);
            }
            else if(roamTimer <= 0 || NPCNavigation.IsDestinationReached)
            {
                roamTimer = Random.Range(roamTimeMin, roamTimeMax);
                return true;
            }

            return false;
        }
    }

    public enum AIState
    {
        ROAMING,
        DEAD,
        KNOCKBACK
    }
}
