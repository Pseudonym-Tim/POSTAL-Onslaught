using NavMeshPlus.Components;
using NavMeshPlus.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles everything related to navigation mesh generation for levels...
/// </summary>
public class LevelNavmesher : Singleton<LevelNavmesher>
{
    private static List<NavMeshSurface> navMeshSurfaces;
    private static LevelManager levelManager = null;

    public void Setup()
    {
        transform.eulerAngles = new Vector3(-90, 0, 0);
        navMeshSurfaces = new List<NavMeshSurface>();
        levelManager = FindFirstObjectByType<LevelManager>();

        for(int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            NavMeshSurface navMeshSurface = gameObject.AddComponent<NavMeshSurface>();
            navMeshSurface.agentTypeID = NavMesh.GetSettingsByIndex(i).agentTypeID;
            navMeshSurface.layerMask = LayerManager.Masks.NAVIGABLE;
            navMeshSurface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navMeshSurface.collectObjects = CollectObjects.All;
            navMeshSurface.buildHeightMesh = true;
            navMeshSurfaces.Add(navMeshSurface);
        }

        gameObject.AddComponent<CollectSources2d>();
    }

    public static Vector2 GetRandomPosition(Vector2 origin, float checkRadius, int areaMask = NavMesh.AllAreas)
    {
        Vector2 randomPosition = Vector2.zero;
        const int MAX_ATTEMPTS = 100;

        for(int i = 0; i < MAX_ATTEMPTS; i++)
        {
            Vector2 randomDirection = origin + (Random.insideUnitCircle * checkRadius);
            NavMeshHit hit;

            if(NavMesh.SamplePosition(randomDirection, out hit, checkRadius, areaMask))
            {
                randomPosition = hit.position;
                return randomPosition;
            }
        }

        Debug.LogWarning("Could not find a valid NavMesh position after " + MAX_ATTEMPTS + " attempts!");
        return randomPosition;
    }

    public static void Build()
    {
        foreach(NavMeshSurface navMeshSurface in navMeshSurfaces)
        {
            navMeshSurface.BuildNavMeshAsync().completed += OnNavMeshBuildComplete;
        }
    }

    private static void OnNavMeshBuildComplete(AsyncOperation operation)
    {
        if(IsNavmeshBuilt)
        {
            NotifyEntities();
        }
    }

    private static void NotifyEntities()
    {
        foreach(Entity entity in levelManager.LevelEntities)
        {
            entity.OnNavmeshBuilt();
        }
    }

    public static void SetNavmeshVolume(Vector2 center, Vector2 size)
    {
        foreach(NavMeshSurface navMeshSurface in navMeshSurfaces)
        {
            navMeshSurface.collectObjects = CollectObjects.Volume;
            navMeshSurface.center = new Vector3(center.x, 0, center.y);
            navMeshSurface.size = new Vector3(size.x, 1, size.y);
        }
    }

    public static bool IsNavmeshBuilt
    {
        get
        {
            foreach(NavMeshSurface navMeshSurface in navMeshSurfaces)
            {
                if(navMeshSurface.navMeshData == null)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
