using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UniRx;
using SimpleQuickMenu;

public class IniController : MonoBehaviour
{
    public string sceneName;

    SimpleQuickMenuController menu;
    StartButton startButton;
    bool canMove;

    void OnEnable()
    {
        Fade.FadeOut().Subscribe(x => canMove = true);
        GameController.first = true;

        menu = FindObjectOfType<SimpleQuickMenuController>();
        startButton = FindObjectOfType<StartButton>();

        Screen.SetResolution(960, 1280, false, 60);
    }

    void Update()
    {
        if (canMove && !menu.View && startButton.down)
        {
            menu.ViewMenu();

#if !UNITY_EDITOR
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
#endif
        }

#if !UNITY_EDITOR
        if (!menu.View)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
#endif
    }
}
