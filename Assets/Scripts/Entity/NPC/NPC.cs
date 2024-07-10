using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all NPC's...
/// </summary>
public class NPC : Entity
{
    public static event System.Action OnNPCKilled;

    private const float HURT_FLASH_TIME = 0.1f;
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected float moveSpeed = 10;
    [SerializeField] protected KnockbackInfo hurtKnockbackInfo;
    public AnimationClip killerAnimation;
    protected int currentHealth = 0;
    protected Player playerEntity;
    protected SpriteRenderer npcGFX;
    protected Vector2 knockbackVelocity = Vector2.zero;
    protected LevelManager levelManager;

    public override void OnEntityAwake()
    {
        npcGFX = GetComponentInChildren<SpriteRenderer>();
        levelManager = FindFirstObjectByType<LevelManager>();
        SetupEntityAnim();
        EntityAnim.Play("Idle");
        currentHealth = maxHealth;
    }

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
            ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
            HurtFlash.ApplyHurtFlash(this, HURT_FLASH_TIME);

            // We died from that hit?
            if(currentHealth <= 0)
            {
                currentHealth = 0;
                CheckDropItem();
                AddPlayerScore();
                UpdatePlayerKill();
                OnNPCKilled?.Invoke();
                OnDeath();
                levelManager.RemoveEntity(this, 0.1f); // TODO: Add actual death animation or effect instead...
                return;
            }
            else
            {
                Debug.Log($"[{ name }] took [{ damageInfo.damageAmount }] damage!");
            }
        }
    }

    private void UpdatePlayerKill()
    {
        int currentKills = ++GameManager.GameStats.CurrentKills;
        PlayerHUD playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        playerHUD.UpdateKilled(currentKills);
    }

    public void CheckDropItem()
    {
        if(Random.value < (float)EntityData.jsonData["dropItemChance"])
        {
            ItemPickup itemPickup = ItemPickup.Create(CenterOfMass);
            JArray itemPool = (JArray)EntityData.jsonData["itemPool"];
            itemPickup.SetItemPool(itemPool);
        }
    }

    public void ApplyKnockback(KnockbackInfo knockbackInfo, Vector2 origin = default)
    {
        if(!IsInKnockback && origin != default)
        {
            Vector2 knockbackDirection = CenterOfMass - origin;
            knockbackVelocity = knockbackDirection.normalized * knockbackInfo.force;
            StartCoroutine(KnockbackRoutine(knockbackInfo.duration));
        }
    }

    private IEnumerator KnockbackRoutine(float duration)
    {
        IsInKnockback = true;
        OnKnockbackStart();

        float knockbackTimer = 0f;
        Vector3 initialVelocity = knockbackVelocity;

        // Apply knockback while the timer is within the specified duration...
        while(knockbackTimer < duration)
        {
            NPCNavigation.StopMoving();
            float lerpFactor = knockbackTimer / duration;
            Vector3 currentKnockbackVelocity = Vector3.Lerp(initialVelocity, Vector3.zero, lerpFactor);
            NPCNavigation.MoveAgent(currentKnockbackVelocity * Time.deltaTime);
            knockbackTimer += Time.deltaTime;
            yield return null;
        }

        knockbackVelocity = Vector3.zero;
        IsInKnockback = false;
        OnKnockbackEnd();
    }

    protected virtual void OnKnockbackStart()
    {

    }

    protected virtual void OnKnockbackEnd()
    {

    }

    public void AddPlayerScore()
    {
        int scoreToAdd = (int)EntityData.jsonData["score"];
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        scoreManager.AddScore(scoreToAdd, true);
        PlayerHUD playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        Vector3 spawnPos = EntityPosition;
        spawnPos.y = EntityPosition.y + npcGFX.size.y;
        string pointsMessage = LocalizationManager.GetMessage("pointsMessage");
        pointsMessage = pointsMessage.Replace("%pointAmount%", scoreToAdd.ToString());
        playerHUD.CreatePopupText(spawnPos, pointsMessage);
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

    public bool IsInKnockback { get; set; } = false;
    public NPCNavigation NPCNavigation { get; private set; }
    public override Vector2 CenterOfMass => EntityPosition + Vector3.up * npcGFX.size.y / 2;
    public virtual bool IsInvulnerable { get; set; } = false; 
    public bool IsAlive { get { return currentHealth > 0; } }
}
