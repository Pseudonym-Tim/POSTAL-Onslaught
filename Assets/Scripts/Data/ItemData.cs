using Newtonsoft.Json.Linq;
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
    public string jsonDataString;
    public Sprite sprite;
    public JObject jsonData => JObject.Parse(jsonDataString);
}
