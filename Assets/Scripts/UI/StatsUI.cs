using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the stats UI...
/// </summary>
public class StatsUI : UIComponent
{
    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;
    [SerializeField] private TextMeshProUGUI statsLabelText;
    [SerializeField] private TextMeshProUGUI exitHelpText;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI bestTimeText;
    [SerializeField] private TextMeshProUGUI itemsCollectedText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI bestKillstreakText;

    private Coroutine exitFlashCoroutine;
    private float flashInterval = 0;
    private GlobalStatistics globalStatistics;

    public override void SetupUI()
    {
        globalStatistics = new GlobalStatistics();
        LoadJsonSettings();
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        SetCanvasInteractivity(UICanvasGroup, showUI);

        if(!showUI) { return; }

        // Fade in...
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();

        // Load and update...
        globalStatistics.LoadStats();

        UpdateHighscore();
        UpdateKills();
        UpdateItemsCollected();
        UpdateKillstreak();
        UpdateDeaths();
        UpdateTime();

        if(exitFlashCoroutine != null) { StopCoroutine(exitFlashCoroutine); }
        exitFlashCoroutine = StartCoroutine(PulseHelpText(exitHelpText, 0));
    }

    private void UpdateKills()
    {
        string killsMessage = LocalizationManager.GetMessage("killsText", UIJsonIdentifier);
        string killAmount = globalStatistics.Kills.ToString();
        killsMessage = killsMessage.Replace("%kills%", killAmount);
        killsText.text = killsMessage;
    }

    private void UpdateItemsCollected()
    {
        string itemsMessage = LocalizationManager.GetMessage("itemText", UIJsonIdentifier);
        string itemAmount = globalStatistics.ItemsCollected.ToString();
        itemsMessage = itemsMessage.Replace("%itemAmount%", itemAmount);
        itemsCollectedText.text = itemsMessage;
    }

    private void UpdateKillstreak()
    {
        string killstreakMessage = LocalizationManager.GetMessage("killstreakText", UIJsonIdentifier);
        string kilstreakAmount = globalStatistics.BestKillstreak.ToString();
        killstreakMessage = killstreakMessage.Replace("%amount%", kilstreakAmount);
        bestKillstreakText.text = killstreakMessage;
    }

    private void UpdateHighscore()
    {
        string highscoreMessage = LocalizationManager.GetMessage("highscoreText", UIJsonIdentifier);
        string highscoreAmount = globalStatistics.Highscore.ToString();
        highscoreMessage = highscoreMessage.Replace("%score%", highscoreAmount);
        highscoreText.text = highscoreMessage;
    }

    private void UpdateDeaths()
    {
        string deathMessage = LocalizationManager.GetMessage("deathText", UIJsonIdentifier);
        string deathAmount = globalStatistics.Deaths.ToString();
        deathMessage = deathMessage.Replace("%deaths%", deathAmount);
        deathsText.text = deathMessage;
    }

    private void UpdateTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = GetFormattedTime(globalStatistics.BestTime);
        timeMessage = timeMessage.Replace("%minutes%", formattedTime.Split(':')[0]);
        timeMessage = timeMessage.Replace("%seconds%", formattedTime.Split(':')[1]);
        bestTimeText.text = timeMessage;
    }

    public string GetFormattedTime(float timeAmount)
    {
        int minutes = Mathf.FloorToInt(timeAmount / 60F);
        int seconds = Mathf.FloorToInt(timeAmount % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void Update()
    {
        if(UICanvasGroup.alpha > 0 && UICanvasGroup.interactable)
        {
            // Quit to main menu...
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
                fadeUI.FadeOut();

                FadeUI.OnFadeOutComplete += OnFadeOutComplete;
                SetCanvasInteractivity(UICanvasGroup, false);
            }
        }
    }

    private IEnumerator PulseHelpText(TextMeshProUGUI helpText, float delay)
    {
        yield return new WaitForSeconds(delay);

        Color originalColor = helpText.color;
        float alpha = 0;

        while(true)
        {
            while(alpha < 1)
            {
                alpha += Time.deltaTime / flashInterval;
                helpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
                yield return null;
            }

            while(alpha > 0)
            {
                alpha -= Time.deltaTime / flashInterval;
                helpText.color = new Color(originalColor.r, originalColor.g, originalColor.b, Mathf.Clamp01(alpha));
                yield return null;
            }
        }
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;
        BackToMainMenu();
    }

    private void BackToMainMenu()
    {
        // Make main menu interactable again...
        MainMenuUI mainMenuUI = UIManager.GetUIComponent<MainMenuUI>();
        mainMenuUI.SetInteractable(true);

        // Fade into main menu...
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI.FadeIn();
        Show(false);
    }

    private void LoadJsonSettings()
    {
        flashInterval = (float)JsonData["flashInterval"];
        exitHelpText.text = LocalizationManager.GetMessage("exitHelpText", UIJsonIdentifier);
        statsLabelText.text = LocalizationManager.GetMessage("overallText", UIJsonIdentifier);
    }

    public override string UIJsonIdentifier => "stats_ui";
}
