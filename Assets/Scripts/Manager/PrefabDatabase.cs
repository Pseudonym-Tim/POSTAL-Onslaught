using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;

/// <summary>
/// Holds and manages a database of prefabs.
/// </summary>
public class PrefabDatabase : Singleton<PrefabDatabase>
{
    [SerializeField] private List<GameObject> registeredPrefabs = new List<GameObject>();

    /// <summary>
    /// Registers a prefab if it doesn't already exist in the database.
    /// </summary>
    public void RegisterPrefab(GameObject prefab)
    {
        if(registeredPrefabs.Any(prefabObject => prefabObject.name == prefab.name))
        {
            Debug.LogWarning($"Prefab: [{ prefab.name }] already exists!");
            return;
        }

        registeredPrefabs.Add(prefab);
        Debug.Log($"Prefab: [{ prefab.name }] registered in { nameof(PrefabDatabase) }!");
    }

    /// <summary>
    /// Retrieves a prefab by its name from the database.
    /// </summary>
    public GameObject GetPrefab(string name)
    {
        foreach(GameObject prefab in registeredPrefabs)
        {
            if(prefab.name == name)
            {
                return prefab;
            }
        }

        return null;
    }

    /// <summary>
    /// Retrieves a component of type T from a prefab by its name from the database.
    /// </summary>
    public T GetPrefab<T>(string name) where T : Component
    {
        foreach(GameObject prefab in registeredPrefabs)
        {
            if(prefab.name == name)
            {
                T component = prefab.GetComponent<T>();

                if(component != null)
                {
                    return component;
                }
            }
        }

        return null;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Register Prefab")]
    private static void CheckRegisterPrefab()
    {
        GameObject selectedPrefab = Selection.activeGameObject;

        if(selectedPrefab != null)
        {
            string prefabPath = AssetUtility.GetAssetPath(selectedPrefab);
            GameObject prefab = AssetUtility.LoadAsset<GameObject>(prefabPath);
            PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
            prefabDatabase?.RegisterPrefab(prefab);
        }
        else
        {
            Debug.LogWarning("No prefab selected to register!");
        }
    }

    [MenuItem("Assets/Register Prefab", true)]
    private static bool IsValidPrefab()
    {
        if(Selection.activeObject is GameObject)
        {
            PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType(Selection.activeObject);
            return prefabAssetType == PrefabAssetType.Regular;
        }

        return false;
    }

    [MenuItem("GameObject/Create Prefab", false, 10)]
    private static void CreatePrefab()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if(selectedObject != null)
        {
            string localPath = "Assets/Prefabs/" + selectedObject.name + ".prefab";

            localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

            PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObject, localPath, InteractionMode.UserAction);
            Debug.Log($"Prefab created at: { localPath }");

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(localPath);
            PrefabDatabase prefabDatabase = FindFirstObjectByType<PrefabDatabase>();
            prefabDatabase?.RegisterPrefab(prefab);
        }
        else
        {
            Debug.LogWarning("No GameObject selected to create a prefab!");
        }
    }

    [MenuItem("GameObject/Create Prefab", true)]
    private static bool ValidateCreatePrefab()
    {
        return Selection.activeGameObject != null;
    }
#endif
}
