using Newtonsoft.Json.Linq;
using UnityEngine;

/// <summary>
/// Pickup that gives the player a weapon...
/// </summary>
public class WeaponPickup : PickupEntity
{
    private Weapon weaponToGive;
    private JArray weaponPool;
    private static LevelManager levelManager;

    public override void OnEntitySpawn()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        weaponPool = (JArray)EntityData.jsonData["weaponPool"];
        int randomIndex = Random.Range(0, weaponPool.Count);
        string weaponID = (string)weaponPool[randomIndex];
        SetWeapon(weaponID);
    }

    public static void Create(string weaponID, Vector2 addPosition)
    {
        // Create weapon pickup...
        WeaponPickup weaponPickup = (WeaponPickup)levelManager.AddEntity("weapon_pickup", addPosition);
        weaponPickup.SetWeapon(weaponID);
    }

    public override void OnInteract()
    {
        Player playerEntity = levelManager.GetEntity<Player>();
        WeaponManager weaponManager = playerEntity.WeaponManager;
        weaponManager.AddWeapon(weaponToGive, true);
        DestroyEntity();
    }

    public void SetWeapon(string weaponID)
    {
        PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        EntityData entityData = EntityManager.RegisteredEntities[weaponID];
        Weapon weaponPrefab = prefabDatabase?.GetPrefab<Weapon>(entityData.prefab);
        weaponToGive = weaponPrefab;
        pickupGFX.sprite = weaponPrefab.weaponSprite;
    }

    public override Vector3 CenterOfMass => EntityPosition;
    public override bool IsInteractable => true;
    public override float InteractRange => pickupRange;
}
