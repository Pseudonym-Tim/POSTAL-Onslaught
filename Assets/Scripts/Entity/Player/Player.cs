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
    [SerializeField] protected CameraShakeInfo hurtCameraShakeInfo;
    [SerializeField] protected WindowShakeInfo hurtWindowShakeInfo;
    [SerializeField] protected List<GoreManager.GibInfo> gibList;
    [SerializeField] protected KnockbackInfo hurtKnockbackInfo;
    public SpriteRenderer playerGFX;
    protected int currentHealth = 0;
    private PlayerMovement playerMovement;
    private PlayerHUD playerHUD;
    private PlayerDistanceTracker distanceTracker;
    private KillCreativityManager killCreativityManager;
    private Transform rootTransform;

    public override void OnEntityAwake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        WeaponManager = GetComponentInChildren<WeaponManager>();
        InventoryManager = GetComponentInChildren<InventoryManager>();
        PlayerMovement = GetComponentInChildren<PlayerMovement>();
        killCreativityManager = FindFirstObjectByType<KillCreativityManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        PlayerCamera = GetComponentInChildren<PlayerCamera>();
        distanceTracker = EntityObject.AddComponent<PlayerDistanceTracker>();
        rootTransform = EntityTransform.Find("Root");
    }

    public override void OnEntitySpawn()
    {
        PlayerCamera.Setup();
        WeaponManager.Setup();
        SetupEntityAnim();
        currentHealth = maxHealth;
    }

    public override void OnLevelGenerated()
    {
        playerHUD.UpdateHealthIndicator(currentHealth, maxHealth);
        PlayerInput.InputEnabled = true;
        killCreativityManager.Setup();
        distanceTracker.Setup();
        EntityAnim.Play("Idle");
        LastDamageInfo = null;
    }

    protected override void OnEntityUpdate()
    {
        playerMovement.UpdateMovement();
        EntityAnim.SetBool("isMoving", playerMovement.IsMoving);

        // TEST! Remove later...
        if(Input.GetKeyDown(KeyCode.H) && Application.isEditor)
        {
            DamageInfo damageInfo = new DamageInfo()
            {
                damageOrigin = EntityPosition + Vector3.up,
                attackerEntity = this,
                damageAmount = 1
            };

            TakeDamage(damageInfo);
        }

        // TODO: Remove later!
        if(Input.GetKeyDown(KeyCode.K) && Application.isEditor)
        {
            TakeDamage(new DamageInfo { attackerEntity = this, damageAmount = 9999, damageOrigin = CenterOfMass });
        }

        // TODO: Remove later!
        if(Input.GetKeyDown(KeyCode.G) && Application.isEditor)
        {
            GoreManager.SpawnGibs(EntityPosition, gibList);
        }
    }

    public void Heal(int healthToGive)
    {
        int initialHealth = currentHealth;
        currentHealth += healthToGive;
        if(currentHealth > maxHealth) { currentHealth = maxHealth; }
        playerHUD.UpdateHealthIndicator(currentHealth, maxHealth);
        int healthRestored = currentHealth - initialHealth;
        SpawnHealthPopupText(healthRestored);
    }

    private void SpawnHealthPopupText(int healthRestored)
    {
        Vector3 spawnPos = EntityPosition;
        spawnPos.y = EntityPosition.y + playerGFX.size.y;
        string restoreHPMessage = LocalizationManager.GetMessage("restoreHPMessage");
        restoreHPMessage = restoreHPMessage.Replace("%hpAmount%", healthRestored.ToString());
        playerHUD.CreatePopupText(spawnPos, restoreHPMessage);
    }

    public void ApplyKnockback(KnockbackInfo knockbackInfo, Vector2 origin = default)
    {
        playerMovement.ApplyKnockback(knockbackInfo, origin);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if(currentHealth > 0 && !IsInvulnerable)
        {
            LastDamageInfo = damageInfo;
            currentHealth -= damageInfo.damageAmount;
            OnTakeDamage(damageInfo);
            ApplyKnockback(hurtKnockbackInfo, damageInfo.damageOrigin);
            DecalManager.SpawnDecal("blood", CenterOfMass);

            CameraShaker.Shake(hurtCameraShakeInfo);
            WindowShaker.Shake(hurtWindowShakeInfo);

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

    public void UpdateGFXFlip(bool flipX)
    {
        playerGFX.flipX = flipX;
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
        PlayerInput.InputEnabled = false;
        GameManager.GlobalStats.Deaths++;
        rootTransform.gameObject.SetActive(false);
        GoreManager.SpawnGibs(EntityPosition, gibList);
        Invoke(nameof(OnGameOver), 1.0f);
    }

    protected override void OnEntityDestroyed()
    {
        
    }

    private void OnGameOver() => GameManager.GameOver();

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CenterOfMass, 0.25f);
    }

    public override Vector2 CenterOfMass => EntityPosition + Vector3.up * playerGFX.size.y / 2;
    public bool IsAlive { get { return currentHealth > 0; } }
    public bool IsInvulnerable { get; set; } = false;
    public PlayerCamera PlayerCamera { get; set; } = null;
    public PlayerMovement PlayerMovement { get; set; } = null;
    public DamageInfo LastDamageInfo { get; set; } = null;
    public WeaponManager WeaponManager { get; set; } = null;
    public InventoryManager InventoryManager { get; set; } = null;
    public bool IsMaxHealth { get { return currentHealth >= maxHealth; } }
}
