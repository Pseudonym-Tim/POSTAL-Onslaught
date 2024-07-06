#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Load or save assets in the Unity Editor!
/// </summary>
public static class AssetUtility
{
    /// <summary>
    /// Load an asset...
    /// </summary>
    public static T LoadAsset<T>(string assetPath) where T : Object
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        T assetLoaded = AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if(assetLoaded == null)
        {
            Debug.LogWarning("Asset not found: " + assetPath);
            return null;
        }
        else
        {
            return assetLoaded;
        }
    }

    /// <summary>
    /// Load all assets at the specified path...
    /// </summary>
    public static List<T> LoadAllAssets<T>(string assetPath) where T : Object
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        List<T> assetsLoaded = new List<T>();

        string[] assetGuids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { assetPath });
        foreach(string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if(asset != null)
            {
                assetsLoaded.Add(asset);
            }
        }

        if(assetsLoaded.Count == 0)
        {
            Debug.LogWarning("No assets found at path: " + assetPath);
        }

        return assetsLoaded;
    }

    /// <summary>
    /// Create a new asset...
    /// </summary>
    public static void CreateAsset(Object asset, string assetPath)
    {
        if(!assetPath.StartsWith("Assets/"))
        {
            assetPath = "Assets/" + assetPath;
        }

        // Check if asset already exists to avoid overwriting it...
        if(LoadAsset<Object>(assetPath) != null)
        {
            Debug.LogWarning($"An asset already exists at the specified path: [{ assetPath }]!");
            return;
        }

        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"Asset created: [{ assetPath }]");
    }

    /// <summary>
    /// Get asset path name...
    /// </summary>
    public static string GetAssetPath(Object obj)
    {
        if(obj == null)
        {
            Debug.LogWarning("Couldn't find asset path!");
            return string.Empty;
        }

        string path = AssetDatabase.GetAssetPath(obj);

        if(string.IsNullOrEmpty(path))
        {
            Debug.LogWarning("Asset path not found for object: " + obj.name);
            return string.Empty;
        }
        else
        {
            return path;
        }
    }
}
#endif
