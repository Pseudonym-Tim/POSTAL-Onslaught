using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to object management...
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    public static Dictionary<string, ObjectData> RegisteredObjects { get; private set; } = null;
    private static PrefabDatabase prefabDatabase = null;

    public static void LoadDatabase()
    {
        prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
        RegisteredObjects = JsonUtility.LoadJson<string, ObjectData>("object_database");

        foreach(KeyValuePair<string, ObjectData> objectData in RegisteredObjects)
        {
            LoadData(objectData.Key, objectData.Value);
        }
    }

    private static void LoadData(string objectID, ObjectData objectData)
    {
        objectData.id = objectID;
        objectData.jsonData = (JObject)JsonUtility.ParseJson("object_database")[objectID];
    }

    public static GameObject CreateObject(string objectID, Vector3 spawnPos, Transform parentTransform = null, bool worldPositionStays = false)
    {
        ObjectData objectData = RegisteredObjects[objectID];
        GameObject objectPrefab = prefabDatabase?.GetPrefab(objectData.prefab);
        GameObject objectSpawned = Instantiate(objectPrefab, spawnPos, Quaternion.identity);
        objectSpawned.name = objectData.prefab;
        objectSpawned.transform.SetParent(parentTransform, worldPositionStays);
        LevelObject levelObject = objectSpawned.GetComponent<LevelObject>();
        if(levelObject) { levelObject.ObjectData = objectData; }
        return objectSpawned;
    }
}
