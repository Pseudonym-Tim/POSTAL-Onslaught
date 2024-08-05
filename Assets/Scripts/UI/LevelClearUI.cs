using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles everything related to the level clear screen...
/// </summary>
public class LevelClearUI : UIComponent
{
    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    [SerializeField] private TextMeshProUGUI killstreakText;
    [SerializeField] private TextMeshProUGUI areaCoveredText;
    [SerializeField] private TextMeshProUGUI creativityText;
    [SerializeField] private TextMeshProUGUI bonusText;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private TextMeshProUGUI continueText;
    private ScoreManager scoreManager;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private Vector2 originalOptionPosition;

    public override void SetupUI()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        originalOptionPosition = continueText.transform.localPosition;
        LoadJsonSettings();
        Show(false);
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        SetCanvasInteractivity(UICanvasGroup, showUI);

        if(!showUI) 
        { 
            continueText.transform.localPosition = originalOptionPosition;
            return;
        }

        PlayerInput.InputEnabled = false;

        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        fadeUI?.FadeIn();

        // Make sure pause menu is closed...
        PauseUI pauseUI = UIManager.GetUIComponent<PauseUI>();
        pauseUI.Show(false);

        string bonusMessage = LocalizationManager.GetMessage("bonusText", UIJsonIdentifier);
        string totalMessage = LocalizationManager.GetMessage("totalText", UIJsonIdentifier);
        bonusText.text = bonusMessage.Replace("%points%", "???");
        totalText.text = totalMessage.Replace("%points%", "???");

        continueText.text = GetFormattedMessage("continueText", inactiveColor);
        SetClearTime();
        UpdateKillstreak();
        UpdateAreaCovered();
        UpdateCreativity();

        StartCoroutine(AnimateScores()); 
    }

    private IEnumerator AnimateScores()
    {
        continueText.enabled = false;
        yield return StartCoroutine(AnimateBonusText());
        yield return StartCoroutine(AnimateTotalText());
        continueText.enabled = true;
    }

    private IEnumerator AnimateBonusText()
    {
        string bonusMessage = LocalizationManager.GetMessage("bonusText", UIJsonIdentifier);
        int bonusAmount = scoreManager.CalculateBonusScore();
        int currentScore = 0;
        float duration = 2.0f; // Duration of the count-up animation
        float elapsed = 0;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentScore = Mathf.FloorToInt(Mathf.Lerp(0, bonusAmount, elapsed / duration));
            bonusText.text = bonusMessage.Replace("%points%", currentScore.ToString("N0"));
            yield return null;
        }

        bonusText.text = bonusMessage.Replace("%points%", bonusAmount.ToString("N0"));
        scoreManager.AwardBonusScore();
    }

    private IEnumerator AnimateTotalText()
    {
        string totalMessage = LocalizationManager.GetMessage("totalText", UIJsonIdentifier);
        int scoreAmount = scoreManager.CurrentScore;
        int currentScore = 0;
        float duration = 2.0f; // Duration of the count-up animation
        float elapsed = 0;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentScore = Mathf.FloorToInt(Mathf.Lerp(0, scoreAmount, elapsed / duration));
            totalText.text = totalMessage.Replace("%points%", currentScore.ToString("N0"));
            yield return null;
        }

        totalText.text = totalMessage.Replace("%points%", scoreAmount.ToString("N0"));
    }

    public void ContinueGame()
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
        LevelManager levelManager = FindFirstObjectByType<LevelManager>(); 
        levelManager.NextLevel();
    }

    public void HoverContinueOption()
    {
        continueText.text = GetFormattedMessage("continueText", activeColor);
        continueText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverContinueOption()
    {
        continueText.text = GetFormattedMessage("continueText", inactiveColor);
        continueText.transform.localPosition -= new Vector3(0, hoverOffset, 0);
    }

    private void UpdateCreativity()
    {
        string creativityMessage = LocalizationManager.GetMessage("creativityText", UIJsonIdentifier);
        int uniqueWeaponCount = LevelManager.LevelStats.UniqueWeaponsUsed;
        int maxCreativity = KillCreativityManager.MAX_CREATIVITY;
        creativityMessage = creativityMessage.Replace("%rating%", uniqueWeaponCount.ToString());
        creativityMessage = creativityMessage.Replace("%max%", maxCreativity.ToString());
        creativityText.text = creativityMessage;
    }

    private void UpdateAreaCovered()
    {
        string areaConveredMessage = LocalizationManager.GetMessage("areaCoveredText", UIJsonIdentifier);
        float distanceCovered = LevelManager.LevelStats.DistanceCovered;
        areaConveredMessage = areaConveredMessage.Replace("%metersWalked%", distanceCovered.ToString("F2"));
        areaCoveredText.text = areaConveredMessage;
    }

    private void UpdateKillstreak()
    {
        string killstreakMessage = LocalizationManager.GetMessage("killstreakText", UIJsonIdentifier);
        int killstreakCount = LevelManager.LevelStats.HighestKillstreak;
        killstreakMessage = killstreakMessage.Replace("%count%", killstreakCount.ToString());
        killstreakText.text = killstreakMessage;
    }

    private void SetClearTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = LevelManager.GetFormattedTime();
        timeMessage = timeMessage.Replace("%minutes%", formattedTime.Split(':')[0]);
        timeMessage = timeMessage.Replace("%seconds%", formattedTime.Split(':')[1]);
        clearTimeText.text = timeMessage;
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

    public override string UIJsonIdentifier => "level_clear_ui";
}
