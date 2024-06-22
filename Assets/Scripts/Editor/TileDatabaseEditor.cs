using NavMeshPlus.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        // Force window size
        minSize = new Vector2(800, 600);
        maxSize = new Vector2(800, 600);
    }

    private void OnDisable()
    {
        SaveTileChanges();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Tile Database Editor", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginVertical(GUILayout.Width(350)); // Adjust width here
        DrawNewTileSection();
        GUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.BeginVertical(GUILayout.Width(350)); // Adjust width here
        DrawExistingTilesSection();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.EndHorizontal();
    }

    private void DrawNewTileSection()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Add New Tile", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        newTileData.name = EditorGUILayout.TextField("Name", newTileData.name);
        newTileData.id = EditorGUILayout.TextField("ID", newTileData.id);
        newTileData.sortingOrder = EditorGUILayout.IntField("Sorting Order", newTileData.sortingOrder);
        newTileData.isAnimated = EditorGUILayout.Toggle("Is Animated", newTileData.isAnimated);
        newTileData.collisionData.addCollision = EditorGUILayout.Toggle("Add Collision", newTileData.collisionData.addCollision);

        if(newTileData.collisionData.addCollision)
        {
            EditorGUI.indentLevel++;
            newTileData.collisionData.size = EditorGUILayout.Vector2Field("Collision Size", newTileData.collisionData.size);
            newTileData.collisionData.offset = EditorGUILayout.Vector2Field("Collision Offset", newTileData.collisionData.offset);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();

        GUILayout.Label("Navigation", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        newTileData.navigationData.area = EditorGUILayout.IntField("Area", newTileData.navigationData.area);
        newTileData.navigationData.overrideArea = EditorGUILayout.Toggle("Override Area", newTileData.navigationData.overrideArea);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        DrawSpriteList(newTileData);

        EditorGUILayout.Space();

        if(GUILayout.Button("Add Tile to Database"))
        {
            AddNewTile();
        }

        GUILayout.EndVertical();
    }

    private void DrawSpriteList(TileData tileData)
    {
        if(tileData.sprites == null)
        {
            tileData.sprites = new List<Sprite>();
        }

        GUILayout.Label("Sprites", EditorStyles.boldLabel);

        int spriteCount = Mathf.Max(0, EditorGUILayout.IntField("Number of Sprites", tileData.sprites.Count));
        AdjustSpriteListSize(tileData, spriteCount);

        EditorGUILayout.Space();

        for(int i = 0; i < tileData.sprites.Count; i++)
        {
            tileData.sprites[i] = (Sprite)EditorGUILayout.ObjectField($"Sprite {i + 1}", tileData.sprites[i], typeof(Sprite), false);
            EditorGUILayout.Space();
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
        GUILayout.BeginVertical("box");
        GUILayout.Label("Existing Tiles", EditorStyles.boldLabel);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(400));

        if(tileDatabase != null && tileDatabase.tileDatabase != null)
        {
            for(int i = 0; i < tileDatabase.tileDatabase.Count; i++)
            {
                DrawTile(tileDatabase.tileDatabase[i], i);
            }
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawTile(TileData tileData, int index)
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label($"Tile {index + 1}", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        tileData.name = EditorGUILayout.TextField("Name", tileData.name);
        tileData.id = EditorGUILayout.TextField("ID", tileData.id);
        tileData.sortingOrder = EditorGUILayout.IntField("Sorting Order", tileData.sortingOrder);
        tileData.isAnimated = EditorGUILayout.Toggle("Is Animated", tileData.isAnimated);
        tileData.collisionData.addCollision = EditorGUILayout.Toggle("Add Collision", tileData.collisionData.addCollision);

        if(tileData.collisionData.addCollision)
        {
            tileData.collisionData.size = EditorGUILayout.Vector2Field("Collision Size", tileData.collisionData.size);
            tileData.collisionData.offset = EditorGUILayout.Vector2Field("Collision Offset", tileData.collisionData.offset);
        }

        EditorGUILayout.Space();

        GUILayout.Label("Navigation", EditorStyles.boldLabel);
        tileData.navigationData.area = EditorGUILayout.IntField("Area", tileData.navigationData.area);
        tileData.navigationData.overrideArea = EditorGUILayout.Toggle("Override Area", tileData.navigationData.overrideArea);

        EditorGUILayout.Space();

        DrawSpriteList(tileData);

        EditorGUILayout.Space();

        if(GUILayout.Button("Save Changes"))
        {
            SaveTileChanges();
        }

        if(GUILayout.Button("Remove Tile"))
        {
            RemoveTile(index);
        }

        GUILayout.EndVertical();
        EditorGUILayout.Space();
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

        EditorUtility.SetDirty(tileDatabase); // Mark the tile database as dirty
        AssetDatabase.SaveAssets(); // Save changes to disk
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
