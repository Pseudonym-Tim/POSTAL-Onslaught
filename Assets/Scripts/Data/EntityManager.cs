using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to entity management...
/// </summary>
public class EntityManager : Singleton<EntityManager>
{
    public static Dictionary<string, EntityData> RegisteredEntities { get; private set; } = null;
    private static PrefabDatabase prefabDatabase = null;

    public static void LoadDatabase()
    {
        prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        RegisteredEntities = JsonUtility.LoadJson<string, EntityData>("entity_database");

        foreach(KeyValuePair<string, EntityData> entityData in RegisteredEntities)
        {
            LoadData(entityData.Key, entityData.Value);
        }
    }

    private static void LoadData(string entityID, EntityData entityData)
    {
        entityData.id = entityID;
        entityData.jsonData = (JObject)JsonUtility.ParseJson("entity_database")[entityID];
    }

    public static Entity CreateEntity(string entityID, Vector3 spawnPos, Transform parentTransform = null, bool worldPositionStays = false)
    {
        EntityData entityData = RegisteredEntities[entityID];
        Entity entityPrefab = prefabDatabase?.GetPrefab<Entity>(entityData.prefab);
        Entity entitySpawned = Instantiate(entityPrefab, spawnPos, Quaternion.identity);
        entitySpawned.name = entityData.prefab;
        entitySpawned.SetParent(parentTransform, worldPositionStays);
        entitySpawned.EntityData = entityData;
        entitySpawned.OnEntitySpawn();
        return entitySpawned;
    }
}
