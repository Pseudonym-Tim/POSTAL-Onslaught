using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds item data and quantity...
/// </summary>
[System.Serializable]
public class InventoryItem 
{
    public ItemData itemData;
    public int itemQuantity = 0;
}
