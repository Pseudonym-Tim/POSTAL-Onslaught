using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the gameover screen...
/// </summary>
public class GameOverUI : UIComponent
{
    [SerializeField] private Canvas uiCanvas;
    public TextMeshProUGUI deathMessageText;

    public override void Setup()
    {
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        uiCanvas.enabled = showUI;
        // TODO: Update gameover stuff here...
    }

    public override string UIJsonIdentifier => "gameover_ui";
}
