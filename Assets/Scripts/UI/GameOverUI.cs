using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the gameover screen...
/// </summary>
public class GameOverUI : UIComponent
{
    [SerializeField] private Canvas UICanvas;
    [SerializeField] private Animator killerAnimator;
    [SerializeField] private Image killerImage;
    [SerializeField] private List<TextMeshProUGUI> menuOptions;
    public TextMeshProUGUI killerLabelText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI tipText;
    private PlayerHUD playerHUD;
    private string inactiveColor;
    private string activeColor;
    private float hoverOffset;

    public override void SetupUI()
    {
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        killerLabelText.text = LocalizationManager.GetMessage("killerLabel", UIJsonIdentifier);
        LoadJsonSettings();
        Show(false);
        SetOptionsInactive();
    }

    public void Show(bool showUI = true)
    {
        UICanvas.enabled = showUI;
        if(!showUI) return;

        SetOptionsInactive();
        tipText.text = GetTipMessage();
        SetKillerAnimation();
        SetNativePivot(killerImage);
        SetTime();

        playerHUD.ShowHealthIndicator(false);
        playerHUD.ShowInventory(false);
        playerHUD.ShowWeaponSelection(false);
        playerHUD.ShowScoreMultiplier(false);
    }

    private void SetTime()
    {
        string timeMessage = LocalizationManager.GetMessage("timeText", UIJsonIdentifier);
        string formattedTime = GameManager.GetFormattedTime();
        timeMessage = timeMessage.Replace("%minutes%", formattedTime.Split(':')[0]);
        timeMessage = timeMessage.Replace("%seconds%", formattedTime.Split(':')[1]);
        timeText.text = timeMessage;
    }

    private void LateUpdate()
    {
        if(killerImage.sprite != null && UICanvas.enabled)
        {
            SetNativePivot(killerImage);
        }
    }

    private string GetTipMessage()
    {
        JArray tipMessages = (JArray)LocalizationManager.JsonData["tipMessages"];
        int randomIndex = Random.Range(0, tipMessages.Count);
        return (string)tipMessages[randomIndex];
    }

    public static void SetNativePivot(Image image)
    {
        Vector2 pivotPixel = image.sprite.pivot;
        Rect spriteRect = image.sprite.rect;
        Vector2 pivotNormalized = new Vector2(pivotPixel.x / spriteRect.width, pivotPixel.y / spriteRect.height);
        image.rectTransform.pivot = new Vector2(pivotNormalized.x, image.rectTransform.pivot.y);
    }

    public void SetKillerAnimation()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        Player playerEntity = levelManager.GetEntity<Player>();
        Entity attackerEntity = playerEntity.LastDamageInfo.attackerEntity;

        if(attackerEntity is NPC killerNPC)
        {
            AnimationClip killerClip = killerNPC.killerAnimation;
            AnimatorOverrideController overrideController = new AnimatorOverrideController(killerAnimator.runtimeAnimatorController)
            {
                name = killerAnimator.runtimeAnimatorController.name
            };

            List<KeyValuePair<AnimationClip, AnimationClip>> animOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(overrideController.overridesCount);
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
        }
    }

    private void SetOptionsInactive()
    {
        menuOptions[0].text = GetFormattedMessage("restartText", inactiveColor);
        menuOptions[1].text = GetFormattedMessage("quitText", inactiveColor);
    }

    public void SelectOption(int optionIndex)
    {
        switch(optionIndex)
        {
            case 0:
                Debug.Log("RESTART GAME!");
                // TODO: Restart the game...
                break;
            case 1:
                Debug.Log("QUIT TO MENU!");
                // TODO: Quit back to menu...
                break;
        }
    }

    public void HoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "restartText" : "quitText", activeColor);
        hoveredText.transform.localPosition += new Vector3(0, hoverOffset, 0);
    }

    public void UnhoverOption(int optionIndex)
    {
        TextMeshProUGUI hoveredText = menuOptions[optionIndex];
        hoveredText.text = GetFormattedMessage(optionIndex == 0 ? "restartText" : "quitText", inactiveColor);
        hoveredText.transform.localPosition -= new Vector3(0, hoverOffset, 0);
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
