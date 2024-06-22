using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all weapons...
/// </summary>
public class Weapon : Entity
{
    public SpriteRenderer weaponGFX;
    public WeaponID weaponID = WeaponID.PISTOL;
    public string weaponName = "NewWeapon";
    protected WeaponManager weaponManager;
    protected Player playerEntity; // TODO: Change this to ownerEntity instead?

    public override void OnEntityAwake()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        playerEntity = GetComponentInParent<Player>();
        SetupEntityAnim();
        EntityAnim.Play("Idle");
    }

    public virtual void OnWeaponSelected() { }
    public virtual void OnWeaponHolster() { }
}

public enum WeaponID
{
    PISTOL,
    BOOMSTICK
}