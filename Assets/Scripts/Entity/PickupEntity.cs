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

    public override void OnEntityAwake()
    {
        SetupEntityAnim();
        EntityAnim.Play("Idle");
        pickupGFX.material.SetColor("_Color", outlineColor);
    }

    public override void OnInteract()
    {

    }

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pickupGFX.transform.position, pickupRange);
    }

    public override bool IsInteractable => true;
}
