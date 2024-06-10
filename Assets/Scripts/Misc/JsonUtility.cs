using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

/// <summary>
/// Helper class for json files...
/// </summary>
public static class JsonUtility
{
    /// <summary>
    /// Deserializes a JSON file into a serialized dictionary...
    /// </summary>
    /// <typeparam name="TKey">The type of the keys in the dictionary</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary</typeparam>
    /// <param name="fileName">The name of the json file to deserialize</param>
    /// <returns>A dictionary of type <typeparamref name="TKey"/> and <typeparamref name="TValue"/></returns>
    public static SerializedDictionary<TKey, TValue> LoadJson<TKey, TValue>(string fileName)
    {
        var settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        string json = ParseJson(fileName).ToString();
        var deserializedDictionary = JsonConvert.DeserializeObject<SerializedDictionary<TKey, TValue>>(json, settings);
        return deserializedDictionary;
    }

    /// <summary>
    /// Parses a JSON file directly...
    /// </summary>
    /// <param name="fileName">The name of the json file to parse</param>
    public static JObject ParseJson(string fileName)
    {
        string filePath = Path.Combine("Assets", "StreamingAssets", $"{ fileName }.json");

        if(!File.Exists(filePath))
        {
            Debug.LogError($"File: [{ filePath }] not found!");
            return null;
        }

        string json = File.ReadAllText(filePath);
        return JObject.Parse(json);
    }
}
