using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorMove : MonoBehaviour
{
    GameController gameController;
    Animator animator;
    
    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (gameController.firstStartTimelineFinished) animator.SetFloat("Speed", 1);
        if (gameController.IsGameOver || gameController.IsGameClear) animator.SetFloat("Speed", 0);
    }
}
