using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all enemy NPC's...
/// </summary>
public class EnemyNPC : NPC
{
    protected WeaponManager weaponManager;

    public void SetupWeaponManager()
    {
        weaponManager = GetComponentInChildren<WeaponManager>();

        if(weaponManager)
        {
            weaponManager.AimTarget = playerEntity;
            weaponManager.Setup();
        }
    }
}
