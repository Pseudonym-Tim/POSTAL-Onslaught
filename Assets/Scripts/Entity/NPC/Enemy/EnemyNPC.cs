using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all enemy NPC's...
/// </summary>
public class EnemyNPC : NPC
{
    protected WeaponManager weaponManager;

    public void SetupWeapon()
    {
        weaponManager = GetComponentInChildren<WeaponManager>();

        if(weaponManager)
        {
            weaponManager.AimTarget = playerEntity;
            weaponManager.Setup();
        }

        EquipDefaultWeapon();
    }

    protected void EquipDefaultWeapon()
    {
        // Equip random weapon from weapon pool...
        JArray weaponPool = (JArray)EntityData.jsonData["weaponPool"];
        int randomIndex = Random.Range(0, weaponPool.Count);
        string weaponID = weaponPool[randomIndex].ToString();
        EntityData weaponEntityData = EntityManager.RegisteredEntities[weaponID];
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        Weapon defaultWeapon = prefabDatabase.GetPrefab<Weapon>(weaponEntityData.prefab);
        weaponManager.GiveWeapon(defaultWeapon);
    }
}
