using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to the player's home...
/// </summary>
public class PlayerHome : Entity
{
    [SerializeField] private Transform spawnPoint;
    private LevelManager levelManager;
    private TaskManager taskManager;
    private bool inLevelTransition = false;
    private Player playerEntity;

    public override void OnEntitySpawn()
    {
        taskManager = FindFirstObjectByType<TaskManager>();
        levelManager = FindFirstObjectByType<LevelManager>();
        playerEntity = levelManager.GetEntity<Player>();
        CheckSpawnPlayer();
    }

    private void CheckSpawnPlayer()
    {
        if(playerEntity != null)
        {
            playerEntity.EntityPosition = spawnPoint.position;
            playerEntity.SetParent(LevelGenerator.EntityParent.transform);
        }
        else
        {
            playerEntity = (Player)levelManager.AddEntity("player_dude", spawnPoint.position);
        }
    }

    protected override void OnEntityUpdate()
    {
        if(taskManager.IsTaskComplete && !inLevelTransition && (playerEntity && playerEntity.IsAlive))
        {
            if(Vector2.Distance(playerEntity.EntityPosition, spawnPoint.position) < 0.5f)
            {
                inLevelTransition = true;
                PlayerInput.InputEnabled = false;
                Invoke(nameof(ClearLevel), 1.0f);
            }
        }
    }

    private void ClearLevel() => levelManager.OnLevelClear();

    public override Vector2 CenterOfMass => EntityPosition;
}
