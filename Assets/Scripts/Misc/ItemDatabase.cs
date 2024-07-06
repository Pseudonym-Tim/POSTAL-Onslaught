using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

#endif
using UnityEngine;

/// <summary>
/// Holds and manages a database of prefabs...
/// </summary>
public class ItemDatabase : ScriptableObject
{
    private const string ASSET_PATH = "Scriptables/ItemDatabase.asset";
    public SerializedDictionary<string, ItemData> itemDictionary;

    public ItemData GetItem(string itemID, string itemCategory = null)
    {
        if(itemCategory == null)
        {
            return itemDictionary[itemID];
        }

        return itemDictionary.Values.FirstOrDefault(item => item.id.Contains(itemCategory) && item.id == itemID);
    }

    public List<ItemData> GetItems(string itemCategory = null)
    {
        List<ItemData> items = new List<ItemData>();

        if(itemCategory == null)
        {
            return itemDictionary.Values.ToList();
        }

        return itemDictionary.Values.Where(item => item.id.Contains(itemCategory)).ToList();
    }

#if UNITY_EDITOR
    public void Load()
    {
        itemDictionary = JsonUtility.LoadJson<string, ItemData>("item_database");

        foreach(KeyValuePair<string, ItemData> itemData in itemDictionary)
        {
            LoadData(itemData.Key, itemData.Value);
        }
    }

    private void LoadData(string itemID, ItemData itemData)
    {
        itemData.id = itemID;
        string spritePath = itemData.gfxPath;
        itemData.sprite = AssetUtility.LoadAsset<Sprite>(spritePath);
        itemData.jsonData = (JObject)JsonUtility.ParseJson("item_database")[itemID];
    }

    [PostProcessScene]
    [MenuItem("Assets/Refresh Item Database #%i")]
    public static void RefreshDatabase()
    {
        ItemDatabase itemDatabase = AssetUtility.LoadAsset<ItemDatabase>(ASSET_PATH);
        EditorUtility.SetDirty(itemDatabase);
        itemDatabase.Load();
        Debug.Log("Item database loaded!");
    }

    [MenuItem("Assets/Refresh Item Database", true)]
    private static bool CheckRefresh()
    {
        return Selection.activeObject is ItemDatabase;
    }

    [MenuItem("Assets/Create Item Database")]
    private static void Create()
    {
        ItemDatabase itemDatabase = CreateInstance<ItemDatabase>();
        AssetUtility.CreateAsset(itemDatabase, ASSET_PATH);
        RefreshDatabase();
    }
#endif
}
