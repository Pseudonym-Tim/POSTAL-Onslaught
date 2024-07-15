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
    private LevelClearUI levelClearUI;

    private void Awake()
    {
        NPC.OnNPCKilled += OnUpdateCount;
    }

    public void Setup()
    {
        LevelManager levelManager = FindFirstObjectByType<LevelManager>();
        playerHUD = UIManager.GetUIComponent<PlayerHUD>();
        levelClearUI = UIManager.GetUIComponent<LevelClearUI>();
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
            Invoke(nameof(ShowLevelClearUI), 1.0f);
        }
    }

    private void ShowLevelClearUI()
    {
        levelClearUI.Show(true);
    }

    public bool IsTaskComplete => currentNPCAmount <= 0;
}
