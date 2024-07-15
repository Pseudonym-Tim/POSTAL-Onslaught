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
    protected Player playerEntity; // TODO: Change this to ownerEntity instead?

    public override void OnEntityAwake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        weaponGFX.sprite = weaponHeldSprite;
        playerEntity = GetComponentInParent<Player>();
        SetupEntityAnim();
        EntityAnim.Play("Idle");
    }

    public virtual void OnWeaponSelected() { }
    public virtual void OnWeaponHolster() { }

    public virtual bool IsMeleeWeapon { get; set; } = false;
}