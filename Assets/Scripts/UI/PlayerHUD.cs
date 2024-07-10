using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles everything related to the player HUD...
/// </summary>
public class PlayerHUD : UIComponent
{
    [Header("Health Indicator")]
    [SerializeField] private GameObject healthIndicatorUI;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TextMeshProUGUI healthLabelText;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Weapon selection")]
    [SerializeField] private GameObject weaponSelectorUI;
    [SerializeField] private Image currentWeaponImage;
    [SerializeField] private TextMeshProUGUI weaponSlotText;

    [Header("Inventory")]
    [SerializeField] private GameObject inventorySystemUI;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemText;

    [Header("Other")]
    [SerializeField] private Canvas playerHUDCanvas;
    [SerializeField] private UICrosshair UICrosshair;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highscoreText;
    [SerializeField] private TextMeshProUGUI scoreMultiplierText;
    [SerializeField] private TextMeshProUGUI killText;
    [SerializeField] private TextMeshProUGUI taskText;
    [SerializeField] private TextMeshProUGUI interactText;

    public override void SetupUI()
    {
        string healthLabel = LocalizationManager.GetMessage("healthLabel");
        healthLabelText.text = healthLabel;
        UpdateScore(0);
        UpdateKilled(0);
        ShowScoreMultiplier(false); 
        UpdateInteractionText(Vector2.zero, null, false);
        ShowHealthIndicator(true);
        ShowInventory(true);
        ShowWeaponSelection(true);
    }

    public void UpdateHealthIndicator(int healthAmount, int maxHealth)
    {
        healthBarImage.fillAmount = (float)healthAmount / maxHealth;
        string healthMessage = LocalizationManager.GetMessage("healthAmountText");
        healthMessage = healthMessage.Replace("%currentHealth%", $"{ healthAmount }");
        healthMessage = healthMessage.Replace("%maxHealth%", $"{ maxHealth }");
        healthText.text = healthMessage;
    }

    public void UpdateCurrentItem(Sprite itemSprite, int itemQuantity)
    {
        string itemMessage = LocalizationManager.GetMessage("itemText");
        string noItemMessage = LocalizationManager.GetMessage("noItemMessage");
        string itemQuantityMessage = itemQuantity > 0 ? itemQuantity.ToString() : noItemMessage;
        itemMessage = itemMessage.Replace("%itemMessage%", itemQuantityMessage);
        itemText.text = itemMessage;
        itemImage.sprite = itemSprite;
        itemImage.enabled = itemQuantity > 0 ? true : false;
    }

    public void UpdateInteractionText(Vector2 origin, string interactMessage, bool showText = true)
    {
        Vector2 worldPos = Camera.main.WorldToScreenPoint(origin);
        interactText.rectTransform.position = worldPos;
        interactText.text = interactMessage;
        interactText.enabled = showText;
    }

    public void UpdateCurrentItem(InventoryItem inventoryItem)
    {
        int itemQuantity = inventoryItem.itemQuantity;
        Sprite itemSprite = inventoryItem.itemData.sprite;
        UpdateCurrentItem(itemSprite, itemQuantity);
    }

    public void UpdatePopulation(int currentAmount, int totalAmount)
    {
        string populationMessage = LocalizationManager.GetMessage("populationText");
        populationMessage = populationMessage.Replace("%current%", currentAmount.ToString());
        populationMessage = populationMessage.Replace("%total%", totalAmount.ToString());
        UpdateTask(populationMessage);
    }

    public void UpdateTask(string taskMessage)
    {
        taskText.text = taskMessage;
    }

    public void UpdateScoreMultiplier(int scorePool, int killAmount, float alpha)
    {
        string scoreMessage = LocalizationManager.GetMessage("scoreMultiplierMessage");
        scoreMessage = scoreMessage.Replace("%scorePool%", scorePool.ToString("N0"));
        scoreMessage = scoreMessage.Replace("%kills%", killAmount.ToString());
        scoreMultiplierText.text = scoreMessage;
        Color currentColor = scoreMultiplierText.color;
        currentColor.a = alpha;
        scoreMultiplierText.color = currentColor;
    }

    public void UpdateHighscoreText(int currentHighscore)
    {
        string highscoreMessage = LocalizationManager.GetMessage("highscoreText");
        highscoreMessage = highscoreMessage.Replace("%highscore%", currentHighscore.ToString());
        highscoreText.text = highscoreMessage;
    }

    public void ShowScoreMultiplier(bool showValue = true)
    {
        Color currentColor = scoreMultiplierText.color;
        currentColor.a = 1;
        scoreMultiplierText.color = currentColor;
        scoreMultiplierText.enabled = showValue;
    }

    public void ShowWeaponSelection(bool showValue = true) => weaponSelectorUI.SetActive(showValue);
    public void ShowInventory(bool showValue = true) => inventorySystemUI.SetActive(showValue);
    public void ShowHealthIndicator(bool showValue = true) => healthIndicatorUI.SetActive(showValue);

    public void UpdateScore(int currentScore)
    {
        string scoreMessage = LocalizationManager.GetMessage("scoreText");
        scoreMessage = scoreMessage.Replace("%currentScore%", currentScore.ToString());
        scoreText.text = scoreMessage;
    }

    public void UpdateKilled(int killAmount)
    {
        string killsMessage = LocalizationManager.GetMessage("killsText");
        killsMessage = killsMessage.Replace("%currentKills%", killAmount.ToString());
        killText.text = killsMessage;
    }

    public void CreatePopupText(Vector2 spawnPos, string message, float scale = 1.0f)
    {
        PopupTextUI.Create(spawnPos, message, playerHUDCanvas.transform, scale);
    }

    public void UpdateWeaponSelection(int slotIndex, int weaponCount, Sprite weaponSprite)
    {
        // Update weapon slots...
        UpdateWeaponSlots(slotIndex, weaponCount);
        currentWeaponImage.sprite = weaponSprite;
    }

    private void UpdateWeaponSlots(int slotIndex, int weaponCount)
    {
        int slotNumber = slotIndex + 1;

        List<string> colors = new List<string>
        {
            (string)JsonData["weaponSlots"]["unavailableColor"],
            (string)JsonData["weaponSlots"]["inactiveColor"],
            (string)JsonData["weaponSlots"]["activeColor"]
        };

        int maxWeaponSlots = (int)JsonData["weaponSlots"]["maxSlots"];

        StringBuilder weaponSlotString = new StringBuilder();

        for(int i = 1; i <= maxWeaponSlots; i++)
        {
            int colorIndex = (i > weaponCount) ? 0 : (i == slotNumber) ? 2 : 1;
            weaponSlotString.Append($"<color={ colors[colorIndex] }>{ i }</color>");

            if(i < maxWeaponSlots)
            {
                weaponSlotString.Append(" ");
            }
        }

        weaponSlotText.text = weaponSlotString.ToString();
    }

    public override string UIJsonIdentifier => "player_hud";
}
