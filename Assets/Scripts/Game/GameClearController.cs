using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using SimpleQuickMenu;

public class GameClearController : MonoBehaviour
{
    public GameObject hideNextStage;
    public int maxStageNum;
    public string nextSceneName;

    SimpleQuickMenuController menu;
    bool canMove;

    void OnEnable()
    {
        Fade.FadeOut().Subscribe(x => canMove = true);
        menu = FindObjectOfType<SimpleQuickMenuController>();
        if (maxStageNum <= GameController.lastPlayStageNum) hideNextStage.SetActive(false);
    }

    void Update()
    {
        if (canMove)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow) && maxStageNum > GameController.lastPlayStageNum)
            {
                canMove = false;
                Fade.FadeIn().Subscribe(x =>
                {
                    GameController.first = true;
                    SceneManager.LoadScene("Stage" + (GameController.lastPlayStageNum + 1));
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
