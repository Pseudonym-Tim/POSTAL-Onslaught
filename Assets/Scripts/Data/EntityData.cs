using Newtonsoft.Json.Linq;

/// <summary>
/// Holds data for entities...
/// </summary> 
[System.Serializable]
public class EntityData
{
    public string prefab;
    public string id;
    public JObject jsonData;
}