using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles everything related to inventory and items...
/// </summary>
public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] private ItemDatabase itemDatabase;
    private List<InventoryItem> inventoryItems;
    private int currentIndex = 0;
    private PlayerHUD playerHUD;
    private LevelManager levelManager;

    private void Awake()
    {
        inventoryItems = new List<InventoryItem>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        levelManager = FindFirstObjectByType<LevelManager>();
        UpdateSelection();
        playerHUD.UpdateCurrentItem(null, 0);
    }

    private void Update()
    {
        if(PlayerInput.InputEnabled)
        {
            UpdateSelection();

            if(InputManager.IsButtonPressed("ItemUse"))
            {
                UseCurrentItem();
            }
        }
    }

    private void UpdateSelection()
    {
        if(inventoryItems.Count > 0)
        {
            if(InputManager.IsButtonPressed("ItemNavigateLeft"))
            {
                currentIndex = (currentIndex - 1 + inventoryItems.Count) % inventoryItems.Count;
                playerHUD.UpdateCurrentItem(CurrentInventoryItem);
            }

            if(InputManager.IsButtonPressed("ItemNavigateRight"))
            {
                currentIndex = (currentIndex + 1) % inventoryItems.Count;
                playerHUD.UpdateCurrentItem(CurrentInventoryItem);
            }
        }
    }

    private void UseCurrentItem()
    {
        if(inventoryItems.Count > 0)
        {
            Player playerEntity = levelManager.GetEntity<Player>();
            JObject itemJsonData = CurrentInventoryItem.itemData.jsonData;
            int healthToGive = (int)itemJsonData["health"];

            if(IsItemUsable(CurrentInventoryItem.itemData))
            {
                if(healthToGive > 0) { playerEntity.Heal(healthToGive); }
                RemoveItem(CurrentInventoryItem.itemData.id);

                if(inventoryItems.Count > 0)
                {
                    playerHUD.UpdateCurrentItem(CurrentInventoryItem);
                }
                else
                {
                    playerHUD.UpdateCurrentItem(null, 0);
                }
            }
        }
    }

    private bool IsItemUsable(ItemData itemData)
    {
        Player playerEntity = levelManager.GetEntity<Player>();
        int healthToGive = (int)itemData.jsonData["health"];

        if(healthToGive > 0 && !playerEntity.IsMaxHealth)
        {
            return true;
        }

        return false;
    }

    public InventoryItem CurrentInventoryItem
    {
        get { return inventoryItems.Count > 0 ? inventoryItems[currentIndex] : null; }
    }

    public void AddItem(string itemID, int quantity, bool autoSelect = true)
    {
        ItemData itemToAdd = itemDatabase.GetItem(itemID);

        if(!CurrentlyHasItem(itemID))
        {
            InventoryItem inventoryItem = new InventoryItem()
            {
                itemData = itemToAdd,
                itemQuantity = quantity
            };

            inventoryItems.Add(inventoryItem);

            if(autoSelect)
            {
                currentIndex = inventoryItems.IndexOf(inventoryItem);
            }
        }
        else
        {
            InventoryItem inventoryItem = GetInventoryItem(itemID);
            inventoryItem.itemQuantity++;
        }

        UpdateSelection();
        playerHUD.UpdateCurrentItem(CurrentInventoryItem);
    }

    public void RemoveItem(string itemID)
    {
        InventoryItem itemToRemove = GetInventoryItem(itemID);

        if(itemToRemove != null)
        {
            if(itemToRemove.itemQuantity > 1)
            {
                itemToRemove.itemQuantity--;
            }
            else
            {
                inventoryItems.Remove(itemToRemove);

                if(inventoryItems.Count > 0)
                {
                    currentIndex = Mathf.Clamp(currentIndex, 0, inventoryItems.Count - 1);
                    playerHUD.UpdateCurrentItem(CurrentInventoryItem.itemData.sprite, CurrentInventoryItem.itemQuantity);
                }
                else
                {
                    playerHUD.UpdateCurrentItem(null, 0);
                }
            }
        }

        UpdateSelection();
    }

    public bool CurrentlyHasItem(string itemID)
    {
        return inventoryItems.Any(item => item.itemData.id == itemID);
    }

    public InventoryItem GetInventoryItem(string itemID)
    {
        return inventoryItems.FirstOrDefault(item => item.itemData.id == itemID);
    }
}