using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the gameover screen...
/// </summary>
public class GameOverUI : UIComponent
{
    public Canvas UICanvas;
    public CanvasGroup UICanvasGroup;
    [SerializeField] private Animator killerAnimator;
    [SerializeField] private Image killerImage;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    public TextMeshProUGUI killerLabelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI tipText;
    private PlayerHUD playerHUD;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;
    private int selectedOptionIndex = 0;
    private int? currentlyHoveredOption = null;

    public override void SetupUI()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        killerLabelText.text = LocalizationManager.GetMessage("killerLabel", UIJsonIdentifier);
        LoadJsonSettings();
        Show(false);
        SetOptionsInactive();
        currentlyHoveredOption = null;
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        SetCanvasInteractivity(UICanvasGroup, showUI);

        if(!showUI) 
        { 
            if(currentlyHoveredOption.HasValue)
            {
                UnhoverOption(currentlyHoveredOption.Value);
                currentlyHoveredOption = null;
            }

            selectedOptionIndex = 0;
            return; 
        }

        string scoreMessage = LocalizationManager.GetMessage("scoreText", UIJsonIdentifier);
        scoreText.text = scoreMessage.Replace("%points%", "???");

        SFXManager sfxManager = FindFirstObjectByType<SFXManager>();
        sfxManager?.Play2DSound("Onslaught Fail");

        SetOptionsInactive();
        tipText.text = GetTipMessage();
        UpdateKillerAnimation();
        UpdateTime();

        playerHUD.ShowHealthIndicator(false);
        playerHUD.ShowInventory(false);
        playerHUD.ShowWeaponSelection(false);
        playerHUD.ShowScoreMultiplier(false);

        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        StartCoroutine(UpdateScoreText(scoreManager));
    }

    private void UpdateTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = GameManager.GetFormattedTime();
        timeMessage = timeMessage.Replace("%minutes%", formattedTime.Split(':')[0]);
        timeMessage = timeMessage.Replace("%seconds%", formattedTime.Split(':')[1]);
        timeText.text = timeMessage;
    }

    private IEnumerator UpdateScoreText(ScoreManager scoreManager)
    {
        // HACK: Wait a bit for the immediate killstreak award to be given...
        yield return new WaitForSeconds(1 / 2);
        string totalMessage = LocalizationManager.GetMessage("scoreText", UIJsonIdentifier);
        int scoreAmount = scoreManager.CurrentScore;
        int currentScore = 0;
        float duration = 1.0f; // Duration of the count-up animation
        float elapsed = 0;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentScore = Mathf.FloorToInt(Mathf.Lerp(0, scoreAmount, elapsed / duration));
            scoreText.text = totalMessage.Replace("%points%", currentScore.ToString("N0"));
            yield return null;
        }

        scoreText.text = totalMessage.Replace("%points%", scoreAmount.ToString("N0"));
    }

    private void LateUpdate()
    {
        if(killerImage.sprite != null && UICanvas.enabled)
        {
            ImageHelper.SetNativeSize(killerImage, 5f);
            ImageHelper.SetNativePivot(killerImage, ImageHelper.PivotAxis.X);
        }
    }

    private string GetTipMessage()
    {
        JArray tipMessages = (JArray)LocalizationManager.JsonData["tipMessages"];
        int randomIndex = Random.Range(0, tipMessages.Count);
        return (string)tipMessages[randomIndex];
    }

    public void UpdateKillerAnimation()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Player playerEntity = levelManager.GetEntity<Player>();
        Entity attackerEntity = playerEntity.LastDamageInfo.attackerEntity;

        if(attackerEntity is NPC killerNPC)
        {
            AnimationClip killerClip = killerNPC.NPCInfo.killerAnimation;

            AnimatorOverrideController overrideController = new AnimatorOverrideController(killerAnimator.runtimeAnimatorController)
            {
                name = killerAnimator.runtimeAnimatorController.name
            };

            List<KeyValuePair<AnimationClip, AnimationClip>> animOverrides; 
            animOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
            overrideController.GetOverrides(animOverrides);

            for(int i = 0; i < animOverrides.Count; i++)
            {
                if(animOverrides[i].Key.name == "killer_idle")
                {
                    animOverrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(animOverrides[i].Key, killerClip);
                    break;
                }
            }

            overrideController.ApplyOverrides(animOverrides);
            killerAnimator.runtimeAnimatorController = overrideController;
            killerAnimator.Play("KillerIdle");
        }
    }

    private void SetOptionsInactive()
    {
        menuOptions[0].text = GetFormattedMessage("restartText", inactiveColor);
        menuOptions[1].text = GetFormattedMessage("quitText", inactiveColor);
    }

    public void SelectOption(int optionIndex)
    {
        SetCanvasInteractivity(UICanvasGroup, false);
        FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
        FadeUI.OnFadeOutComplete += OnFadeOutComplete;
        fadeUI.FadeOut();

        selectedOptionIndex = optionIndex;
    }

    private void OnFadeOutComplete()
    {
        FadeUI.OnFadeOutComplete -= OnFadeOutComplete;

        switch(selectedOptionIndex)
        {
            case 0:
                GameManager.RestartGame();
                break;
            case 1:
                GameManager.QuitToMainMenu();
                break;
        }

        Show(false);
    }

    public void HoverOption(int optionIndex)
    {
        if(currentlyHoveredOption == optionIndex) { return; }

        if(currentlyHoveredOption.HasValue)
        {
            UnhoverOption(currentlyHoveredOption.Value);
        }

        currentlyHoveredOption = optionIndex;

        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "restartText" : "quitText", activeColor);
        hoveredText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        if(currentlyHoveredOption != optionIndex) { return; }

        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "restartText" : "quitText", inactiveColor);
        hoveredText.transform.localPosition -= new Vector3(0, hoverOffset, 0);

        currentlyHoveredOption = null;
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

    public override string UIJsonIdentifier => "gameover_ui";
}
