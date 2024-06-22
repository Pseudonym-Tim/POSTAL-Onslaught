using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the management of levels...
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    private LevelGenerator levelGenerator;
    private TileManager tileManager;
    private PrefabDatabase prefabDatabase = null;

    public void CreateLevel()
    {
        CurrentLevel++;
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        tileManager = FindFirstObjectByType<TileManager>();
        prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        LevelTiles = new Dictionary<Vector2, LevelTile>();
        LevelEntities = new List<Entity>();
        LoadEntityDatabase();
        levelGenerator.GenerateLevel();
    }

    private void LoadEntityDatabase()
    {
        EntityDatabase = JsonUtility.LoadJson<string, EntityData>("entity_database");

        foreach(KeyValuePair<string, EntityData> entityData in EntityDatabase)
        {
            LoadEntityData(entityData.Key, entityData.Value);
        }
    }

    private void LoadEntityData(string entityID, EntityData entityData)
    {
        entityData.id = entityID;
        entityData.jsonData = (JObject)JsonUtility.ParseJson("entity_database")[entityID];
    }

    public void AddTile(string tileID, Vector2 addPos)
    {
        Vector2 tilePos = TileManager.SnapToGrid(addPos);
        RemoveTile(tilePos);
        LevelTile levelTile = tileManager.AddTile(tileID, tilePos);
        LevelTiles[tilePos] = levelTile;
    }

    public void RemoveTile(Vector2 removePos)
    {
        Vector2 tilePos = TileManager.SnapToGrid(removePos);

        if(LevelTiles.ContainsKey(tilePos))
        {
            tileManager.RemoveTile(tilePos);
            LevelTiles.Remove(tilePos);
        }
    }

    public LevelTile GetTile(string tileID, Vector2 tilePos)
    {
        if(LevelTiles.ContainsKey(tilePos))
        {
            LevelTile levelTile = LevelTiles[tilePos];

            if(levelTile.TileData.id == tileID)
            {
                return levelTile;
            }
        }

        return null;
    }

    public List<LevelTile> GetTiles(string tileID)
    {
        List<LevelTile> levelTiles = new List<LevelTile>();

        foreach(KeyValuePair<Vector2, LevelTile> tile in LevelTiles)
        {
            if(GetTile(tileID, tile.Key))
            {
                levelTiles.Add(tile.Value);
            }
        }

        return levelTiles;
    }

    public Entity AddEntity(string entityID, Vector2 addPos)
    {
        foreach(KeyValuePair<string, EntityData> entity in EntityDatabase)
        {
            if(entity.Key == entityID)
            {
                Entity entityPrefab = prefabDatabase?.GetPrefab<Entity>(entity.Value.name);
                Transform parentTransform = LevelGenerator.EntityParent.transform;
                Entity entitySpawned = Instantiate(entityPrefab, addPos, Quaternion.identity);
                entitySpawned.SetParent(parentTransform, false);
                entitySpawned.name = entityPrefab.name;
                entitySpawned.EntityData = entity.Value;
                LevelEntities.Add(entitySpawned);
                entitySpawned.OnEntitySpawn();
                return entitySpawned;
            }
        }

        return null;
    }

    public T GetEntity<T>() where T : Entity
    {
        foreach(Entity entity in LevelEntities)
        {
            if(entity is T) { return (T)entity; }
        }

        return null;
    }

    public List<T> GetEntities<T>() where T : Entity
    {
        List<T> results = new List<T>();

        foreach(Entity entity in LevelEntities)
        {
            if(entity is T)
            {
                results.Add(entity as T);
            }
        }

        return results;
    }

    public static int CurrentLevel { get; set; } = 0;
    public static Dictionary<string, EntityData> EntityDatabase { get; private set; } = null;
    public static Dictionary<string, TileData> TileDatabase { get; private set; } = null;
    public Dictionary<Vector2, LevelTile> LevelTiles { get; set; }
    public List<Entity> LevelEntities { get; set; }
}
