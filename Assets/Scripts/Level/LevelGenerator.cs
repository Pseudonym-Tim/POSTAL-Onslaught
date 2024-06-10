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
    }

    public void SpawnEntity(string entityID, Vector2 spawnPos)
    {
        levelManager.AddEntity(entityID, spawnPos);
        Debug.Log($"Spawned: [{ entityID }] at position: [{ spawnPos }]");
    }

    // NOTE: The valid spawn position logic might be flawed...
    public void SpawnEntity(string entityID, string spawnTileID, int minBoundsDist = 0)
    {
        int levelBoundsDist = levelScriptParser.LevelBoundsDist;
        int levelSizeX = levelScriptParser.LevelSizeX;
        int levelSizeY = levelScriptParser.LevelSizeY;
        int minBoundaryDist = levelBoundsDist + minBoundsDist + 1;

        List<LevelTile> spawnTiles = levelManager.GetTiles(spawnTileID);

        if(spawnTiles.Count == 0)
        {
            Debug.LogWarning($"No spawn tiles found for tile ID: {spawnTileID}!");
            return;
        }

        List<LevelTile> validSpawnTiles = spawnTiles.FindAll(levelTile =>
        {
            float tilePosX = levelTile.TilePosition.x;
            float tilePosY = levelTile.TilePosition.y;
            bool withinXBounds = tilePosX >= minBoundaryDist && tilePosX <= levelSizeX - minBoundaryDist;
            bool withinYBounds = tilePosY >= minBoundaryDist && tilePosY <= levelSizeY - minBoundaryDist;
            return withinXBounds && withinYBounds;
        });

        if(validSpawnTiles.Count == 0)
        {
            Debug.LogWarning($"No valid spawn tiles found for tile ID: {spawnTileID} within the specified boundaries!");
            return;
        }

        LevelTile spawnTile = validSpawnTiles[Random.Range(0, validSpawnTiles.Count)];
        SpawnEntity(entityID, spawnTile.TilePosition);
    }


    private void InitializeLevel()
    {
        if(LevelParent != null) { Destroy(LevelParent); }
        LevelParent = new GameObject("Level");
        TileParent = new GameObject("Tiles");
        EntityParent = new GameObject("Entities");
        LevelParent.transform.SetParent(transform, false);
        TileParent.transform.SetParent(LevelParent.transform, false);
        EntityParent.transform.SetParent(LevelParent.transform, false);
    }

    private static void SetRandomSeed()
    {
        System.Random initialRandom = new System.Random();
        CurrentLevelSeed = initialRandom.Next(int.MinValue, int.MaxValue);
        Random.InitState(CurrentLevelSeed);
    }

    public static GameObject LevelParent { get; set; } = null;
    public static GameObject TileParent { get; set; } = null;
    public static GameObject EntityParent { get; set; } = null;
    private static int CurrentLevelSeed { get; set; } = 0;
}
