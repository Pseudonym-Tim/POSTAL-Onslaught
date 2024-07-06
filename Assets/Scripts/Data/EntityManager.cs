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

    public static Entity CreateEntity(string entityID, Vector3? spawnPos = null, Transform parentTransform = null, bool worldPositionStays = false)
    {
        EntityData entityData = RegisteredEntities[entityID];
        Entity entityPrefab = prefabDatabase?.GetPrefab<Entity>(entityData.prefab);
        Vector3 positionToSpawn = spawnPos ?? entityPrefab.EntityPosition;
        Entity entitySpawned = Instantiate(entityPrefab, parentTransform, worldPositionStays);
        entitySpawned.EntityPosition = positionToSpawn;
        entitySpawned.name = entityData.prefab;
        entitySpawned.EntityData = entityData;
        entitySpawned.OnEntitySpawn();
        return entitySpawned;
    }
}
