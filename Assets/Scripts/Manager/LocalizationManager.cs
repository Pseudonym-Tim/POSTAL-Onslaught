using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine;

/// <summary>
/// Handles everything related to text localization...
/// </summary>
public static class LocalizationManager
{
    public static JObject JsonData
    {
        get
        {
            return JsonUtility.ParseJson("game_english");
        }
    }

    public static string GetMessage(string key, string category = null)
    {
        if(JsonData == null)
        {
            Debug.LogError("JSON data not loaded!");
            return null;
        }

        JToken jsonToken = category == null ? JsonData[key] : JsonData[category]?[key];

        if(jsonToken != null)
        {
            return jsonToken.ToString();
        }
        else
        {
            string message = category == null ? $"Message not found for key: {key}" : $"Message not found for category: {category}, key: {key}";
            Debug.LogError(message);
            return null;
        }
    }
}
