using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the crosshair on the player hud...
/// </summary>
public class HUDCrosshair : UIComponent
{
    public override void Setup()
    {
        
    }

    public void UpdatePosition()
    {
        transform.position = Input.mousePosition;
    }
}
