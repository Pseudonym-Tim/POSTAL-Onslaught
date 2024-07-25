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
    public CanvasGroup UICanvasGroup;
    public Canvas UICanvas;
    [SerializeField] private TextMeshProUGUI pauseLabelText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private TextMeshProUGUI resumeOptionText;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private GameOverUI gameOverUI;
    private LevelClearUI levelClearUI;
    private int selectedOptionIndex = 0;

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

        if(!showUI)
        {
            GameManager.ResumeGame();
            SetCanvasInteractivity(UICanvasGroup, false);
            selectedOptionIndex = 0;
            return;
        }

        GameManager.PauseGame();
        SetOptionsInactive();
        tipText.text = GetTipMessage();
        UpdateTime();
        SetCanvasInteractivity(UICanvasGroup, true);
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
            case 1: // Restart...
                BeginFade();
                break;
            case 2: // Quit game...
                BeginFade();
                break;
        }

        selectedOptionIndex = optionIndex;
    }

    public void BeginFade()
    {
        SetCanvasInteractivity(UICanvasGroup, false);
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        FadeUI.OnFadeOutComplete += OnFadeOutComplete;
        fadeUI.FadeOut();
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;
        Show(false);

        switch(selectedOptionIndex)
        {
            case 1: // Restart...
                GameManager.RestartGame();
                break;
            case 2: // Quit game...
                GameManager.QuitToMainMenu();
                break;
        }
    }

    public void HoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        string optionMessage = GetOptionMessage(optionIndex);
        hoveredText.text = GetFormattedMessage(optionMessage, activeColor);
        hoveredText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex]; 
        string optionMessage = GetOptionMessage(optionIndex);
        hoveredText.text = GetFormattedMessage(optionMessage, inactiveColor);
        hoveredText.transform.localPosition -= new Vector3(0, hoverOffset, 0);
    }

    private string GetOptionMessage(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0: return "resumeText";
            case 1: return "restartText";
            case 2: return "quitText";
        }

        return null;
    }

    private void SetOptionsInactive()
    {
        menuOptions[0].text = GetFormattedMessage("resumeText", inactiveColor);
        menuOptions[1].text = GetFormattedMessage("restartText", inactiveColor);
        menuOptions[2].text = GetFormattedMessage("quitText", inactiveColor);
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
