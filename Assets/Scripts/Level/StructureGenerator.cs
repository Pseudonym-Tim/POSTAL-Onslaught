using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to structure generation...
/// </summary>
public static class StructureGenerator
{
    public static LevelManager LevelManager { get; set; } = null;
    public static LevelGenerator LevelGenerator { get; set; } = null;

    // List to keep track of existing structure bounds...
    private static List<Bounds> existingStructureBounds = new List<Bounds>();

    public static void Initialize(LevelManager levelManager, LevelGenerator levelGenerator)
    {
        LevelManager = levelManager;
        LevelGenerator = levelGenerator;

        // Clear debug bounds when initializing...
        existingStructureBounds.Clear();
        StructureDebugVisualizer.ClearBounds();
    }

    public static void GenerateStructure(string structureID, int minBoundaryDist)
    {
        JObject structureData = (JObject)JsonUtility.ParseJson("structure_database")[structureID];
        Vector2 structureBounds = new Vector2((int)structureData["structureBounds"]["boundsX"], (int)structureData["structureBounds"]["boundsY"]);
        Vector2 center = new Vector2((int)structureData["structureBounds"]["center"]["x"], (int)structureData["structureBounds"]["center"]["y"]);

        Vector2 spawnPos = FindValidSpawnPosition(structureBounds, center, minBoundaryDist);

        if(spawnPos == Vector2.zero)
        {
            Debug.LogWarning($"No valid spawn position found for structure {structureID} within specified boundaries!");
            return;
        }

        GenerateEntities(structureData["entities"] as JArray, spawnPos, center, structureBounds);
        GenerateLevelObjects(structureData["level_objects"] as JArray, spawnPos, center, structureBounds);

        // Register the structure bounds after generation...
        RegisterStructureBounds(spawnPos, structureBounds, center);
    }

    private static Vector2 FindValidSpawnPosition(Vector2 structureBounds, Vector2 center, int minBoundaryDist)
    {
        int levelSizeX = LevelGenerator.levelScriptParser.LevelSizeX;
        int levelSizeY = LevelGenerator.levelScriptParser.LevelSizeY;

        int minX = minBoundaryDist + (int)center.x;
        int maxX = levelSizeX - minBoundaryDist - (int)structureBounds.x + (int)center.x;
        int minY = minBoundaryDist + (int)center.y;
        int maxY = levelSizeY - minBoundaryDist - (int)structureBounds.y + (int)center.y;

        int maxAttempts = 100; // Maximum number of attempts to find a valid position...

        for(int attempt = 0; attempt < maxAttempts; attempt++)
        {
            int x = Random.Range(minX, maxX + 1);
            int y = Random.Range(minY, maxY + 1);
            Vector2 proposedPos = new Vector2(x, y);

            if(IsPositionValid(proposedPos, structureBounds, center))
            {
                Debug.Log($"Valid position found: {proposedPos}");
                return proposedPos;
            }
        }

        Debug.LogWarning("Failed to find valid position after maximum attempts");
        return Vector2.zero;
    }

    private static bool IsPositionValid(Vector2 position, Vector2 bounds, Vector2 center)
    {
        Vector2 min = position - center;
        Vector2 max = min + bounds;

        Debug.Log($"Checking position: {position}, Bounds min: {min}, max: {max}");

        foreach(Entity entity in LevelManager.LevelEntities)
        {
            if(entity.EntityPosition.x >= min.x && entity.EntityPosition.x <= max.x &&
                entity.EntityPosition.y >= min.y && entity.EntityPosition.y <= max.y)
            {
                Debug.Log($"Invalid position due to entity at: {entity.EntityPosition}");
                return false;
            }
        }

        // Remove overlapping level objects
        RemoveOverlappingLevelObjects(min, max);

        foreach(Bounds structureBounds in existingStructureBounds)
        {
            if(DoBoundsOverlap(new Bounds(position + (bounds / 2), bounds), structureBounds))
            {
                Debug.Log($"Invalid position due to overlap with structure bounds: {structureBounds}");
                return false;
            }
        }

        return true;
    }

    private static void RemoveOverlappingLevelObjects(Vector2 min, Vector2 max)
    {
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach(GameObject levelObject in LevelManager.LevelObjects)
        {
            Vector2 objectPos = levelObject.transform.position;

            if(objectPos.x >= min.x && objectPos.x <= max.x && objectPos.y >= min.y && objectPos.y <= max.y)
            {
                Debug.Log($"Removing overlapping level object at: {objectPos}");
                objectsToRemove.Add(levelObject);
            }
        }

        foreach(GameObject obj in objectsToRemove)
        {
            LevelManager.RemoveObject(obj);
        }
    }

    private static bool ShouldGenerate(float generateChance)
    {
        return Random.value <= generateChance;
    }

    private static void RegisterStructureBounds(Vector2 position, Vector2 bounds, Vector2 center)
    {
        Bounds newStructureBounds = new Bounds(position - center + (bounds / 2), bounds);
        existingStructureBounds.Add(newStructureBounds);
        StructureDebugVisualizer.AddBounds(newStructureBounds); // Add bounds to the visualizer...

        Debug.Log($"Structure registered with bounds: {newStructureBounds}");
    }

    private static void GenerateEntities(JArray entities, Vector2 origin, Vector2 center, Vector2 structureBounds)
    {
        foreach(JObject entityEntry in entities)
        {
            foreach(KeyValuePair<string, JToken> entity in entityEntry)
            {
                string id = entity.Key;
                JObject entityData = entity.Value as JObject;
                float generateChance = (float)entityData["generateChance"];

                if(entityData["generateChance"] != null && !ShouldGenerate(generateChance))
                {
                    continue;
                }

                int x = (int)entityData["position"]["x"];
                int y = (int)entityData["position"]["y"];
                Vector2 spawnPos = new Vector2(x, y) + origin - center + (structureBounds / 2);

                LevelGenerator.SpawnEntity(id, spawnPos);
            }
        }
    }

    private static void GenerateLevelObjects(JArray levelObjects, Vector2 origin, Vector2 center, Vector2 structureBounds)
    {
        foreach(JObject objectEntry in levelObjects)
        {
            foreach(KeyValuePair<string, JToken> levelObject in objectEntry)
            {
                string id = levelObject.Key;
                JObject objectData = levelObject.Value as JObject;
                float generateChance = (float)objectData["generateChance"];

                if(objectData["generateChance"] != null && !ShouldGenerate(generateChance))
                {
                    continue;
                }

                int x = (int)objectData["position"]["x"];
                int y = (int)objectData["position"]["y"];
                Vector2 spawnPos = new Vector2(x, y) + origin - center + (structureBounds / 2);

                LevelGenerator.SpawnObject(id, spawnPos);
            }
        }
    }

    // Method to check if two bounds overlap...
    private static bool DoBoundsOverlap(Bounds a, Bounds b)
    {
        return a.min.x < b.max.x && a.max.x > b.min.x &&
               a.min.y < b.max.y && a.max.y > b.min.y;
    }
}
