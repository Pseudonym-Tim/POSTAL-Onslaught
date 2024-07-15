using System;
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
    private TaskManager taskManager;

    public void CreateLevel()
    {
        CurrentLevel++;
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        tileManager = FindFirstObjectByType<TileManager>();
        taskManager = FindFirstObjectByType<TaskManager>();

        RemoveTiles();
        RemoveObjects();
        RemoveEntities();

        LevelStats = new LevelStatistics();
        ObjectManager.LoadDatabase();
        EntityManager.LoadDatabase();
        levelGenerator.GenerateLevel();
        taskManager.Setup();
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
        foreach(KeyValuePair<string, EntityData> entityData in EntityManager.RegisteredEntities)
        {
            if(entityData.Key == entityID)
            {
                Transform parentTransform = LevelGenerator.EntityParent.transform;
                Entity entitySpawned = EntityManager.CreateEntity(entityID, addPos, parentTransform, false);
                LevelEntities.Add(entitySpawned);
                return entitySpawned;
            }
        }

        return null;
    }

    public void RemoveEntities(bool removePlayer = false)
    {
        if(LevelEntities.Count > 0)
        {
            List<Entity> entitiesToRemoveList = new List<Entity>();
            entitiesToRemoveList.AddRange(LevelEntities);

            foreach(Entity entity in entitiesToRemoveList)
            {
                if(entity is Player && !removePlayer) { continue; }
                RemoveEntity(entity);
            }
        }
    }

    public void RemoveObjects()
    {
        if(LevelObjects.Count > 0)
        {
            List<GameObject> objectsToRemoveList = new List<GameObject>();
            objectsToRemoveList.AddRange(LevelObjects);

            foreach(GameObject objectToRemove in objectsToRemoveList)
            {
                Destroy(objectToRemove);
                LevelObjects.Remove(objectToRemove);
            }

            objectsToRemoveList.Clear();
        }
    }

    public void RemoveTiles()
    {
        if(LevelTiles.Count > 0)
        {
            List<LevelTile> tilesToRemoveList = new List<LevelTile>();
            tilesToRemoveList.AddRange(LevelTiles.Values);

            foreach(LevelTile levelTile in tilesToRemoveList)
            {
                levelTile.DestroyTile();
                LevelTiles.Remove(levelTile.TilePosition);
            }

            LevelTiles.Clear();
        }
    }

    public void RemoveEntity(Entity entityToRemove, float removeTime = 0)
    {
        if(LevelEntities.Contains(entityToRemove))
        {
            entityToRemove.DestroyEntity(removeTime);
            LevelEntities.Remove(entityToRemove);
        }
    }

    public GameObject AddObject(string objectID, Vector2 addPos)
    {
        foreach(KeyValuePair<string, ObjectData> objectData in ObjectManager.RegisteredObjects)
        {
            if(objectData.Key == objectID)
            {
                Transform parentTransform = LevelGenerator.ObjectParent.transform;
                GameObject objectSpawned = ObjectManager.CreateObject(objectID, addPos, parentTransform, false);
                LevelObjects.Add(objectSpawned);
                return objectSpawned;
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
        List<T> entityList = new List<T>();

        foreach(Entity entity in LevelEntities)
        {
            if(entity is T)
            {
                entityList.Add(entity as T);
            }
        }

        return entityList;
    }

    public void RestartLevel()
    {
        // Unparent player entity so they will carry over to the next level, create new level...
        Player playerEntity = GetEntity<Player>();
        playerEntity.SetParent(null);
        CreateLevel();
        GameManager.BeginPlaying();
    }

    public List<T> GetEntities<T>(Vector2 origin, float range) where T : Entity
    {
        List<T> entityList = new List<T>();

        foreach(Entity entity in LevelEntities)
        {
            if(entity is T)
            {
                if(Vector2.Distance(origin, entity.CenterOfMass) <= range)
                {
                    entityList.Add(entity as T);
                }
            }
        }

        return entityList;
    }

    public static int CurrentLevel { get; set; } = 0;
    public static LevelStatistics LevelStats { get; private set; } = null;
    public static Dictionary<string, TileData> TileDatabase { get; private set; } = null;
    public Dictionary<Vector2, LevelTile> LevelTiles { get; set; } = new Dictionary<Vector2, LevelTile>();
    public List<Entity> LevelEntities { get; set; } = new List<Entity>();
    public List<GameObject> LevelObjects { get; set; } = new List<GameObject>();

    [System.Serializable]
    public class LevelStatistics
    {
        public int CurrentKills { get; set; } = 0;
        public int UniqueWeaponsUsed { get; set; } = 0;
        public int HighestKillstreak { get; set; } = 0;
        public float DistanceCovered { get; set; } = 0;
    }
}
