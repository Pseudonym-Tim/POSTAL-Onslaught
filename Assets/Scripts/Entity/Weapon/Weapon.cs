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
    protected Entity ownerEntity;

    public override void OnEntityAwake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        weaponGFX.sprite = weaponHeldSprite;
        ownerEntity = weaponManager.GetOwnerEntity();
        SetupEntityAnim();
        EntityAnim.Play("Idle");
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