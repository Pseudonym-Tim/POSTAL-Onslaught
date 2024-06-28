using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player entity related stuff...
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class Player : Entity
{
    private const float HURT_FLASH_TIME = 0.1f;
    [SerializeField] protected int maxHealth = 10;
    public SpriteRenderer playerGFX;
    protected int currentHealth = 0;
    private PlayerMovement playerMovement;
    private PlayerHUD playerHUD;

    public override void OnEntitySpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        WeaponManager = GetComponentInChildren<WeaponManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        PlayerInput.InputEnabled = true;
        SetupEntityAnim();
        EntityAnim.Play("Idle");
        currentHealth = maxHealth;
    }

    protected override void OnEntityUpdate()
    {
        playerMovement.UpdateMovement();
        EntityAnim.SetBool("isMoving", playerMovement.IsMoving);

        // TEST! Remove later...
        if(Input.GetKeyDown(KeyCode.H))
        {
            DamageInfo damageInfo = new DamageInfo()
            {
                attackerEntity = this,
                damageAmount = 1
            };

            TakeDamage(damageInfo);
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if(currentHealth > 0 && !IsInvulnerable)
        {
            currentHealth -= damageInfo.damageAmount;
            OnTakeDamage(damageInfo);

            // We died from that hit?
            if(currentHealth <= 0)
            {
                OnDeath();
                return;
            }
            else
            {
                Debug.Log($"[{name}] took [{damageInfo.damageAmount}] damage!");
            }
        }
    }

    private void OnTakeDamage(DamageInfo damageInfo)
    {
        if(currentHealth < 0) { currentHealth = 0; }
        HurtFlash.ApplyHurtFlash(this, HURT_FLASH_TIME);
        playerHUD.UpdateHealthIndicator(currentHealth, maxHealth);
    }

    private void OnDeath()
    {
        Debug.Log("Player died!");
    }

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CenterOfMass, 0.25f);
    }

    public override Vector3 CenterOfMass => EntityPosition + Vector3.up * playerGFX.size.y / 2;
    public bool IsAlive { get { return currentHealth > 0; } }
    public bool IsInvulnerable { get; set; } = false;
    public PlayerCamera PlayerCamera { get; set; } = null;
    public WeaponManager WeaponManager { get; set; } = null;
}
