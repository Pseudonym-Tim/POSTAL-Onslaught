using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// House that spawns NPC's when disturbed...
/// </summary>
public class NPCHome : Entity
{
    public const float DISTURB_RANGE = 8.0f;
    private const float EMPTY_HOUSE_CHANCE = 0.25f;

    [SerializeField] private int npcCountMin = 2;
    [SerializeField] private int npcCountMax = 4;
    [SerializeField] private float spawnDelayMin = 1;
    [SerializeField] private float spawnDelayMax = 2;
    [SerializeField] private Transform spawnPoint;

    private LevelManager levelManager;
    private bool hasBeenDisturbed = false;

    public override void OnEntityAwake()
    {
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    public void TriggerNPCSpawn()
    {
        if(!hasBeenDisturbed)
        {
            // Check to see if the house is empty...
            if(Random.value <= EMPTY_HOUSE_CHANCE)
            {
                hasBeenDisturbed = true;
                return;
            }

            StartCoroutine(SpawnNPCCoroutine());
            hasBeenDisturbed = true;
        }
    }

    private IEnumerator SpawnNPCCoroutine()
    {
        int npcSpawnCount = Random.Range(npcCountMin, npcCountMax);

        TaskManager taskManager = FindFirstObjectByType<TaskManager>();
        LevelClearUI levelClearUI = UIManager.GetUIComponent<LevelClearUI>();
        GameOverUI gameOverUI = UIManager.GetUIComponent<GameOverUI>();

        // Randomly pick one NPC type from the pool...
        JArray npcPool = (JArray)EntityData.jsonData["npcPool"];
        int randomIndex = Random.Range(0, npcPool.Count);
        string npcID = (string)npcPool[randomIndex];

        for(int i = 0; i < npcSpawnCount; i++)
        {
            // Level clear or gameover screen shown? Stop spawning...
            if(levelClearUI.UICanvas.enabled || gameOverUI.UICanvas.enabled)
            {
                break;
            }

            Vector2 spawnPosition = GetValidPosition(spawnPoint.position);

            if(spawnPosition == Vector2.zero)
            {
                Debug.LogWarning("No valid NavMesh position found near spawn point...");
                continue;
            }

            NPC npcEntity = (NPC)levelManager.AddEntity(npcID, spawnPosition);
            taskManager.AddPopulation(npcEntity);
            float waitTime = Random.Range(spawnDelayMin, spawnDelayMax);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private Vector2 GetValidPosition(Vector2 origin)
    {
        NavMeshHit hit;

        if(NavMesh.SamplePosition(origin, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return Vector2.zero;
    }

    protected override void OnDrawEntityGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(CenterOfMass, DISTURB_RANGE);
    }
}
