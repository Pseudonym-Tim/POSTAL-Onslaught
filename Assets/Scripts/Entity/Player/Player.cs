using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player entity related stuff...
/// </summary>
public class Player : Entity
{
    public override void OnEntitySpawn()
    {
        SetupEntityAnim();
        EntityAnim.Play("Idle");
    }

    protected override void OnEntityUpdate()
    {

    }
}
