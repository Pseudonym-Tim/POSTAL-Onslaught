using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player interaction...
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerInteraction : MonoBehaviour
{
    private const float UPDATE_RATE = 0.5f / 2;
    private Player playerEntity;
    private InteractableEntity currentInteractableEntity;
    private LevelManager levelManager;

    private void Awake()
    {
        playerEntity = GetComponent<Player>();
        levelManager = FindFirstObjectByType<LevelManager>();
        InvokeRepeating(nameof(UpdateInteraction), 0.1f, UPDATE_RATE);
    }

    private void Update()
    {
        if(PlayerInput.Interact && currentInteractableEntity != null)
        {
            currentInteractableEntity.OnInteract();
        }
    }

    private void UpdateInteraction()
    {
        if(!playerEntity.IsAlive)
        {
            currentInteractableEntity = null;
            CancelInvoke(nameof(UpdateInteraction));
            return;
        }

        const float MAX_INTERACT_RADIUS = 10f;
        List<InteractableEntity> interactableEntities = levelManager.GetEntities<InteractableEntity>();
        float nearestDistance = float.MaxValue;
        currentInteractableEntity = null;

        foreach(InteractableEntity interactableEntity in interactableEntities)
        {
            if(interactableEntity != null && interactableEntity.IsInteractable)
            {
                float distance = Vector2.Distance(playerEntity.CenterOfMass, interactableEntity.CenterOfMass);

                if(distance <= interactableEntity.InteractRange && distance <= MAX_INTERACT_RADIUS && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    currentInteractableEntity = interactableEntity;
                }
            }
        }
    }
}
