using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Angry civilian enemy NPC...
/// </summary>
public class CivilianEnemyNPC : EnemyNPC
{
    private const float STOPPING_DISTANCE = 2.0f;

    [SerializeField] private AIState currentAIState = AIState.IDLE;
    [SerializeField] private float roamRadius = 5f;
    [SerializeField] private float roamTimeMin = 0.75f;
    [SerializeField] private float roamTimeMax = 1.5f;
    [SerializeField] private float roamSpeedMin = 1.5f;
    [SerializeField] private float roamSpeedMax = 2.25f;
    [SerializeField] private float attackDurationMin = 1.0f;
    [SerializeField] private float attackDurationMax = 3.0f;
    [SerializeField] private float chaseRadius = 6f;
    [SerializeField] private float chaseTimeMin = 2.0f;
    [SerializeField] private float chaseTimeMax = 4.0f;
    private float attackTimer = 0;
    private float roamTimer = 0;
    private float chaseTimer = 0;
    private Vector3 chaseDestination;
    private bool isNewDestination = false;

    public override void OnEntitySpawn()
    {
        playerEntity = GetPlayer();

        if(!weaponManager)
        {
            SetupWeapon();
        }

        if(LevelNavmesher.IsNavmeshBuilt && NPCNavigation == null)
        {
            SetupNPCNavigation(0);
            BeginRoaming();
        }
    }

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();
        if(!weaponManager) { SetupWeapon(); }
    }

    public override void OnNavmeshBuilt()
    {
        if(NPCNavigation == null)
        {
            SetupNPCNavigation(0);
            BeginRoaming();
        }
    }

    protected override void OnEntityUpdate()
    {
        if(!playerEntity.IsAlive)
        {
            currentAIState = AIState.IDLE;
        }

        switch(currentAIState)
        {
            case AIState.IDLE:
                break;
            case AIState.ROAMING:
                UpdateRoaming();
                break;
            case AIState.CHASING:
                UpdateChasing();
                break;
            case AIState.ATTACKING:
                UpdateAttacking();
                break;
            case AIState.KNOCKBACK:
                break;
            case AIState.DEAD:
                break;
        }

        EntityAnim.SetBool("isMoving", NPCNavigation.IsMoving);

        if(NPCNavigation.NavMeshAgentVelocity.x != 0)
        {
            Vector2 agentVelocity = NPCNavigation.NavMeshAgentVelocity;
            npcGFX.flipX = agentVelocity.normalized.x < 0;
        }
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
        currentAIState = AIState.DEAD;
    }

    protected override void OnTakeDamage(DamageInfo damageInfo)
    {
        BeginChasing();
    }

    private void BeginRoaming()
    {
        ResetNavigation();
        roamTimer = Random.Range(roamTimeMin, roamTimeMax);
        NPCNavigation.NavMeshAgentSpeed = Random.Range(roamSpeedMin, roamSpeedMax);
        currentAIState = AIState.ROAMING;
        weaponManager.IsAttackingAllowed = false;
    }

    private void BeginChasing()
    {
        ResetNavigation();
        chaseTimer = Random.Range(chaseTimeMin, chaseTimeMax);
        currentAIState = AIState.CHASING;
        weaponManager.IsAttackingAllowed = false;
    }

    private void BeginAttacking()
    {
        if(IsVisibleToPlayer)
        {
            attackTimer = Random.Range(attackDurationMin, attackDurationMax);
            currentAIState = AIState.ATTACKING;
            weaponManager.IsAttackingAllowed = true;
        }
    }

    private void UpdateChasing()
    {
        if(IsVisibleToPlayer)
        {
            if(weaponManager.SelectedWeapon.IsMeleeWeapon)
            {
                BeginAttacking();
                return;
            }
            else if(isNewDestination && !NPCNavigation.IsMoving && NPCNavigation.IsDestinationReached)
            {
                BeginAttacking();
                return;
            }

            if(chaseTimer <= 0 || !isNewDestination)
            {
                UpdateChaseDestination();
            }
            else
            {
                chaseTimer -= Time.deltaTime;
            }
        }
        else
        {
            BeginRoaming();
        }
    }

    private void UpdateRoaming()
    {
        if(IsVisibleToPlayer)
        {
            BeginChasing();
            return;
        }

        if(IsReadyForNextRoam())
        {
            NPCNavigation.NavMeshAgentSpeed = Random.Range(roamSpeedMin, roamSpeedMax);
            Vector3 roamDestination = LevelNavmesher.GetRandomPosition(EntityPosition, roamRadius);
            NPCNavigation.SetDestination(roamDestination);
        }
    }

    private void UpdateAttacking()
    {
        if(weaponManager.SelectedWeapon.IsMeleeWeapon)
        {
            UpdateMeleeAttacking();
        }
        else
        {
            if(attackTimer > 0)
            {
                attackTimer -= Time.deltaTime;
            }
            else
            {
                BeginChasing();
            }
        }
    }

    private void UpdateMeleeAttacking()
    {
        NPCNavigation.SetDestination(playerEntity.EntityPosition, STOPPING_DISTANCE);
        float distanceToPlayer = Vector2.Distance(playerEntity.CenterOfMass, EntityPosition);
        MeleeWeapon meleeWeapon = (MeleeWeapon)weaponManager.SelectedWeapon;
        weaponManager.IsAttackingAllowed = distanceToPlayer <= meleeWeapon.attackRange;
    }

    private bool IsReadyForNextRoam()
    {
        if(roamTimer > 0)
        {
            roamTimer -= Time.deltaTime;
            return false;
        }
        else
        {
            roamTimer = Random.Range(roamTimeMin, roamTimeMax);
            return true;
        }
    }

    private void ResetNavigation()
    {
        NPCNavigation.StopMoving();
        isNewDestination = false;
    }

    private void UpdateChaseDestination()
    {
        chaseDestination = LevelNavmesher.GetRandomPosition(playerEntity.EntityPosition, chaseRadius);
        float stoppingDistance = weaponManager.SelectedWeapon.IsMeleeWeapon ? 1 : 0;

        if(weaponManager.SelectedWeapon.IsMeleeWeapon)
        {
            chaseDestination = playerEntity.EntityPosition;
        }

        NPCNavigation.SetDestination(chaseDestination, stoppingDistance);
        chaseTimer = Random.Range(chaseTimeMin, chaseTimeMax);
        isNewDestination = true;
    }

    public enum AIState
    {
        IDLE,
        ROAMING,
        CHASING,
        ATTACKING,
        KNOCKBACK,
        DEAD
    }
}