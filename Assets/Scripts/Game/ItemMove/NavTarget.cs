using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavTarget : MonoBehaviour
{
    public Transform target;

    NavMeshAgent agent;
    StageMove stageMove;
    GameController gameController;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        stageMove = FindObjectOfType<StageMove>();
        gameController = FindObjectOfType<GameController>();
    }

    void Update()
    {
        if(gameController.firstStartTimelineFinished && !gameController.IsGameOver && !gameController.IsGameClear)
        {
            if (stageMove.selectAngle)
            {
                agent.enabled = false;
            }
            else
            {
                agent.enabled = true;
                agent.SetDestination(target.position);
            }
        }
        else
        {
            agent.enabled = false;
        }
    }
}