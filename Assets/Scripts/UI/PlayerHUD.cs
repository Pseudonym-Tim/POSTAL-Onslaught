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
    [SerializeField] private Image healthBarImage;
    [SerializeField] private TextMeshProUGUI healthLabelText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private HUDCrosshair hudCrosshair;

    [Header("Weapon selection")]
    [SerializeField] private Image currentWeaponImage;
    [SerializeField] private TextMeshProUGUI weaponSlotText;

    public override void Setup()
    {
        healthLabelText.text = (string)JsonData["healthLabel"];
    }

    public void UpdateHealthIndicator(int healthAmount, int maxHealth)
    {
        healthBarImage.fillAmount = (float)healthAmount / maxHealth;
        string healthMessage = JsonData["healthAmountText"].ToString();
        healthMessage = healthMessage.Replace("%currentHealth%", $"{ healthAmount }");
        healthMessage = healthMessage.Replace("%maxHealth%", $"{ maxHealth }");
        healthText.text = healthMessage;
    }

    private void Update()
    {
        hudCrosshair.UpdatePosition();
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

        StringBuilder weaponSlotString = new StringBuilder();

        for(int i = 1; i <= WeaponManager.MAX_SLOTS; i++)
        {
            int colorIndex = (i > weaponCount) ? 0 : (i == slotNumber) ? 2 : 1;
            weaponSlotString.Append($"<color={ colors[colorIndex] }>{ i }</color>");

            if(i < WeaponManager.MAX_SLOTS)
            {
                weaponSlotString.Append(" ");
            }
        }

        weaponSlotText.text = weaponSlotString.ToString();
    }

    public override string UIJsonIdentifier => "player_hud";
}
