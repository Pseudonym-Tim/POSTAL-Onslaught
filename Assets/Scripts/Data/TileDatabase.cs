using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Holds and manages a database of tiles..
/// </summary>
public class TileDatabase : ScriptableObject
{
    public const string ASSET_PATH = "Assets/Scriptables/TileDatabase.asset";

    public List<TileData> tileDatabase;

    public TileData GetTile(string tileID)
    {
        return tileDatabase.FirstOrDefault(tileData => tileData.id == tileID);
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create Tile Database")]
    private static void Create()
    {
        TileDatabase tileDatabase = CreateInstance<TileDatabase>();
        AssetUtility.CreateAsset(tileDatabase, ASSET_PATH);
        EditorUtility.SetDirty(tileDatabase);
    }

    [MenuItem("Assets/Create Tile Database", true)]
    private static bool CheckCreate()
    {
        TileDatabase tileDatabase = Load();
        if(tileDatabase == null) { return true; }
        return false;
    }

    public static TileDatabase Load()
    {
        return AssetUtility.LoadAsset<TileDatabase>(ASSET_PATH);
    }
#endif
}
