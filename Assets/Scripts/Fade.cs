using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using TMPro;
using UniRx;

/// <summary>
/// 簡単にFade処理を行えるClass
/// </summary>
public class Fade
{
    /// <summary>
    /// デフォルトのFadeの速さ
    /// </summary>
    public static float defaultSpeed { get { return 3f; } }
    
    /// <summary>
    /// デフォルトでフェードする画面暗転用Imageを取得
    /// </summary>
    public static Image DefaultImage()
    {
        return GameObject.Find("UICanvas/FadeImage").GetComponent<Image>();
    }

    //強制停止できるようにdisposableを保管する
    static Dictionary<object, IDisposable> disposables = new Dictionary<object, IDisposable>();

    //デフォルトのImageをデフォルトの速さでFadeIn
    public static IObservable<Unit> FadeIn(bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, DefaultImage(), defaultSpeed, 0, 1, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(DefaultImage())) disposables[DefaultImage()] = dis;
        else disposables.Add(DefaultImage(), dis);
        return sub;
    }
    //指定したObjectをデフォルトの速さでFadeIn
    public static IObservable<Unit> FadeIn<T>(T fadeObject, bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, defaultSpeed, 0, 1, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(fadeObject)) disposables[fadeObject] = dis;
        else disposables.Add(fadeObject, dis);
        return sub;
    }
    //指定したObjectを指定した速さでFadeIn
    public static IObservable<Unit> FadeIn<T>(T fadeObject, float fadeSpeed, bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, 0, 1, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(fadeObject)) disposables[fadeObject] = dis;
        else disposables.Add(fadeObject, dis);
        return sub;
    }

    //デフォルトのImageをデフォルトの速さでFadeOut
    public static IObservable<Unit> FadeOut(bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, DefaultImage(), defaultSpeed, 1, 0, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(DefaultImage())) disposables[DefaultImage()] = dis;
        else disposables.Add(DefaultImage(), dis);
        return sub;
    }
    //指定したObjectをデフォルトの速さでFadeOut
    public static IObservable<Unit> FadeOut<T>(T fadeObject, bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, defaultSpeed, 1, 0, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(fadeObject)) disposables[fadeObject] = dis;
        else disposables.Add(fadeObject, dis);
        return sub;
    }
    //指定したObjectを指定した速さでFadeOut
    public static IObservable<Unit> FadeOut<T>(T fadeObject, float fadeSpeed, bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, 1, 0, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(fadeObject)) disposables[fadeObject] = dis;
        else disposables.Add(fadeObject, dis);
        return sub;
    }

    //Object、速さ、最初と最後の透明度を自由に決定できる
    public static IObservable<Unit> FadeCustom<T>(T fadeObject, float fadeSpeed, float startAlpha, float endAlpha, bool unscaledTime = false)
    {
        Subject<Unit> sub = new Subject<Unit>();
        IDisposable dis = Observable.FromMicroCoroutine<Unit>(x => Fe(x, fadeObject, fadeSpeed, startAlpha, endAlpha, unscaledTime)).Subscribe(sub);
        if (disposables.ContainsKey(fadeObject)) disposables[fadeObject] = dis;
        else disposables.Add(fadeObject, dis);
        return sub;
    }

    //Fadeを強制停止する
    public static void ForceStop(object key)
    {
        if (disposables.ContainsKey(key)) disposables[key].Dispose();
    }

    //Fadeさせる
    static IEnumerator Fe<T>(IObserver<Unit> observer, T fadeObject, float speed, float firstAlfa, float endAlfa, bool unscaledTime)
    {
        float t = 0;
        while (true)
        {
            try
            {
                if(unscaledTime) t += Time.unscaledDeltaTime * speed;
                else t += Time.deltaTime * speed;
                if (typeof(T) == typeof(Image))
                {
                    Image image = fadeObject as Image;
                    image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                }
                else if (typeof(T) == typeof(CanvasGroup))
                {
                    CanvasGroup canvasGroup = fadeObject as CanvasGroup;
                    canvasGroup.alpha = Mathf.Lerp(firstAlfa, endAlfa, t);
                }
                else if (typeof(T) == typeof(SpriteRenderer))
                {
                    SpriteRenderer spriteRenderer = fadeObject as SpriteRenderer;
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                }
                else if (typeof(T) == typeof(Text))
                {
                    Text text = fadeObject as Text;
                    text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                }
                else if (typeof(T) == typeof(TextMeshPro) || typeof(T) == typeof(TextMeshProUGUI))
                {
                    TMP_Text text = fadeObject as TMP_Text;
                    text.color = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(firstAlfa, endAlfa, t));
                }
                else if (typeof(T) == typeof(ColorGrading))
                {
                    ColorGrading colorGrading = fadeObject as ColorGrading;
                    Color value = default;
                    value = new Color(Mathf.Lerp(endAlfa, firstAlfa, t), Mathf.Lerp(endAlfa, firstAlfa, t), Mathf.Lerp(endAlfa, firstAlfa, t), 0);
                    colorGrading.colorFilter.value = value;
                }
                else throw new Exception("未実装の型で実行されました");
            }
            catch { }
            if (t >= 1)
            {
                disposables.Remove(fadeObject);
                observer.OnNext(Unit.Default);
                observer.OnCompleted();
            }
            yield return null;
        }
    }
}
