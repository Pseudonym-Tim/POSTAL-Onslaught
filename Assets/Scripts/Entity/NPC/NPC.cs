using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all NPC's...
/// </summary>
public class NPC : Entity
{
    [SerializeField] protected int maxHealth = 10;
    protected int currentHealth = 0;
    [SerializeField] protected float moveSpeed = 10;
    protected Player playerEntity;
    protected SpriteRenderer npcGFX;

    public override void OnLevelGenerated()
    {
        playerEntity = GetPlayer();
    }

    protected void SetupNPCNavigation(int agentID)
    {
        NPCNavigation = EntityObject.AddComponent<NPCNavigation>();
        NPCNavigation.Setup(this, agentID);
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
                Debug.Log($"[{ name }] took [{ damageInfo.damageAmount }] damage!");
            }
        }
    }

    protected Player GetPlayer()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        return levelManager.GetEntity<Player>();
    }

    protected virtual void OnTakeDamage(DamageInfo damageInfo)
    {
        
    }

    protected virtual void OnDeath()
    {
        
    }

    public NPCNavigation NPCNavigation { get; private set; }
    public override Vector3 CenterOfMass => EntityPosition;
    public virtual bool IsInvulnerable { get; set; } = false; 
    public bool IsAlive { get { return currentHealth > 0; } }
}
