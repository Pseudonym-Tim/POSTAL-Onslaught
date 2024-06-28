using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the generation of levels...
/// </summary>
public class LevelGenerator : Singleton<LevelGenerator>
{
    private LevelManager levelManager = null;
    private LevelScriptParser levelScriptParser;

    public void GenerateLevel()
    {
        levelManager = FindFirstObjectByType<LevelManager>();

        InitializeLevel();
        SetRandomSeed();

        // Create script parser and parse the level script...
        levelScriptParser = new LevelScriptParser(levelManager, this);
        levelScriptParser.ParseScript();

        AddLevelCollision();
        GenerateNavigationMesh();

        // Notify all entities that the level has been generated...
        foreach(Entity entity in levelManager.LevelEntities)
        {
            entity.OnLevelGenerated();
        }
    }

    private void GenerateNavigationMesh()
    {
        int tileSize = TileManager.TILE_SIZE;
        int levelBoundsDist = levelScriptParser.LevelBoundsDist;
        int sizeX = levelScriptParser.LevelSizeX - (levelBoundsDist + tileSize * 2);
        int sizeY = levelScriptParser.LevelSizeY - (levelBoundsDist + tileSize * 2);
        Vector2 centerPos = new Vector2(levelScriptParser.LevelSizeX / 2, levelScriptParser.LevelSizeY / 2);
        LevelNavmesher.SetNavmeshVolume(centerPos - (Vector2.one * tileSize) / 2, new Vector2(sizeX, sizeY));
        LevelNavmesher.Build();
    }

    private void AddLevelCollision()
    {
        Rigidbody2D levelRigidbody2D = LevelParent.AddComponent<Rigidbody2D>();
        levelRigidbody2D.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D levelCollider2D = LevelParent.AddComponent<CompositeCollider2D>();
        levelCollider2D.geometryType = CompositeCollider2D.GeometryType.Outlines;
        levelCollider2D.generationType = CompositeCollider2D.GenerationType.Synchronous;
    }

    private List<LevelTile> GetValidSpawnTiles(string spawnTileID, int minBoundsDist)
    {
        int levelBoundsDist = levelScriptParser.LevelBoundsDist;
        int levelSizeX = levelScriptParser.LevelSizeX;
        int levelSizeY = levelScriptParser.LevelSizeY;
        int minBoundaryDist = levelBoundsDist + minBoundsDist + 1;

        List<LevelTile> spawnTiles = levelManager.GetTiles(spawnTileID);

        if(spawnTiles.Count == 0)
        {
            Debug.LogWarning($"No spawn tiles found for tile ID: {spawnTileID}!");
            return null;
        }

        List<LevelTile> validSpawnTiles = spawnTiles.FindAll(levelTile =>
        {
            float tilePosX = levelTile.TilePosition.x;
            float tilePosY = levelTile.TilePosition.y;
            bool withinXBounds = tilePosX >= minBoundaryDist && tilePosX <= levelSizeX - minBoundaryDist;
            bool withinYBounds = tilePosY >= minBoundaryDist && tilePosY <= levelSizeY - minBoundaryDist;
            return withinXBounds && withinYBounds && IsValidSpawnPosition(levelTile.TilePosition);
        });

        if(validSpawnTiles.Count == 0)
        {
            Debug.LogWarning($"No valid spawn tiles found for tile ID: {spawnTileID} within the specified boundaries!");
        }

        return validSpawnTiles;
    }

    public void SpawnEntity(string entityID, Vector2 spawnPos)
    {
        levelManager.AddEntity(entityID, spawnPos);
        Debug.Log($"Spawned: [{entityID}] at position: [{spawnPos}]");
    }

    public void SpawnObject(string objectID, Vector2 spawnPos)
    {
        levelManager.AddObject(objectID, spawnPos);
        Debug.Log($"Spawned: [{objectID}] at position: [{spawnPos}]");
    }

    public void SpawnEntity(string entityID, string spawnTileID, int minBoundsDist = 0)
    {
        List<LevelTile> validSpawnTiles = GetValidSpawnTiles(spawnTileID, minBoundsDist);
        if(validSpawnTiles.Count == 0) { return; }
        LevelTile spawnTile = validSpawnTiles[Random.Range(0, validSpawnTiles.Count)];
        SpawnEntity(entityID, spawnTile.TilePosition);
    }

    public void SpawnObject(string objectID, string spawnTileID, int minBoundsDist = 0)
    {
        List<LevelTile> validSpawnTiles = GetValidSpawnTiles(spawnTileID, minBoundsDist);
        if(validSpawnTiles.Count == 0) { return; }
        LevelTile spawnTile = validSpawnTiles[Random.Range(0, validSpawnTiles.Count)];
        SpawnObject(objectID, spawnTile.TilePosition);
    }

    private void InitializeLevel()
    {
        if(LevelParent != null) { Destroy(LevelParent); }
        LevelParent = new GameObject("Level");
        TileParent = new GameObject("Tiles");
        ObjectParent = new GameObject("Objects");
        EntityParent = new GameObject("Entities");
        LevelParent.transform.SetParent(transform, false);
        TileParent.transform.SetParent(LevelParent.transform, false);
        ObjectParent.transform.SetParent(LevelParent.transform, false);
        EntityParent.transform.SetParent(LevelParent.transform, false);
    }

    private bool IsValidSpawnPosition(Vector2 spawnPos)
    {
        foreach(Entity entity in levelManager.LevelEntities)
        {
            if(entity.EntityPosition == (Vector3)spawnPos)
            {
                return false;
            }
        }

        foreach(GameObject levelObject in levelManager.LevelObjects)
        {
            if(levelObject.transform.position == (Vector3)spawnPos)
            {
                return false;
            }
        }

        return true;
    }

    private static void SetRandomSeed()
    {
        System.Random initialRandom = new System.Random();
        CurrentLevelSeed = initialRandom.Next(int.MinValue, int.MaxValue);
        Random.InitState(CurrentLevelSeed);
    }

    public static GameObject LevelParent { get; set; } = null;
    public static GameObject TileParent { get; set; } = null;
    public static GameObject ObjectParent { get; set; } = null;
    public static GameObject EntityParent { get; set; } = null;
    private static int CurrentLevelSeed { get; set; } = 0;
}
