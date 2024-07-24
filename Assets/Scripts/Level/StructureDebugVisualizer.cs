using System.Collections.Generic;
using UnityEngine;

public class StructureDebugVisualizer : MonoBehaviour
{
    public static List<Bounds> DebugBounds = new List<Bounds>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach(Bounds bounds in DebugBounds)
        {
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    public static void AddBounds(Bounds bounds)
    {
        DebugBounds.Add(bounds);
    }

    public static void ClearBounds()
    {
        DebugBounds.Clear();
    }
}
