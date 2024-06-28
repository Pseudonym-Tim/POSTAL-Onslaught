using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all weapons...
/// </summary>
public class Weapon : Entity
{
    public SpriteRenderer weaponGFX;
    public Sprite weaponSprite;
    public Sprite weaponHeldSprite;
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
}