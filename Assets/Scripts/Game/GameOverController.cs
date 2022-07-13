using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UniRx;
using SimpleQuickMenu;

public class GameOverController : MonoBehaviour
{
    public TextMeshProUGUI whyText; 
    
    public static string why = "よくわからないがやられた！";
    
    bool canMove;

    void OnEnable()
    {
        Fade.FadeOut().Subscribe(x => canMove = true);
        whyText.text = why;
    }

    void Update()
    {
        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                canMove = false;
                Fade.FadeIn().Subscribe(x =>
                {
                    GameController.first = false;
                    SceneManager.LoadScene("Stage" + GameController.lastPlayStageNum);
                });
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                canMove = false;
                Fade.FadeIn().Subscribe(x =>
                {
                    SceneManager.LoadScene("Ini");
                });
            }
        }
    }
}
