using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all pickup entities...
/// </summary>
public class PickupEntity : InteractableEntity
{
    public float pickupRange = 1f;
    public SpriteRenderer pickupGFX;
    [SerializeField] private Color outlineColor = Color.yellow;
    public string pickupSound;
    protected PlayerHUD playerHUD;
    protected SFXManager sfxManager;

    public override void OnEntityAwake()
    {
        SetupEntityAnim();
        EntityAnim.Play("Idle");
        pickupGFX.material.SetColor("_Color", outlineColor);
        sfxManager = FindFirstObjectByType<SFXManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
    }

    public override void OnInteract()
    {

    }

    protected void SpawnPickupText(string pickupName)
    {
        Vector3 spawnPos = EntityPosition + Vector3.up * pickupGFX.size.y;
        string popupMessage = LocalizationManager.GetMessage("pickupMessage");
        popupMessage = popupMessage.Replace("%pickupName%", pickupName);
        playerHUD.CreatePopupText(spawnPos, popupMessage);
    }

    protected override void OnDrawEntityGizmos()
    {
        if(pickupGFX != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pickupGFX.transform.position, pickupRange);
        }
    }

    public override string InteractMessage => LocalizationManager.GetMessage("pickupInteractMessage");
    public override float InteractRange => pickupRange;
    public override bool IsInteractable => true;
}
