using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pickup that gives the player an item...
/// </summary>
public class ItemPickup : PickupEntity
{
    [SerializeField] private ItemDatabase itemDatabase;
    private ItemData itemToGive;
    private JArray itemPool;
    private static LevelManager levelManager;

    public override void OnEntitySpawn()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        itemPool = (JArray)EntityData.jsonData["itemPool"];
        int randomIndex = Random.Range(0, itemPool.Count);
        string itemID = (string)itemPool[randomIndex];
        SetItem(itemID);
    }

    public static ItemPickup Create(Vector2 addPosition, string itemID = null)
    {
        // Create item pickup...
        ItemPickup itemPickup = (ItemPickup)levelManager.AddEntity("item_pickup", addPosition);
        if(itemID != null) { itemPickup.SetItem(itemID); }
        return itemPickup;
    }

    public void SetItemPool(JArray newItemPool)
    {
        itemPool = newItemPool;
        int randomIndex = Random.Range(0, itemPool.Count);
        string itemID = (string)itemPool[randomIndex];
        SetItem(itemID);
    }

    public override void OnInteract()
    {
        Player playerEntity = levelManager.GetEntity<Player>();
        InventoryManager inventoryManager = playerEntity.InventoryManager;
        inventoryManager.AddItem(itemToGive.id, 1, true);
        SpawnPickupText(itemToGive.name);
        levelManager.RemoveEntity(this);
    }

    public void SetItem(string itemID)
    {
        itemToGive = itemDatabase.GetItem(itemID);
        pickupGFX.sprite = itemToGive.sprite;
    }

    public override Vector2 CenterOfMass => EntityPosition;
    public override bool IsInteractable => true;
}
