using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the crosshair on the player hud...
/// </summary>
public class UICrosshair : UIComponent
{
    public override void SetupUI()
    {

    }

    private void Update()
    {
        transform.position = Input.mousePosition;
    }
}
