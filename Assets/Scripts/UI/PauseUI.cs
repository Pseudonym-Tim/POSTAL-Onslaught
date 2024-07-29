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
    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;
    [SerializeField] private TextMeshProUGUI pauseLabelText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI itemsText;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    [SerializeField] private TextMeshProUGUI resumeOptionText;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private GameOverUI gameOverUI;
    private LevelClearUI levelClearUI;
    private int selectedOptionIndex = 0;
    private int? currentlyHoveredOption = null;
    private OptionsUI optionsUI = null;
    private MusicManager musicManager;

    public override void SetupUI()
    {
        gameOverUI = UIManager.GetUIComponent<GameOverUI>();
        musicManager = FindFirstObjectByType<MusicManager>();
        levelClearUI = UIManager.GetUIComponent<LevelClearUI>();
        pauseLabelText.text = LocalizationManager.GetMessage("pauseLabel", UIJsonIdentifier);
        optionsUI = UIManager.GetUIComponent<OptionsUI>();
        LoadJsonSettings();
        Show(false); 
        currentlyHoveredOption = null;
    }

    public void Show(bool showUI = true)
    {
        if(levelClearUI.UICanvas.enabled) { return; }
        if(gameOverUI.UICanvas.enabled) { return; }
        UICanvas.enabled = showUI;
        SetCanvasInteractivity(UICanvasGroup, showUI);

        if(!showUI)
        {
            musicManager.Resume();
            GameManager.ResumeGame();

            if(currentlyHoveredOption.HasValue)
            {
                UnhoverOption(currentlyHoveredOption.Value);
                currentlyHoveredOption = null;
            }

            selectedOptionIndex = 0;
            return;
        }

        musicManager.Pause();
        GameManager.PauseGame();
        SetOptionsInactive();
        UpdateTime();
        UpdateKills();
        UpdateItems();
    }

    private void UpdateKills()
    {
        string killsMessage = LocalizationManager.GetMessage("killsText", UIJsonIdentifier);
        string killAmount = LevelManager.LevelStats.CurrentKills.ToString();
        killsMessage = killsMessage.Replace("%kills%", killAmount);
        killsText.text = killsMessage;
    }

    private void UpdateItems()
    {
        string itemsMessage = LocalizationManager.GetMessage("itemText", UIJsonIdentifier);
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Player playerEntity = levelManager.GetEntity<Player>();
        string itemAmount = playerEntity.InventoryManager.GetItemCount().ToString();
        itemsMessage = itemsMessage.Replace("%items%", itemAmount);
        itemsText.text = itemsMessage;
    }

    private void UpdateTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = LevelManager.GetFormattedTime();
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
        if(optionsUI.UICanvas.enabled) { return; }

        switch(optionIndex)
        {
            case 0: // Resume...
                Show(false);
                break;
            case 1: // Restart...
                BeginFade();
                break;
            case 2: // Options...
                optionsUI.Show();
                break;
            case 3: // Quit game...
                BeginFade();
                break;
        }

        selectedOptionIndex = optionIndex;
    }

    public void BeginFade()
    {
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeOut();
        FadeUI.OnFadeOutComplete += OnFadeOutComplete;
        SetCanvasInteractivity(UICanvasGroup, false);
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;

        switch(selectedOptionIndex)
        {
            case 1: // Restart...
                GameManager.RestartGame();
                break;
            case 3: // Quit game...
                GameManager.QuitToMainMenu();
                break;
        }

        Show(false);
    }

    public void HoverOption(int optionIndex)
    {
        if(optionsUI.UICanvas.enabled) { return; }

        if(currentlyHoveredOption == optionIndex) { return; }

        if(currentlyHoveredOption.HasValue)
        {
            UnhoverOption(currentlyHoveredOption.Value);
        }

        currentlyHoveredOption = optionIndex;

        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        string optionMessage = GetOptionMessage(optionIndex);
        hoveredText.text = GetFormattedMessage(optionMessage, activeColor);
        hoveredText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        if(currentlyHoveredOption != optionIndex) { return; }

        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        string optionMessage = GetOptionMessage(optionIndex);
        hoveredText.text = GetFormattedMessage(optionMessage, inactiveColor);
        hoveredText.transform.localPosition -= new Vector3(0, hoverOffset, 0);

        currentlyHoveredOption = null;
    }

    private string GetOptionMessage(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0: return "resumeText";
            case 1: return "restartText";
            case 2: return "optionsText";
            case 3: return "quitText";
        }

        return null;
    }

    private void SetOptionsInactive()
    {
        menuOptions[0].text = GetFormattedMessage("resumeText", inactiveColor);
        menuOptions[1].text = GetFormattedMessage("restartText", inactiveColor);
        menuOptions[2].text = GetFormattedMessage("optionsText", inactiveColor);
        menuOptions[3].text = GetFormattedMessage("quitText", inactiveColor);
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
