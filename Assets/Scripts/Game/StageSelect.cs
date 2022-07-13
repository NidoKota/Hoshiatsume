using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

public class StageSelect : MonoBehaviour
{
    [Min(1)]
    public int stageNum;

    public static int stageCleared = 0;

    void Start()
    {
        if ((stageCleared & (1 << stageNum)) == (1 << stageNum)) name += " (Cleared!)";
    }

    public void Load()
    {
        Fade.FadeIn(unscaledTime: true).Subscribe(x =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Stage" + stageNum);
        });
    }
}
