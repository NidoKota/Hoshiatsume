using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

/// <summary>
/// シーンを読み込む
/// </summary>
public class LoadScene : MonoBehaviour
{
    public void Reload()
    {
       Fade.FadeIn(unscaledTime: true).Subscribe(x =>
       {
           Time.timeScale = 1;
           SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
       });
    }

    public void Load(string sceneName)
    {
        Fade.FadeIn(unscaledTime: true).Subscribe(x =>
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(sceneName);
        });
    }
}
