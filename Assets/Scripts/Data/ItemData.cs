using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for inventory items...
/// </summary>
[System.Serializable]
public class ItemData
{
    public string id;
    public string name;
    public string gfxPath;
    public Sprite sprite;
    public JObject jsonData;
}
