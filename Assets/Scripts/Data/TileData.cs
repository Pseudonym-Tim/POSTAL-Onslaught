using NavMeshPlus.Extensions;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for level tiles...
/// </summary>
[System.Serializable]
public class TileData
{
    public string name = "NewTile";
    public string id = "new_tile";
    public int sortingOrder = 0;
    public List<Sprite> sprites;
    public bool isAnimated = false;
    public CollisionData collisionData;
    public NavigationData navigationData = new NavigationData();

    [System.Serializable]
    public class NavigationData
    {
        public int area = 0;
        public bool overrideArea = true;
    }

    [System.Serializable]
    public struct CollisionData
    {
        public bool addCollision;
        public Vector2 size;
        public Vector2 offset;
    }
}