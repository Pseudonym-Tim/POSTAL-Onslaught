using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all NPC's...
/// </summary>
public class NPC : Entity
{
    [SerializeField] protected float moveSpeed = 10;
    protected Player playerEntity;
    protected SpriteRenderer npcGFX;

    public override Vector3 CenterOfMass => EntityPosition;
}
