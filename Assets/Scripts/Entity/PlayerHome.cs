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

    public override void OnLevelGenerated()
    {
        ExitIndicatorUI exitIndicatorUI = UIManager.GetUIComponent<ExitIndicatorUI>();
        exitIndicatorUI?.Initialize(CenterOfMass);
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
            if(Vector2.Distance(playerEntity.EntityPosition, spawnPoint.position) < 0.75f)
            {
                inLevelTransition = true;
                PlayerInput.InputEnabled = false;

                FadeUI fadeUI = UIManager.GetUIComponent<FadeUI>();
                fadeUI?.FadeOut();

                FadeUI.OnFadeOutComplete -= ClearLevel;
                FadeUI.OnFadeOutComplete += ClearLevel;
            }
        }
    }

    private void ClearLevel() => levelManager.OnLevelClear();

    public override Vector2 CenterOfMass => EntityPosition;
}
