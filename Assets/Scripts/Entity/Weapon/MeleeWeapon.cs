using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

/// <summary>
/// Close quarters melee weapon...
/// </summary>
public class MeleeWeapon : Weapon
{
    [SerializeField] private float attackRate = 0.2f;
    [SerializeField] private float attackRange = 1f;
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

        if(playerEntity.IsAlive && PlayerInput.InputEnabled)
        {
            if(weaponManager.IsPlayerArmed)
            {
                UpdateAttacking();
            }
        }
    }

    private void UpdateAttacking()
    {
        if(attackDelayTimer > 0)
        {
            attackDelayTimer -= Time.deltaTime;
            return;
        }

        if(weaponManager.IsAttackingAllowed && PlayerInput.IsButtonHeld("Attack"))
        {
            OnWeaponAttack();
            attackDelayTimer = attackRate;
        }
    }

    public virtual void OnWeaponAttack()
    {
        swingRight = !swingRight;
        string swingAnimation = swingRight ? "SwingRight" : "SwingLeft";
        EntityAnim.Play(swingAnimation);
        SpawnSplashFX();
        ApplySwingPush();
    }

    private void ApplySwingPush()
    {
        // Apply knockback in the direction of the player's swing...
        // In the future it would be better to not use the knockback function...
        float localScaleX = weaponManager.AimParent.localScale.x;
        float knockbackDirection = localScaleX > 0 ? -1 : 1;

        // Define the knockback information...
        KnockbackInfo knockbackInfo = new KnockbackInfo()
        {
            duration = 0.25f,
            force = -(2.5f * knockbackDirection)
        };

        // Calculate the knockback origin based on the attack direction...
        Vector3 aimParentOrigin = weaponManager.AimParent.position;
        Vector3 knockbackDirectionVector = weaponManager.AimParent.right * knockbackDirection;
        Vector2 knockbackOrigin = aimParentOrigin + knockbackDirectionVector;

        // Apply the knockback to the player to move them into their melee attack...
        playerEntity.ApplyKnockback(knockbackInfo, knockbackOrigin);
    }

    private void SpawnSplashFX()
    {
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        GameObject slashFX = Instantiate(prefabDatabase.GetPrefab("SlashFX"), weaponManager.AimParent);
        Vector2 slashPos = weaponManager.AimParent.localPosition;
        slashPos.y = 0;
        slashFX.transform.localPosition = slashPos + Vector2.right * 1.25f;
        SpriteRenderer slashGFX = slashFX.GetComponentInChildren<SpriteRenderer>();
        slashGFX.flipY = !swingRight;
        Destroy(slashFX, 0.5f);
    }

    // NOTE: Called via anim event...
    public void CheckWeaponHit()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        List<NPC> npcList = levelManager.GetEntities<NPC>(CenterOfMass, attackRange);
        NPC closestNPC = GetClosestNPCWithinAngle(npcList);

        if(closestNPC != null)
        {
            DamageInfo damageInfo = new DamageInfo()
            {
                damageOrigin = CenterOfMass,
                damageAmount = Random.Range(damageMin, damageMax),
                attackerEntity = playerEntity,
            };

            // Hurt, knockback, screenshake...
            closestNPC.TakeDamage(damageInfo);
            closestNPC.ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
            CameraShaker.Shake(hitShakeInfo);
        }
    }

    private NPC GetClosestNPCWithinAngle(List<NPC> npcList)
    {
        NPC closestNPC = null;
        float closestDistanceSqr = float.MaxValue;

        Vector2 currentPosition = playerEntity.CenterOfMass;
        Vector2 forward = weaponManager.AimParent.localScale.y < 0 ? Vector2.left : Vector2.right;

        foreach(NPC npc in npcList)
        {
            Vector2 directionToNPC = (Vector2)npc.CenterOfMass - currentPosition;
            float angleToNPC = Vector2.Angle(forward, directionToNPC);

            if(angleToNPC <= attackAngle)
            {
                float distanceSqr = directionToNPC.sqrMagnitude;
                if(distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestNPC = npc;
                }
            }
        }

        return closestNPC;
    }

    protected override void OnDrawEntityGizmos()
    {
        // Draw the attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerEntity.CenterOfMass, attackRange);

        // Draw the melee angle
        Gizmos.color = Color.yellow;
        Vector2 forward = weaponManager.AimParent.localScale.y < 0 ? Vector2.left : Vector2.right;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, attackAngle) * forward * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, 0, -attackAngle) * forward * attackRange;

        Gizmos.DrawLine(playerEntity.CenterOfMass, (Vector3)playerEntity.CenterOfMass + rightBoundary);
        Gizmos.DrawLine(playerEntity.CenterOfMass, (Vector3)playerEntity.CenterOfMass + leftBoundary);
    }

    public override bool IsMeleeWeapon => true;
}
