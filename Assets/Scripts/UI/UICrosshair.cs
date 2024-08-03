using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the crosshair on the player hud...
/// </summary>
public class UICrosshair : UIComponent
{
    private void Awake()
    {
        
    }

    private void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        transform.position = mousePosition;
    }
}
