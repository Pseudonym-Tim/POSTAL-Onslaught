using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all enemy NPC's...
/// </summary>
public class Enemy : NPC
{
    [SerializeField] protected int maxHealth = 10;
    protected int currentHealth = 0;

    public void TakeDamage(DamageInfo damageInfo)
    {
        if(currentHealth > 0)
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
                Debug.Log($"[{ name }] took [{ damageInfo.damageAmount }] damage!");
            }
        }
    }

    protected virtual void OnTakeDamage(DamageInfo damageInfo)
    {

    }

    protected virtual void OnDeath()
    {

    }

    public bool IsAlive { get { return currentHealth > 0; } }
}
