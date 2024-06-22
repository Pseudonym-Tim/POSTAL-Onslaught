using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player entity related stuff...
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class Player : Entity
{
    [SerializeField] protected int maxHealth = 100;
    protected int currentHealth = 0;
    private PlayerMovement playerMovement;

    public override void OnEntitySpawn()
    {
        playerMovement = GetComponent<PlayerMovement>();
        PlayerInput.InputEnabled = true;
        SetupEntityAnim();
        EntityAnim.Play("Idle");
        currentHealth = maxHealth;
    }

    protected override void OnEntityUpdate()
    {
        playerMovement.UpdateMovement();
        EntityAnim.SetBool("isMoving", playerMovement.IsMoving);
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

    }

    private void OnDeath()
    {
        Debug.Log("Player died!");
    }

    public virtual bool IsInvulnerable { get; set; } = false;
    public PlayerCamera PlayerCamera { get; set; } = null;
}
