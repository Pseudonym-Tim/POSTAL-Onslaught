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
    public const float DISTURB_RANGE = 10.0f;
    private const float EMPTY_HOUSE_CHANCE = 0.25f;
    private const float EXIT_TIME_MIN = 5;
    private const float EXIT_TIME_MAX = 10;
    private const float CHECK_EXIT_RATE = 1;

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
        float exitTime = Random.Range(EXIT_TIME_MIN, EXIT_TIME_MAX);
        InvokeRepeating(nameof(TriggerNPCSpawn), exitTime, CHECK_EXIT_RATE);
    }

    public void TriggerNPCSpawn()
    {
        if(!hasBeenDisturbed && IsSpawnPointVisible())
        {
            hasBeenDisturbed = true;

            // Check to see if the house is empty...
            if(Random.value <= EMPTY_HOUSE_CHANCE)
            {
                return;
            }

            StartCoroutine(SpawnNPCs());
        }
    }

    private IEnumerator SpawnNPCs()
    {
        int npcSpawnCount = Random.Range(npcCountMin, npcCountMax);

        TaskManager taskManager = FindFirstObjectByType<TaskManager>();

        // Randomly pick one NPC type from the pool...
        JArray npcPool = (JArray)EntityData.jsonData["npcPool"];
        int randomIndex = Random.Range(0, npcPool.Count);
        string npcID = (string)npcPool[randomIndex];

        for(int i = 0; i < npcSpawnCount; i++)
        {
            // Level clear or gameover screen shown? Stop spawning...
            if(GameManager.IsLevelCleared || GameManager.IsGameOver)
            {
                yield break;
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

    private bool IsSpawnPointVisible()
    {
        if(Camera.main != null)
        {
            Vector3 viewportPosition = Camera.main.WorldToViewportPoint(spawnPoint.position);
            float viewportX = viewportPosition.x;
            float viewportZ = viewportPosition.z;
            float viewportY = viewportPosition.y;
            return viewportZ > 0 && viewportX > 0 && viewportX < 1 && viewportY > 0 && viewportY < 1;
        }

        return false;
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
