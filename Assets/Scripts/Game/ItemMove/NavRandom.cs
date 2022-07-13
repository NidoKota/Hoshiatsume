using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavRandom : MonoBehaviour
{
    public Transform[] targets;

    int targetInt;
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
        if (gameController.firstStartTimelineFinished && !gameController.IsGameOver && !gameController.IsGameClear)
        {
            if (stageMove.selectAngle)
            {
                agent.enabled = false;
            }
            else
            {
                agent.enabled = true;

                if (Vector2.Distance(transform.position, targets[targetInt].position) < 0.5f) targetInt = Random.Range(0, targets.Length);

                agent.SetDestination(targets[targetInt].position);
            }
        }
        else
        {
            agent.enabled = false;
        }
    }
}
