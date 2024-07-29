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

    private void Awake()
    {
        NPC.OnNPCKilled += OnUpdateCount;
    }

    public void Setup()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        currentNPCList = levelManager.GetEntities<NPC>();
        currentNPCAmount = currentNPCList.Count;
        totalNPCAmount = currentNPCList.Count;
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
            Invoke(nameof(ClearLevel), 1.0f);
        }
    }

    public void AddPopulation(NPC npcToAdd)
    {
        currentNPCList.Add(npcToAdd);
        currentNPCAmount++;
        totalNPCAmount = currentNPCList.Count;
        playerHUD.UpdatePopulation(currentNPCAmount, totalNPCAmount);
    }

    private void ClearLevel()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        levelManager.OnLevelClear();
    }

    public bool IsTaskComplete => currentNPCAmount <= 0;
}
