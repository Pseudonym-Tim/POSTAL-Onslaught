using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles everything related to the pause UI...
/// </summary>
public class PauseUI : UIComponent
{
    [SerializeField] private TextMeshProUGUI pauseLabelText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private TextMeshProUGUI resumeOptionText;
    [SerializeField] private Canvas UICanvas;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private GameOverUI gameOverUI;
    private LevelClearUI levelClearUI;

    public override void SetupUI()
    {
        gameOverUI = UIManager.GetUIComponent<GameOverUI>();
        levelClearUI = UIManager.GetUIComponent<LevelClearUI>();
        pauseLabelText.text = LocalizationManager.GetMessage("pauseLabel", UIJsonIdentifier);
        LoadJsonSettings();
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        if(levelClearUI.UICanvas.enabled) { return; }
        if(gameOverUI.UICanvas.enabled) { return; }
        UICanvas.enabled = showUI;
        if(!showUI) { GameManager.ResumeGame(); return; }

        SetOptionsInactive();
        tipText.text = GetTipMessage();
        UpdateTime();
        GameManager.PauseGame();
    }

    private void UpdateTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = GameManager.GetFormattedTime();
        timeMessage = timeMessage.Replace("%minutes%", formattedTime.Split(':')[0]);
        timeMessage = timeMessage.Replace("%seconds%", formattedTime.Split(':')[1]);
        timeText.text = timeMessage;
    }

    private void LateUpdate()
    {
        if(InputManager.IsButtonPressed("Pause"))
        {
            Show(!UICanvas.enabled);
        }
    }

    public void SelectOption(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0: // Resume...
                Show(false);
                break;
            case 1: // Quit game...
                Debug.Log("QUIT TO MENU!");
                // TODO: Quit back to menu...
                break;
        }
    }

    public void HoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "resumeText" : "quitText", activeColor);
        hoveredText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "resumeText" : "quitText", inactiveColor);
        hoveredText.transform.localPosition -= new Vector3(0, hoverOffset, 0);
    }

    private void SetOptionsInactive()
    {
        menuOptions[0].text = GetFormattedMessage("resumeText", inactiveColor);
        menuOptions[1].text = GetFormattedMessage("quitText", inactiveColor);
    }

    private string GetTipMessage()
    {
        JArray tipMessages = (JArray)LocalizationManager.JsonData["tipMessages"];
        int randomIndex = Random.Range(0, tipMessages.Count);
        return (string)tipMessages[randomIndex];
    }

    private void LoadJsonSettings()
    {
        inactiveColor = (string)JsonData["options"]["inactiveColor"];
        activeColor = (string)JsonData["options"]["activeColor"];
        hoverOffset = (float)JsonData["options"]["hoverOffset"];
    }

    private string GetFormattedMessage(string messageKey, string color)
    {
        string message = LocalizationManager.GetMessage(messageKey, UIJsonIdentifier);
        return message.Replace("%color%", color);
    }

    public override string UIJsonIdentifier => "pause_ui";
}
