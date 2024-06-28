using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An entity that the player can interact with...
/// </summary>
public class InteractableEntity : Entity
{
    public virtual void OnInteract() { }
    public virtual bool IsInteractable => true;
    public virtual float InteractRange => 1.0f;
}
