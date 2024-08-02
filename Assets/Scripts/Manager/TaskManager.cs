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
    private LevelManager levelManager;

    public void Setup()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        currentNPCList = levelManager.GetEntities<NPC>();
        currentNPCAmount = currentNPCList.Count;
        totalNPCAmount = currentNPCList.Count;
        playerHUD.UpdatePopulation(currentNPCAmount, totalNPCAmount);

        NPC.OnNPCKilled -= OnUpdateCount;
        NPC.OnNPCKilled += OnUpdateCount;
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

    public void AddPopulation(NPC npcToAdd)
    {
        currentNPCList.Add(npcToAdd);
        currentNPCAmount++;
        totalNPCAmount = currentNPCList.Count;
        playerHUD.UpdatePopulation(currentNPCAmount, totalNPCAmount);
    }

    public bool IsTaskComplete => currentNPCAmount <= 0;
}
