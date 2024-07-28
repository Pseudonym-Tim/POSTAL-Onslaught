using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Ranged weapon...
/// </summary>
public class RangedWeapon : Weapon
{
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private bool isAutomatic = false;
    [SerializeField] private int damageMin = 3, damageMax = 6;
    [SerializeField] private int numberOfShots = 1;
    [SerializeField] private float bulletSpreadAmount = 0;
    [SerializeField] private CameraShakeInfo shootCameraShakeInfo;
    [SerializeField] private WindowShakeInfo shootWindowShakeInfo;
    [SerializeField] private KnockbackInfo shootKnockbackInfo;
    [SerializeField] private ShotTrail shotTrailPrefab;
    [SerializeField] private Animator muzzleFlashAnimator;
    private float shotDelayTimer = 0;

    protected override void OnEntityUpdate()
    {
        if(!PlayerInput.InputEnabled)
        {
            shotDelayTimer = INPUT_DELAY;
        }

        if(IsOwnerAlive() && PlayerInput.InputEnabled)
        {
            if(weaponManager.WeaponCount > 0)
            {
                UpdateShooting();
            }
        }
    }

    private void UpdateShooting()
    {
        if(IsOwnerPlayer())
        {
            bool canShoot = !IsShootOriginObstructed && weaponManager.IsAttackingAllowed;
            bool gotAttackInput = isAutomatic ? PlayerInput.IsButtonHeld("Attack") : PlayerInput.IsButtonPressed("Attack");

            if(shotDelayTimer > 0) { shotDelayTimer -= Time.deltaTime; }
            else if(shotDelayTimer <= 0 && gotAttackInput && canShoot)
            {
                OnFireWeapon(); // Fire the weapon...
                shotDelayTimer = fireRate; // Delay the next shot using the fire rate...
            }
        }

        if(IsOwnerNPC() && weaponManager.IsAttackingAllowed)
        {
            if(shotDelayTimer > 0) { shotDelayTimer -= Time.deltaTime; }
            else if(shotDelayTimer <= 0)
            {
                OnFireWeapon(); // Fire the weapon...
                shotDelayTimer = fireRate; // Delay the next shot using the fire rate...
            }
        }
    }

    public virtual void OnFireWeapon()
    {
        for(int i = 0; i < numberOfShots; i++)
        {
            bool hasBulletSpread = bulletSpreadAmount > 0;
            float spreadAngle = Random.Range(-bulletSpreadAmount / 2, bulletSpreadAmount / 2);
            Vector2 spreadShotDir = Quaternion.Euler(0, 0, spreadAngle) * ShootOriginTransform.right;
            Vector2 shotDir = hasBulletSpread ? spreadShotDir : ShootOriginTransform.right;
            RaycastHit2D shootHitInfo = ShootRaycast(default, shotDir, numberOfShots);
            CreateShotTrail(shotTrailPrefab, shootHitInfo, spreadShotDir);
        }

        EntityAnim.Play("Shoot");
        muzzleFlashAnimator.Play("Blast");

        if(IsOwnerPlayer())
        {
            Player.ApplyKnockback(shootKnockbackInfo, ShootOriginTransform.position);
            CameraShaker.Shake(shootCameraShakeInfo);
            WindowShaker.Shake(shootWindowShakeInfo);
        }

        CheckHouseDisturbance();
    }

    protected virtual RaycastHit2D ShootRaycast(Vector2 origin, Vector2 direction, int numberOfShots)
    {
        // Set defaults if parameters are not provided...
        origin = origin == Vector2.zero ? ShootOriginTransform.position : origin;
        direction = direction == Vector2.zero ? ShootOriginTransform.right : direction;
        LayerMask layerMask = IsOwnerPlayer() ? LayerManager.Masks.SHOOTABLE_NPC : LayerManager.Masks.SHOOTABLE_PLAYER;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Infinity, layerMask);

        if(hit && hit.collider)
        {
            Entity entityHit = hit.collider.GetComponentInParent<Entity>();

            if(entityHit != ownerEntity)
            {
                DamageInfo damageInfo = new DamageInfo()
                {
                    damageOrigin = hit.point,
                    damageAmount = Random.Range(damageMin, damageMax) / numberOfShots,
                    attackerEntity = ownerEntity,
                };

                if(IsOwnerNPC() && entityHit is Player playerHit)
                {
                    playerHit.TakeDamage(damageInfo);
                    playerHit.ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
                }
                else if(IsOwnerPlayer() && entityHit is NPC npcHit)
                {
                    npcHit.TakeDamage(damageInfo);
                    npcHit.ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
                }

                // Hit visualization...
                Debug.DrawLine(origin, hit.point, Color.red, 0.5f);
            }
        }

        return hit;
    }

    protected void CreateShotTrail(ShotTrail shotTrailPrefab, RaycastHit2D shootHitInfo, Vector2 shotDir)
    {
        Vector2 spawnPos = ShootOriginTransform.position;
        Quaternion spawnRot = ShootOriginTransform.rotation;
        ShotTrail spawnedShotTrail = Instantiate(shotTrailPrefab, spawnPos, spawnRot);
        spawnedShotTrail.name = shotTrailPrefab.name;

        // If we don't hit anything, then the end position is just set to be pretty far off-screen!
        const float OFFSCREEN_DIST = 69; // Ha-ha, funny number!
        Vector2 offscreenEndPos = ShootOriginTransform.position + (Vector3)shotDir * OFFSCREEN_DIST;
        spawnedShotTrail.EndPosition = shootHitInfo ? shootHitInfo.point : offscreenEndPos; // Set end point destination...
    }

    public bool IsShootOriginObstructed
    {
        get
        {
            Vector2 startPos = weaponManager.AimParent.position;
            Vector2 endPos = ShootOriginTransform.position;
            LayerMask layerMask = IsOwnerPlayer() ? LayerManager.Masks.SHOOTABLE_NPC : LayerManager.Masks.SHOOTABLE_PLAYER;
            RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, layerMask);
            Debug.DrawLine(startPos, endPos, Color.yellow);
            return hit;
        }
    }

    protected Transform ShootOriginTransform
    {
        get
        {
            Transform originTransform = EntityTransform.Find("Root").Find("ShootOrigin");
            if(!originTransform) { Debug.LogWarning($"ShootOrigin not found on: [{ weaponName }]!"); }
            return originTransform;
        }
    }

    protected override void OnDrawEntityGizmos()
    {
        if(ShootOriginTransform)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(ShootOriginTransform.position, ShootOriginTransform.right * 25);
        }
    }
}
