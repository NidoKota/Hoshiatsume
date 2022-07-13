using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class MainThreadDispatcherInstantiate
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void Instantiate()
    {
        GameObject go = new GameObject("MainThreadDispatcher");
        go.AddComponent<MainThreadDispatcher>();
        Object.DontDestroyOnLoad(go);
    }
}
