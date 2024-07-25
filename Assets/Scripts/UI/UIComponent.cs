using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all UI components...
/// </summary>
public class UIComponent : MonoBehaviour
{
    public virtual void SetupUI()
    {

    }

    public virtual void ResetUI()
    {

    }

    protected void SetCanvasInteractivity(CanvasGroup canvasGroup, bool interactable)
    {
        if(canvasGroup != null)
        {
            canvasGroup.interactable = interactable;
            canvasGroup.blocksRaycasts = interactable;
        }
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
