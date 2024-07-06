using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player interaction...
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerInteraction : MonoBehaviour
{
    private const float MAX_INTERACT_RADIUS = 10f;
    private const float UPDATE_INTERVAL = 0.25f;

    private Player playerEntity;
    private PlayerHUD playerHUD;
    private InteractableEntity currentInteractable;
    private LevelManager levelManager;

    private void Awake()
    {
        playerEntity = GetComponent<Player>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        levelManager = FindFirstObjectByType<LevelManager>();
        InvokeRepeating(nameof(UpdateInteraction), 0.1f, UPDATE_INTERVAL);
    }

    private void Update()
    {
        if(PlayerInput.Interact && currentInteractable != null)
        {
            currentInteractable.OnInteract();
        }

        UpdateInteractMessage();
    }

    private void UpdateInteraction()
    {
        if(!playerEntity.IsAlive)
        {
            currentInteractable = null;
            CancelInvoke(nameof(UpdateInteraction));
            return;
        }

        List<InteractableEntity> interactableEntities = levelManager.GetEntities<InteractableEntity>();
        float nearestDistance = float.MaxValue;
        currentInteractable = null;

        foreach(InteractableEntity interactableEntity in interactableEntities)
        {
            if(interactableEntity != null && interactableEntity.IsInteractable)
            {
                float distance = Vector2.Distance(playerEntity.CenterOfMass, interactableEntity.CenterOfMass);

                if(distance <= interactableEntity.InteractRange && distance <= MAX_INTERACT_RADIUS && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    currentInteractable = interactableEntity;
                }
            }
        }
    }

    private void UpdateInteractMessage()
    {
        if(currentInteractable != null)
        {
            Vector2 messagePosition = currentInteractable.CenterOfMass + Vector2.down * 1.25f;
            string interactMessage = GetInteractMessage();
            playerHUD.UpdateInteractionText(messagePosition, interactMessage, true);
        }
        else
        {
            playerHUD.UpdateInteractionText(Vector2.zero, null, false);
        }
    }

    private string GetInteractMessage()
    {
        string baseMessage = LocalizationManager.GetMessage("interactButton");
        string buttonString = InputManager.GetButtonString("Interact");
        string interactButtonMessage = baseMessage.Replace("%interactButton%", buttonString);
        return $"{ currentInteractable.InteractMessage } { interactButtonMessage }";
    }
}
