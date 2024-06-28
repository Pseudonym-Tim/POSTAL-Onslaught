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

    public void CreateLevel()
    {
        CurrentLevel++;
        levelGenerator = FindFirstObjectByType<LevelGenerator>();
        tileManager = FindFirstObjectByType<TileManager>();
        LevelTiles = new Dictionary<Vector2, LevelTile>();
        LevelEntities = new List<Entity>();
        LevelObjects = new List<GameObject>();
        ObjectManager.LoadDatabase();
        EntityManager.LoadDatabase();
        levelGenerator.GenerateLevel();
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
        return LevelEntities.OfType<T>().FirstOrDefault();
    }

    public List<T> GetEntities<T>() where T : Entity
    {
        return LevelEntities.OfType<T>().ToList();
    }

    public List<T> GetEntities<T>(Vector2 origin, float range) where T : Entity
    {
        Func<Entity, bool> isWithinRange = entity => Vector2.Distance(origin, entity.EntityPosition) <= range;
        return LevelEntities.OfType<T>().Where(entity => isWithinRange(entity)).ToList();
    }

    public static int CurrentLevel { get; set; } = 0;
    public static Dictionary<string, TileData> TileDatabase { get; private set; } = null;
    public Dictionary<Vector2, LevelTile> LevelTiles { get; set; }
    public List<Entity> LevelEntities { get; set; } = null;
    public List<GameObject> LevelObjects { get; set; } = null;
}
