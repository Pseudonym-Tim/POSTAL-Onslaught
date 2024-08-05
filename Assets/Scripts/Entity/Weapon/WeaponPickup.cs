using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup that gives the player a weapon...
/// </summary>
public class WeaponPickup : PickupEntity
{
    private Weapon weaponToGive;
    private JArray weaponPool;
    private static LevelManager levelManager;
    private WeaponManager weaponManager;

    public override void OnEntitySpawn()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public override void OnLevelGenerated()
    {
        weaponManager = levelManager.GetEntity<Player>().WeaponManager;
        weaponPool = (JArray)EntityData.jsonData["weaponPool"];
        List<string> filteredWeaponPool = FilterOwnedWeapons(weaponPool);

        if(filteredWeaponPool.Count == 0)
        {
            Debug.LogWarning("All weapons are owned by the player!");
            DestroyEntity();
            return;
        }

        string weaponID = GetRandomWeaponID(filteredWeaponPool);
        SetWeapon(weaponID);
    }

    private List<string> FilterOwnedWeapons(JArray weaponPool)
    {
        List<string> filteredWeaponPool = new List<string>();

        foreach(JToken weaponToken in weaponPool)
        {
            string weaponID = weaponToken.ToString();

            if(!weaponManager.IsWeaponOwned(weaponID))
            {
                filteredWeaponPool.Add(weaponID);
            }
        }

        return filteredWeaponPool;
    }

    private string GetRandomWeaponID(List<string> weaponPool)
    {
        int randomIndex = Random.Range(0, weaponPool.Count);
        return weaponPool[randomIndex];
    }

    public static WeaponPickup Create(string weaponID, Vector2 addPosition)
    {
        WeaponPickup weaponPickup = (WeaponPickup)levelManager.AddEntity("weapon_pickup", addPosition);
        weaponPickup.SetWeapon(weaponID);
        return weaponPickup;
    }

    public override void OnInteract()
    {
        weaponManager = levelManager.GetEntity<Player>().WeaponManager;

        if(!weaponManager.IsWeaponOwned(weaponToGive.weaponID))
        {
            if(weaponManager.WeaponCount < WeaponManager.MAX_SLOTS)
            {
                bool weaponGiven = weaponManager.GiveWeapon(weaponToGive, true);
                if(weaponGiven) { SpawnPickupText(weaponToGive.weaponName); }
            }
            else
            {
                string selectedWeaponID = weaponManager.SelectedWeapon.weaponID;
                int currentWeaponSlot = weaponManager.SelectedSlotIndex;
                weaponManager.ReplaceWeapon(currentWeaponSlot, weaponToGive);
                Create(selectedWeaponID, EntityPosition);
            }
        }
        else
        {
            Vector3 spawnPos = EntityPosition + Vector3.up * pickupGFX.size.y;
            string popupMessage = LocalizationManager.GetMessage("weaponOwnedMessage");
            playerHUD.CreatePopupText(spawnPos, popupMessage);
            return;
        }

        sfxManager.Play2DSound(pickupSound);
        levelManager.RemoveEntity(this);
    }

    public void SetWeapon(string weaponID)
    {
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        EntityData entityData = EntityManager.RegisteredEntities[weaponID];
        Weapon weaponPrefab = prefabDatabase?.GetPrefab<Weapon>(entityData.prefab);
        weaponToGive = weaponPrefab;
        pickupGFX.sprite = weaponPrefab.weaponSprite;
    }

    public override Vector2 CenterOfMass => EntityPosition;
    public override bool IsInteractable => true;
}
