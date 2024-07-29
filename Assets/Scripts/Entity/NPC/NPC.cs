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
    [SerializeField] protected NPCRandomizer npcRandomizer;
    [SerializeField] protected int maxHealth = 10;
    [SerializeField] protected float moveSpeed = 10;
    protected int currentHealth = 0;
    protected Player playerEntity;
    public SpriteRenderer npcGFX;
    protected Vector2 knockbackVelocity = Vector2.zero;
    protected LevelManager levelManager;

    public override void OnEntityAwake()
    {
        npcGFX = GetComponentInChildren<SpriteRenderer>();
        levelManager = FindFirstObjectByType<LevelManager>();
        SetupEntityAnim();
        SetRandomAppearance();
        EntityAnim.Play("Idle");
        currentHealth = maxHealth;
        playerEntity = GetPlayer();
    }

    public void SetRandomAppearance()
    {
        NPCInfo = npcRandomizer.GetRandom();
        EntityAnim.Animator.runtimeAnimatorController = NPCInfo.animatorController;
    }

    public override void OnLevelGenerated()
    {
        if(!playerEntity) { playerEntity = GetPlayer(); }
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
            HurtFlash.ApplyHurtFlash(this, HURT_FLASH_TIME);
            DecalManager.SpawnDecal("blood", CenterOfMass);

            // We died from that hit?
            if(currentHealth <= 0)
            {
                currentHealth = 0;
                CheckDropItem();
                AddPlayerScore();
                OnNPCKilled?.Invoke();
                OnDeath();
                GameManager.GlobalStats.Kills++;
                OnKnockbackEnd();
                StopAllCoroutines();
                levelManager.RemoveEntity(this); // TODO: Add actual death animation or effect instead...
                return;
            }
            else
            {
                Debug.Log($"[{ name }] took [{ damageInfo.damageAmount }] damage!");
            }
        }
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
            if(!IsAlive) { yield break; }
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

    public void UpdateGFXFlip(bool flipX)
    {
       npcGFX.flipX = flipX;
    }

    public bool IsVisibleToPlayer
    {
        get
        {
            if(!npcGFX.isVisible) { return false; }
            Camera playerCamera = playerEntity.PlayerCamera.Camera;
            Vector3 viewportPos = playerCamera.WorldToViewportPoint(CenterOfMass);
            bool isInViewport = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1 && viewportPos.z > 0;
            return isInViewport;
        }
    }

    public bool IsInKnockback { get; set; } = false;
    public NPCNavigation NPCNavigation { get; private set; }
    public NPCRandomizer.NPCInfo NPCInfo { get; private set; } = null;
    public override Vector2 CenterOfMass => EntityPosition + Vector3.up * npcGFX.size.y / 2;
    public virtual bool IsInvulnerable { get; set; } = false; 
    public bool IsAlive { get { return currentHealth > 0; } }

    [System.Serializable]
    public class NPCRandomizer
    {
        public List<NPCInfo> npcInfo;

        [System.Serializable]
        public class NPCInfo
        {
            public string npcID = "new_npc_id";
            public RuntimeAnimatorController animatorController;
            public AnimationClip killerAnimation;
        }

        public NPCInfo GetRandom()
        {
            if(npcInfo.Count == 0)
            {
                Debug.LogWarning("No NPC info available...");
                return null;
            }

            int randomIndex = Random.Range(0, npcInfo.Count);
            return npcInfo[randomIndex];
        }
    }
}
