using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles everything related to player tasks...
/// </summary>
public class TaskManager : Singleton<TaskManager>
{
    private List<NPC> currentNPCList = new List<NPC>();
    private int currentNPCAmount = 0;
    private int totalNPCAmount = 0;
    private PlayerHUD playerHUD;

    public void Setup()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        currentNPCList = levelManager.GetEntities<NPC>();
        currentNPCAmount = currentNPCList.Count;
        totalNPCAmount = currentNPCList.Count;
        NPC.OnNPCKilled += OnUpdateCount;
        playerHUD.UpdatePopulation(currentNPCAmount, totalNPCAmount);
    }

    public void OnUpdateCount()
    {
        currentNPCAmount--;
        playerHUD.UpdatePopulation(currentNPCAmount, totalNPCAmount);

        if(currentNPCAmount <= 0)
        {
            string endLevelMessage = LocalizationManager.GetMessage("endLevelMessage");
            playerHUD.UpdateTask(endLevelMessage);
        }
    }

    public bool IsTaskComplete => currentNPCAmount <= 0;
}
