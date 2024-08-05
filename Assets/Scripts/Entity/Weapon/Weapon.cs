using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all weapons...
/// </summary>
public class Weapon : Entity
{
    protected const float INPUT_DELAY = 0.5f;
    public SpriteRenderer weaponGFX;
    public Sprite weaponSprite;
    public Sprite weaponHeldSprite;
    public Sprite weaponIconSprite;
    public KnockbackInfo hurtKnockbackInfo;
    public string weaponID = "new_weapon";
    public string weaponName = "NewWeapon";
    protected WeaponManager weaponManager;
    protected LevelManager levelManager;
    protected Entity ownerEntity;
    protected SFXManager sfxManager;

    public override void OnEntityAwake()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        sfxManager = FindFirstObjectByType<SFXManager>();
        weaponManager = GetComponentInParent<WeaponManager>();
        weaponGFX.sprite = weaponHeldSprite;
        ownerEntity = weaponManager.GetOwnerEntity();
        SetupEntityAnim();
        EntityAnim.Play("Idle");
    }

    public void CheckHouseDisturbance()
    {
        List<NPCHome> npcHomes = new List<NPCHome>();
        Vector2 checkOrigin = ownerEntity.CenterOfMass;
        float checkRange = NPCHome.DISTURB_RANGE;
        npcHomes.AddRange(levelManager.GetEntities<NPCHome>(checkOrigin, checkRange));

        for(int i = 0; i < npcHomes.Count; i++)
        {
            npcHomes[i].TriggerNPCSpawn();
        }
    }

    public virtual void OnWeaponSelected() { }
    public virtual void OnWeaponHolster() { }

    public NPC NPC => ownerEntity as NPC;
    public Player Player => ownerEntity as Player;
    public bool IsOwnerNPC() => ownerEntity is NPC;
    public bool IsOwnerPlayer() => ownerEntity is Player;

    public bool IsOwnerAlive()
    {
        if(IsOwnerNPC()) { return NPC.IsAlive; }
        else if(IsOwnerPlayer()) { return Player.IsAlive; }
        return false;
    }

    public bool IsMeleeWeapon { get { return this is MeleeWeapon; } }
}