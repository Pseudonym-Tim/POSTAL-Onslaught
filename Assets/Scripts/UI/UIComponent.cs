using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all UI components...
/// </summary>
public class UIComponent : MonoBehaviour
{
    public virtual void Setup()
    {

    }

    public JObject JsonData
    {
        get
        {
            JObject jsonData = UIManager.GetUIDatabase();
            return (JObject)jsonData[UIJsonIdentifier];
        }
    }

    public virtual string UIJsonIdentifier => "";
}
