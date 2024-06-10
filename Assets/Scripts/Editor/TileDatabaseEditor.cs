using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor for the tile database.
/// </summary>
public class TileDatabaseEditor : EditorWindow
{
    private TileDatabase tileDatabase;
    private TileData newTileData;
    private Vector2 scrollPosition;

    [MenuItem("Window/Tile Database Editor")]
    public static void ShowWindow()
    {
        GetWindow<TileDatabaseEditor>("Tile Database Editor");
    }

    private void OnEnable()
    {
        newTileData = new TileData();
        tileDatabase = TileDatabase.Load();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        DrawNewTileSection();
        GUILayout.Space(20);
        DrawExistingTilesSection();
    }

    private void DrawNewTileSection()
    {
        GUILayout.Label("Add New Tile", EditorStyles.boldLabel);

        newTileData.name = EditorGUILayout.TextField("Name", newTileData.name);
        newTileData.id = EditorGUILayout.TextField("ID", newTileData.id);
        newTileData.sortingOrder = EditorGUILayout.IntField("Sorting Order", newTileData.sortingOrder);
        newTileData.isAnimated = EditorGUILayout.Toggle("Is Animated", newTileData.isAnimated);
        newTileData.addCollision = EditorGUILayout.Toggle("Add Collision", newTileData.addCollision);

        DrawSpriteList(newTileData);

        if(GUILayout.Button("Add Tile to Database"))
        {
            AddNewTile();
        }
    }

    private void DrawSpriteList(TileData tileData)
    {
        if(tileData.sprites == null)
        {
            tileData.sprites = new List<Sprite>();
        }

        int spriteCount = Mathf.Max(0, EditorGUILayout.IntField("Number of Sprites", tileData.sprites.Count));
        AdjustSpriteListSize(tileData, spriteCount);

        GUILayout.Space(10);

        for(int i = 0; i < tileData.sprites.Count; i++)
        {
            tileData.sprites[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", tileData.sprites[i], typeof(Sprite), false);
            GUILayout.Space(10);
        }
    }

    private void AdjustSpriteListSize(TileData tileData, int spriteCount)
    {
        while(spriteCount < tileData.sprites.Count)
        {
            tileData.sprites.RemoveAt(tileData.sprites.Count - 1);
        }

        while(spriteCount > tileData.sprites.Count)
        {
            tileData.sprites.Add(null);
        }
    }

    private void DrawExistingTilesSection()
    {
        GUILayout.Label("Existing Tiles", EditorStyles.boldLabel);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        if(tileDatabase != null && tileDatabase.tileDatabase != null)
        {
            for(int i = 0; i < tileDatabase.tileDatabase.Count; i++)
            {
                DrawTile(tileDatabase.tileDatabase[i], i);
            }
        }

        GUILayout.EndScrollView();
    }

    private void DrawTile(TileData tile, int index)
    {
        GUILayout.BeginVertical("box");

        tile.name = EditorGUILayout.TextField("Name", tile.name);
        tile.id = EditorGUILayout.TextField("ID", tile.id);
        tile.sortingOrder = EditorGUILayout.IntField("Sorting Order", tile.sortingOrder);
        tile.isAnimated = EditorGUILayout.Toggle("Is Animated", tile.isAnimated);
        tile.addCollision = EditorGUILayout.Toggle("Add Collision", tile.addCollision);

        DrawSpriteList(tile);

        if(GUILayout.Button("Save Changes"))
        {
            SaveTileChanges();
        }

        if(GUILayout.Button("Remove Tile"))
        {
            RemoveTile(index);
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void AddNewTile()
    {
        if(tileDatabase == null)
        {
            Debug.LogError("TileDatabase is not assigned!");
            return;
        }

        if(tileDatabase.tileDatabase == null)
        {
            tileDatabase.tileDatabase = new List<TileData>();
        }

        tileDatabase.tileDatabase.Add(newTileData);
        EditorUtility.SetDirty(tileDatabase);

        newTileData = new TileData();
        Debug.Log("Tile added to the database!");
    }

    private void SaveTileChanges()
    {
        if(tileDatabase == null)
        {
            Debug.LogError("TileDatabase is not assigned!");
            return;
        }

        EditorUtility.SetDirty(tileDatabase);
        Debug.Log("Tile changes saved!");
    }

    private void RemoveTile(int index)
    {
        if(tileDatabase == null || tileDatabase.tileDatabase == null || index < 0 || index >= tileDatabase.tileDatabase.Count)
        {
            Debug.LogError("Invalid tile index!");
            return;
        }

        tileDatabase.tileDatabase.RemoveAt(index);
        EditorUtility.SetDirty(tileDatabase);
        Debug.Log("Tile removed from the database!");
    }
}
