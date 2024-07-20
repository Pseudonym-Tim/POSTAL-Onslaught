using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for level tile splats...
/// </summary>
[System.Serializable]
public class SplatData
{
    public string splatID = "new_splat";
    public List<Sprite> sprites;
    public List<string> tilesToAffect;
    public int spawnIterations = 3;
    public float maxSpawnOffset = 0.5f;
    public float spawnChance = 1.0f;
    public float spawnRange = 1.0f;
    public float minScale = 1f;
    public float maxScale = 1f;
}
