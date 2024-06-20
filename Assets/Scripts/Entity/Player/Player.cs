using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player entity related stuff...
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class Player : Entity
{
    private PlayerMovement playerMovement;

    public override void OnEntityAwake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnEntitySpawn()
    {
        PlayerInput.InputEnabled = true;
        SetupEntityAnim();
        EntityAnim.Play("Idle");
    }

    protected override void OnEntityUpdate()
    {
        playerMovement.UpdateMovement();
        EntityAnim.SetBool("isMoving", playerMovement.IsMoving);
    }

    public PlayerCamera PlayerCamera { get; set; } = null;
}
