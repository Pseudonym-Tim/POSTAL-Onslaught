using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles everything related to NPC navigation...
/// </summary>
public class NPCNavigation : MonoBehaviour
{
    private NPC currentNPCEntity;

    public void Setup(NPC npcEntity, int agentID)
    {
        currentNPCEntity = npcEntity;
        NavMeshAgent = npcEntity.AddComponent<NavMeshAgent>();
        NavMeshAgent.agentTypeID = agentID;
        NavMeshAgent.angularSpeed = 9999;
        NavMeshAgent.acceleration = 9999;
        NavMeshAgent.updateRotation = false;
        NavMeshAgent.updateUpAxis = false;
        NavMeshAgent.baseOffset = 0;
        NavMeshAgent.height = 1;
    }

    public void SetDestination(Vector2 destination, float stoppingDist = 0)
    {
        // NOTE: Hack to fix agent getting stuck while moving on Y axis!
        const float STUCKERY_OFFSET = 0.0001f;
        Vector2 fixedDestination = destination + (STUCKERY_OFFSET * Random.insideUnitCircle);
        NavMeshAgent.stoppingDistance = stoppingDist;
        NavMeshAgent.SetDestination(fixedDestination);
    }

    public static NavMeshObstacle CreateNavmeshObstacle(Entity entity, Vector2 size, bool stationaryOnly = true)
    {
        // Setup navmesh obstacle for us...
        NavMeshObstacle navMeshObstacle = entity.EntityObject.AddComponent<NavMeshObstacle>();
        navMeshObstacle.carving = true;
        navMeshObstacle.carveOnlyStationary = stationaryOnly;
        navMeshObstacle.center = entity.CenterOfMass;
        navMeshObstacle.size = new Vector3(size.x, size.y, 1);
        return navMeshObstacle;
    }

    public void RemoveNavmeshObstacle()
    {
        NavMeshObstacle navMeshObstacle = currentNPCEntity.GetComponent<NavMeshObstacle>();
        if(navMeshObstacle) { Destroy(navMeshObstacle); }
    }

    public void MoveAgent(Vector3 movePos) => NavMeshAgent.Move(movePos);

    public void WarpAgent(Vector3 warpPos, bool stopMoving = true)
    {
        if(stopMoving) { StopMoving(); }
        NavMeshAgent.Warp(warpPos);
    }

    public void SnapToNavMesh(float maxDistance = 1.0f)
    {
        NavMeshHit hit;

        if(NavMesh.SamplePosition(currentNPCEntity.EntityPosition, out hit, maxDistance, NavMesh.AllAreas))
        {
            WarpAgent(hit.position, false);
        }
    }

    public void StopMoving()
    {
        SetDestination(currentNPCEntity.EntityPosition, 0);
        NavMeshAgent.ResetPath();
    }

    public void Disable()
    {
        NavMeshAgent.enabled = false;
        NavMeshAgent.nextPosition = currentNPCEntity.EntityPosition;
    }

    public void Enable()
    {
        NavMeshAgent.nextPosition = currentNPCEntity.EntityPosition;
        NavMeshAgent.enabled = true;
    }

    public bool IsDestinationReached
    {
        get
        {
            if(!NavMeshAgent.pathPending)
            {
                if(NavMeshAgent.remainingDistance <= NavMeshAgent.stoppingDistance)
                {
                    if(!NavMeshAgent.hasPath || Mathf.Approximately(NavMeshAgent.velocity.sqrMagnitude, 0f))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    public bool IsMoving { get { return !IsDestinationReached; } }
    public float NavMeshAgentSpeed { get { return NavMeshAgent.speed; } set { NavMeshAgent.speed = value; } }
    public Vector2 NavMeshAgentVelocity { get { return NavMeshAgent.velocity; } }
    public float NavMeshAgentMagnitude { get { return NavMeshAgentVelocity.magnitude; } }
    public NavMeshAgent NavMeshAgent { get; set; } = null;
}
