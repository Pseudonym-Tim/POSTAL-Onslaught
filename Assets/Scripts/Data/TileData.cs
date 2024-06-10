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
    public bool addCollision = false;
}