using System;
using System.Collections;
using UnityEngine;
using UnityEditor;
using TMPro;

//FPSを計測する
public class FPSCounter : MonoBehaviour
{
    TextMeshPro fPSText;
    int frameCount;
    float prevTime;
    float fPS;
    static ForwardDisplay forwardDisplay;

    [RuntimeInitializeOnLoadMethod]
    static void Ini()
    {
        GameObject fPSView = new GameObject("FPSView");
        TextMeshPro textMeshPro = fPSView.AddComponent<TextMeshPro>();
        textMeshPro.alignment = TextAlignmentOptions.Center;
        textMeshPro.fontSize = 0.35f;
        fPSView.AddComponent<FPSCounter>();
        forwardDisplay = fPSView.AddComponent<ForwardDisplay>();
        forwardDisplay.cam = Camera.main.transform;
        forwardDisplay.position = new Vector3(0.58f, 0.83f, 1.5f);
        forwardDisplay.updatePosition = true;
        DontDestroyOnLoad(fPSView);
    }

    void Start()
    {
        fPSText = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        if (!forwardDisplay.cam) forwardDisplay.cam = Camera.main.transform;

        ++frameCount;

        float time = Time.realtimeSinceStartup - prevTime;

        if (time >= 0.5f)
        {
            fPS = frameCount / time;
            fPSText.text = $"{fPS:F0}fps";
            frameCount = 0;
            prevTime = Time.realtimeSinceStartup;
        }
        if (fPS >= 59) fPSText.color = Color.white;
        else fPSText.color = Color.red;
    }
}
