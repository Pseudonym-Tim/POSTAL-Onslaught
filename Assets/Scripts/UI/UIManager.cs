using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles core UI related stuff...
/// </summary>
public class UIManager : Singleton<UIManager>
{
    public static void SetupUI()
    {
        List<UIComponent> uiComponentList = GetUIComponents<UIComponent>();

        foreach(UIComponent uiComponent in uiComponentList)
        {
            uiComponent.Setup();
        }
    }

    public static T GetUIComponent<T>() where T : UIComponent
    {
        return GetComponentRecursively<T>(UIParent.transform);
    }

    public static List<T> GetUIComponents<T>() where T : UIComponent
    {
        return GetComponentsRecursively<T>(UIParent.transform).ToList();
    }

    private static T GetComponentRecursively<T>(Transform parentTransform) where T : UIComponent
    {
        T component = parentTransform.GetComponent<T>();

        if(component != null)
        {
            return component;
        }

        for(int i = 0; i < parentTransform.childCount; i++)
        {
            T foundComponent = GetComponentRecursively<T>(parentTransform.GetChild(i));

            if(foundComponent != null)
            {
                return foundComponent;
            }
        }

        return null;
    }

    private static IEnumerable<T> GetComponentsRecursively<T>(Transform parentTransform) where T : UIComponent
    {
        T[] components = parentTransform.GetComponentsInChildren<T>(true);

        foreach(T component in components)
        {
            yield return component;
        }
    }

    public static JObject GetUIDatabase()
    {
        JObject jsonData = JsonUtility.ParseJson("ui_database");
        return jsonData;
    }

    public static GameObject UIParent { get { return GameObject.Find("UI"); } }
}
