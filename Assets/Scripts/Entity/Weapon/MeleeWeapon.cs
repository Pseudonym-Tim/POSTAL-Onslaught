using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Close quarters melee weapon...
/// </summary>
public class MeleeWeapon : Weapon
{
    private const float SLASH_FX_LIFETIME = 1f;
    private const float SLASH_FX_OFFSET = 1.25f;

    public float attackRange = 1f;
    [SerializeField] private float attackRate = 0.2f;
    [SerializeField] private float attackAngle = 45f;
    [SerializeField] private int damageMin = 3, damageMax = 6;
    [SerializeField] private CameraShakeInfo hitShakeInfo;
    private float attackDelayTimer = 0;
    private bool swingRight = false;

    public override void OnWeaponSelected()
    {
        string swingAnimation = swingRight ? "SwingRight" : "SwingLeft";
        EntityAnim.Play(swingAnimation, 1); // Play the animation, skip to the end...
    }

    protected override void OnEntityUpdate()
    {
        if(!PlayerInput.InputEnabled)
        {
            attackDelayTimer = INPUT_DELAY;
        }

        if(IsOwnerAlive() && PlayerInput.InputEnabled)
        {
            if(weaponManager.WeaponCount > 0)
            {
                UpdateAttacking();
            }
        }
    }

    private void UpdateAttacking()
    {
        if(IsOwnerPlayer())
        {
            bool canAttack = weaponManager.IsAttackingAllowed;
            bool gotAttackInput = PlayerInput.IsButtonHeld("Attack");

            if(attackDelayTimer > 0) { attackDelayTimer -= Time.deltaTime; }
            else if(attackDelayTimer <= 0 && gotAttackInput && canAttack)
            {
                OnWeaponAttack();
                attackDelayTimer = attackRate;
            }
        }

        if(IsOwnerNPC() && weaponManager.IsAttackingAllowed)
        {
            if(attackDelayTimer > 0) { attackDelayTimer -= Time.deltaTime; }
            else if(attackDelayTimer <= 0)
            {
                OnWeaponAttack();
                attackDelayTimer = attackRate;
            }
        }
    }

    public virtual void OnWeaponAttack()
    {
        swingRight = !swingRight;
        PlaySwingAnimation();
        SpawnSlashFX();
    }

    private void SpawnSlashFX()
    {
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        GameObject slashFX = Instantiate(prefabDatabase.GetPrefab("SlashFX"), weaponManager.AimParent);
        Vector2 slashPos = weaponManager.AimParent.localPosition;
        slashPos.y = 0;
        slashFX.transform.localPosition = slashPos + Vector2.right * SLASH_FX_OFFSET;
        SpriteRenderer slashGFX = slashFX.GetComponentInChildren<SpriteRenderer>();
        slashGFX.flipY = !swingRight;
        Destroy(slashFX, SLASH_FX_LIFETIME);
    }

    // NOTE: Called via anim event...
    public void CheckWeaponHit()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();

        if(IsOwnerPlayer())
        {
            HandlePlayerHit(levelManager);
        }
        else if(IsOwnerNPC())
        {
            HandleNPCHit(levelManager);
        }
    }

    private void HandlePlayerHit(LevelManager levelManager)
    {
        List<NPC> npcList = levelManager.GetEntities<NPC>(Player.CenterOfMass, attackRange);
        NPC closestNPC = GetClosestNPCWithinAngle(npcList);

        if(closestNPC != null)
        {
            DamageInfo damageInfo = new DamageInfo()
            {
                damageOrigin = Player.CenterOfMass,
                damageAmount = Random.Range(damageMin, damageMax),
                attackerEntity = ownerEntity,
            };

            closestNPC.TakeDamage(damageInfo);
            closestNPC.ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
            CameraShaker.Shake(hitShakeInfo);
        }
    }

    private void HandleNPCHit(LevelManager levelManager)
    {
        Player playerEntity = levelManager.GetEntity<Player>();

        if(playerEntity != null && IsPlayerWithinAttackAngle(playerEntity))
        {
            DamageInfo damageInfo = new DamageInfo()
            {
                damageOrigin = NPC.CenterOfMass,
                damageAmount = Random.Range(damageMin, damageMax),
                attackerEntity = ownerEntity,
            };

            playerEntity.TakeDamage(damageInfo);
            playerEntity.ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
        }
    }

    private bool IsPlayerWithinAttackAngle(Player playerEntity)
    {
        Vector2 currentPosition = NPC.CenterOfMass;
        Vector2 forward = GetAttackDirection();
        Vector2 directionToPlayer = playerEntity.CenterOfMass - currentPosition;
        float angleToPlayer = Vector2.Angle(forward, directionToPlayer);

        return angleToPlayer <= attackAngle && Vector2.Distance(playerEntity.CenterOfMass, NPC.CenterOfMass) < attackRange;
    }

    private NPC GetClosestNPCWithinAngle(List<NPC> npcList)
    {
        NPC closestNPC = null;
        float closestDistanceSqr = float.MaxValue;

        Vector2 currentPosition = Player.CenterOfMass;
        Vector2 forward = GetAttackDirection();

        foreach(NPC npcEntity in npcList)
        {
            Vector2 directionToNPC = npcEntity.CenterOfMass - currentPosition;
            float angleToNPC = Vector2.Angle(forward, directionToNPC);

            if(angleToNPC <= attackAngle)
            {
                float distanceSqr = directionToNPC.sqrMagnitude;
                if(distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNPC = npcEntity;
                }
            }
        }

        return closestNPC;
    }

    private Vector2 GetAttackDirection()
    {
        return weaponManager.AimParent.localScale.y < 0 ? Vector2.left : Vector2.right;
    }

    private void PlaySwingAnimation()
    {
        string swingAnimation = swingRight ? "SwingRight" : "SwingLeft";
        EntityAnim.Play(swingAnimation);
    }

    protected override void OnDrawEntityGizmos()
    {
        DrawAttackRangeGizmo();

        Vector2 forward = GetAttackDirection();
        Vector3 rightBoundary = Quaternion.Euler(0, 0, attackAngle) * forward * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, -attackAngle) * forward * attackRange;

        Gizmos.color = Color.yellow;

        if(IsOwnerPlayer())
        {
            Gizmos.DrawLine(Player.CenterOfMass, (Vector3)Player.CenterOfMass + rightBoundary);
            Gizmos.DrawLine(Player.CenterOfMass, (Vector3)Player.CenterOfMass + leftBoundary);
        }
        else if(IsOwnerNPC())
        {
            Gizmos.DrawLine(NPC.CenterOfMass, (Vector3)NPC.CenterOfMass + rightBoundary);
            Gizmos.DrawLine(NPC.CenterOfMass, (Vector3)NPC.CenterOfMass + leftBoundary);
        }
    }

    private void DrawAttackRangeGizmo()
    {
        Gizmos.color = Color.red;

        if(IsOwnerPlayer())
        {
            Gizmos.DrawWireSphere(Player.CenterOfMass, attackRange);
        }
        else if(IsOwnerNPC())
        {
            Gizmos.DrawWireSphere(NPC.CenterOfMass, attackRange);
        }
    }
}