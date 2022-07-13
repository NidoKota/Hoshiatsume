using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;
using UnityEngine.Animations;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SimpleQuickMenu
{
    /// <summary>
    /// SimpleQuickMenuを制御するClass
    /// </summary>
    [ExecuteAlways, RequireComponent(typeof(Animator)), RequireComponent(typeof(CanvasGroup))]
    public class SimpleQuickMenuController : MonoBehaviour, IAnimationClipSource
    {
        /// <summary>
        /// 決定の処理を実行する時に呼ばれるCallBack(Transformはメニューの階層)
        /// </summary>
        public event Action<Transform> InvokeMenuCallBack;

        /// <summary>
        /// 現在選択中のMenuの階層
        /// </summary>
        public Transform CurrentMenuHierarchy { get; private set; }

        /// <summary>
        /// メニューが表示されているか
        /// </summary>
        public bool View { get; set; }

        [Header("Component")]
        [SerializeField] Image selecter = default;                      //選択を示すImage
        [SerializeField] TextMeshProUGUI titleText = default;           //タイトルを表示するTMPro
        [SerializeField] TextMeshProUGUI menuText = default;            //メニューを表示するTMPro

        [Header("Setting")]
        [SerializeField] Transform menuHierarchy = default;             //メニューの階層をGameObjectの階層で設定する
        [SerializeField] float lineHeight = 0;                          //メニューの1行の高さ
        [SerializeField] float lineSpaceHeight = 0;                     //行の間の空白の高さ
        [SerializeField] float firstSelectY = 0;                        //メニューの1行目のY座標
        [SerializeField] float multiplyTextLength = 0;                  //文字の長さに掛けてSelecterのX軸の大きさを調整する
        [SerializeField] float smooth = 0;                              //移動やフェードのスムーズ係数
        [SerializeField] float selectWaitTime = 0;                      //menuTextが移動したら数秒待つ
        [SerializeField] bool notUseCancelKey = false;                  
        [SerializeField] AnimationClip selectAnimationClip = default;   //menuTextが移動した時に再生するAnimationClip
        [SerializeField] AnimationClip decisionAnimationClip = default; //決定した時に再生するAnimationClip
        [SerializeField] AnimationClip cancelAnimationClip = default;   //キャンセルしたときに再生するAnimationClip

        [Header("AnimationProperty")]
        [SerializeField] float animationSelecterScaleX = 0;             //SelecterのX軸の大きさを、0に近いほど文字数の長さから計算された数値に、1に近いほどtargetScaleXの数値にする
        [SerializeField] float targetScaleX = 0;                        //animationSelecterScaleXが1の時のSelecterのX軸の大きさ
        [SerializeField] float addSelecterScaleX = 0;                   //SelecterのX軸の大きさに加算する

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] int selectPreview = 0;                          //selecterの選択のプレビュー
        [SerializeField] bool viewInfo = false;                         //デバッグ情報を表示する
#endif

        State state;                                                    //現在の状態を表すステート
        CanvasGroup canvasGroup;                                        //メニュー全体をフェードさせる
        MatchCollection menuTextLineMatch;                            //menuTextを行ごとに分ける
        TMPLineColorController colorController;
        AnimationPlayableController animController;
        bool waitOnce;                                                  //menuTextが移動したら一度だけ待つ
        float selectWaitTimer;                                          //menuTextが移動したら時間を計る
        int menuTextLineCount;                                          //menuTextの行数
        string menuTextChecker;

        /// <summary>
        /// 選択中の行数(CurrentMenuHierarchyの情報を変更したり渡すProperty)
        /// </summary>
        int SelectLine
        {
            get { return CurrentMenuHierarchy.GetSiblingIndex(); }
            set { CurrentMenuHierarchy = CurrentMenuHierarchy.parent.GetChild(value); }
        }

        void Start()
        {
#if UNITY_EDITOR
            //ゲーム上でのみ実行する
            if (EditorApplication.isPlaying)
#endif
            {
                //Component取得
                canvasGroup = GetComponent<CanvasGroup>();

                //初期化
                canvasGroup.alpha = 0;
                selecter.transform.localScale = new Vector3(0, selecter.transform.localScale.y, selecter.transform.localScale.z);
                CurrentMenuHierarchy = menuHierarchy.GetChild(0);
                
                animController = new AnimationPlayableController(GetComponent<Animator>(), "SimpleQuickMenu", selectAnimationClip, decisionAnimationClip, cancelAnimationClip);
                colorController = new TMPLineColorController(menuText);
                UpdateText();
                menuTextChecker = menuText.text;

                menuText.transform.localPosition = Vector3.up * firstSelectY + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;
                for (int index = 2; index <= -3; index--) colorController.SetLineVertexColor(index, Color.clear, Color.clear, 1);
            }
        }

        //AnimationでPropertyを変更してから計算するためにLateUpdateにする
        void LateUpdate()
        {
#if UNITY_EDITOR
            //ゲーム上でのみ実行する
            if (EditorApplication.isPlaying)
#endif
            {
                //メニューが表示されている時
                if (View)
                {
                    canvasGroup.blocksRaycasts = true;

                    //選択している行の文字列の横の長さを計算
                    float textScale = GetTextLength(menuTextLineMatch[SelectLine].Groups["v"].Value, menuText.font);
                    //AnimationPropertyの値からSelecterのX軸の大きさを計算する
                    selecter.transform.localScale = new Vector3((targetScaleX - textScale) * animationSelecterScaleX + textScale + addSelecterScaleX, selecter.transform.localScale.y, selecter.transform.localScale.z);

                    //選択中
                    if (state == State.Select)
                    {
                        //menuTextを移動させたら指定時間待つ
                        if (waitOnce)
                        {
                            selectWaitTimer += Time.unscaledDeltaTime;
                            if (selectWaitTimer >= selectWaitTime)
                            {
                                selectWaitTimer = 0;
                                waitOnce = false;
                            }
                        }
                        else
                        {
                            //上に移動させる
                            if (Input.GetKey(KeyCode.UpArrow))
                            {
                                //animController.PlayAnimation(selectAnimationClip);

                                //一番下から場外へ移動する時
                                if (0 > SelectLine - 1) { } //SelectLine = menuTextLineCount - 1;
                                else
                                {
                                    animController.PlayAnimation(selectAnimationClip);
                                    SelectLine--;
                                }
                                waitOnce = true;
                            }
                            //下に移動させる
                            else if (Input.GetKey(KeyCode.DownArrow))
                            {
                                //animController.PlayAnimation(selectAnimationClip);

                                //一番上から場外へ移動する時
                                if (menuTextLineCount <= SelectLine + 1) { } //SelectLine = 0;
                                else
                                {
                                    animController.PlayAnimation(selectAnimationClip);
                                    SelectLine++;
                                }
                                waitOnce = true;
                            }
                        }

                        //決定
                        if (Input.GetKey(KeyCode.Space))
                        {
                            SimpleQuickMenuLine simpleQuickMenuLine = CurrentMenuHierarchy.GetComponent<SimpleQuickMenuLine>();

                            //上の項目に戻る場合、キャンセルと同じ処理を行う
                            if (simpleQuickMenuLine && simpleQuickMenuLine.back)
                            {
                                animController.PlayAnimation(cancelAnimationClip);
                                state = State.Cancel;
                            }
                            //決定の処理を行う
                            else
                            {
                                animController.PlayAnimation(decisionAnimationClip);
                                state = State.Decision;
                            }
                        }

                        //キャンセル
                        if (Input.GetKey(KeyCode.Q) && !notUseCancelKey)
                        {
                            animController.PlayAnimation(cancelAnimationClip);
                            state = State.Cancel;
                        }

                        //現在選択している行にSelecterが重なるようにmenuTextを滑らかに移動させる
                        //???lineSpaceHeightが2行目以降ではなく3行目からになってる
                        menuText.transform.localPosition = Vector3.up * Mathf.Lerp(menuText.transform.localPosition.y, firstSelectY + lineHeight * SelectLine + lineSpaceHeight * (SelectLine <= 0 ? 0 : SelectLine - 1), smooth * Time.unscaledDeltaTime)
                        + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;

                        //行の色を指定
                        colorController.SetLineVertexColor(2, new Color(1, 1, 1, 0.2f), new Color(1, 1, 1, 0.7f), smooth * Time.unscaledDeltaTime);
                        colorController.SetLineVertexColor(1, new Color(1, 1, 1, 0.8f), new Color(1, 1, 1, 0.9f), smooth * Time.unscaledDeltaTime);
                        colorController.SetLineVertexColor(0, Color.black, Color.black, smooth * Time.unscaledDeltaTime);
                        colorController.SetLineVertexColor(-1, new Color(1, 1, 1, 0.9f), new Color(1, 1, 1, 0.8f), smooth * Time.unscaledDeltaTime);
                        colorController.SetLineVertexColor(-2, new Color(1, 1, 1, 0.7f), new Color(1, 1, 1, 0.2f), smooth * Time.unscaledDeltaTime);
                        colorController.SetLineVertexColor(-3, Color.clear, Color.clear, smooth * Time.unscaledDeltaTime);

                        //canvasGroupとtitleTextをフェードイン
                        canvasGroup.alpha = canvasGroup.alpha >= 0.95f ? 1 : Mathf.Lerp(canvasGroup.alpha, 1, 6 * Time.unscaledDeltaTime);
                        titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, titleText.color.a >= 0.95f ? 1 : Mathf.Lerp(titleText.color.a, 1, smooth * Time.unscaledDeltaTime));

                    }
                    //決定またはキャンセルしたかつ、Animationの再生が完了した時
                    if (state == State.Decision && animController.IsFinished(decisionAnimationClip) || state == State.Cancel && animController.IsFinished(cancelAnimationClip))
                    {
                        //決定したかつ、次に進む項目がない時はフェードを行わない
                        if (state == State.Decision && CurrentMenuHierarchy.childCount == 0)
                        {
                            //登録された処理を実行
                            CurrentMenuHierarchy.GetComponent<SimpleQuickMenuLine>()?.unityEvent.Invoke();
                            InvokeMenuCallBack?.Invoke(CurrentMenuHierarchy);

                            animController.PlayAnimation(selectAnimationClip);
                            state = State.Select;
                        }
                        else
                        {
                            //menuTextとtitleTextをフェードアウト
                            for (int index = 2; index >= -3; index--) colorController.SetLineVertexColor(index, Color.clear, Color.clear, smooth * Time.unscaledDeltaTime);
                            titleText.color = new Color(titleText.color.r, titleText.color.g, titleText.color.b, titleText.color.a <= 0.05f ? 0 : Mathf.Lerp(titleText.color.a, 0, smooth * Time.unscaledDeltaTime));

                            //フェードアウトが終わった時
                            if (titleText.color.a == 0)
                            {
                                //キャンセルの処理
                                if (state == State.Cancel)
                                {
                                    //後ろに戻る項目がない時メニューを消す
                                    if (CurrentMenuHierarchy.parent == menuHierarchy)
                                    {
                                        SelectLine = 0;

                                        menuText.transform.localPosition = Vector3.up * firstSelectY + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;

                                        animController.PlayAnimation(null);
                                        state = State.Select;
                                        Time.timeScale = 1;
                                        View = false;
                                    }
                                    //後ろの項目に戻る
                                    else
                                    {
                                        CurrentMenuHierarchy = CurrentMenuHierarchy.parent;
                                        SelectLine = 0;

                                        menuText.transform.localPosition = Vector3.up * firstSelectY + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;

                                        animController.PlayAnimation(selectAnimationClip);
                                        state = State.Select;
                                    }
                                }
                                //決定の処理
                                else if (state == State.Decision)
                                {
                                    //登録された処理を実行
                                    CurrentMenuHierarchy.GetComponent<SimpleQuickMenuLine>()?.unityEvent.Invoke();
                                    InvokeMenuCallBack?.Invoke(CurrentMenuHierarchy);

                                    //前の項目に進める
                                    CurrentMenuHierarchy = CurrentMenuHierarchy.GetChild(0);
                                    SelectLine = 0;

                                    menuText.transform.localPosition = Vector3.up * firstSelectY + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;

                                    animController.PlayAnimation(selectAnimationClip);
                                    state = State.Select;
                                }
                            }
                        }
                    }
                }
                else
                {
                    canvasGroup.blocksRaycasts = false;

                    //canvasGroupをフェードアウト
                    canvasGroup.alpha = canvasGroup.alpha <= 0.05f ? 0 : Mathf.Lerp(canvasGroup.alpha, 0, 6 * Time.unscaledDeltaTime);
                    
                    //(GetKeyUpにしているのは1F判定じゃないとメニュー上で勝手にキャンセルされてしまうから
                    //GetKeyDownにしないのは、メニュー上のキャンセルの判定がGetKeyであるから
                    //メニュー上のキャンセルの判定をGetKeyDownにしないのは、Time.timeScaleが0の時はUpとDownが判定されないから
                    //この問題は新しいImputSystemを使う事で解決するが、あまり流行っていないのでこちらにした)
                    if (Input.GetKeyUp(KeyCode.Q) && !notUseCancelKey)
                    {
                        //メニューを表示させる
                        ViewMenu();
                    }
                }

                if (GetMenuText(CurrentMenuHierarchy.parent) != menuTextChecker)
                {
                    UpdateText();
                    menuTextChecker = menuText.text;
                }

                colorController.ApplyTMPColor(SelectLine);
            }

#if UNITY_EDITOR
            //Editor上でしか実行しない(Animationの確認用)
            else
            {
                if (menuText && selecter && menuHierarchy)
                {
                    titleText.text = menuHierarchy.name;
                    menuText.text = GetMenuText(menuHierarchy);
                    menuTextLineMatch = new Regex("(?<v>.+)\n?").Matches(menuText.text);
                    menuTextLineCount = menuTextLineMatch.Count;

                    selectPreview = Mathf.Clamp(selectPreview, 0, menuTextLineCount - 1);

                    menuText.transform.localPosition = Vector3.up * (firstSelectY + lineHeight * selectPreview + lineSpaceHeight * (selectPreview <= 0 ? 0 : selectPreview - 1))
                        + Vector3.right * menuText.transform.localPosition.x + Vector3.forward * menuText.transform.localPosition.z;

                    //プレビューする行の文字列の横の大きさを計算
                    float textScale = GetTextLength(menuTextLineMatch[selectPreview].Groups["v"].Value, menuText.font);

                    //AnimationWindowがプレビュー状態の時はAnimationPropertyの値を適応してSelecterのX軸の大きさを計算する
                    if (AnimationMode.InAnimationMode())
                        selecter.transform.localScale = new Vector3((targetScaleX - textScale) * animationSelecterScaleX + textScale + addSelecterScaleX, selecter.transform.localScale.y, selecter.transform.localScale.z);
                    else selecter.transform.localScale = new Vector3(textScale, selecter.transform.localScale.y, selecter.transform.localScale.z);
                }
            }
#endif
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            //デバッグ情報を表示する
            if (viewInfo)
            {
                GUILayout.Box($"State {state}", GUILayout.ExpandWidth(false));
                GUILayout.Box($"View {View}", GUILayout.ExpandWidth(false));
            }
        }
#endif

        public void ViewMenu()
        {
            Time.timeScale = 0;
            View = true;
            animController.PlayAnimation(selectAnimationClip);
        }

        /// <summary>
        /// menuHierarchyの変更をテキストに適応する
        /// </summary>
        void UpdateText()
        {
            titleText.text = CurrentMenuHierarchy.parent.name;
            menuText.text = GetMenuText(CurrentMenuHierarchy.parent);
            menuTextLineMatch = new Regex("(?<v>.+)\n?").Matches(menuText.text);
            menuTextLineCount = menuTextLineMatch.Count;
            colorController.UpdateText(menuTextLineMatch);
            SelectLine = Mathf.Clamp(SelectLine, 0, menuTextLineCount);
        }

        /// <summary>
        /// GameObjectの階層から現在menuTextに表示すべきTextを取得する
        /// </summary>
        string GetMenuText(Transform parent)
        {
            string s = default;
            bool first = true;
            foreach (Transform child in parent)
            {
                s += first ? child.name : "\n" + child.name;
                first = false;
            }
            return s;
        }

        /// <summary>
        /// 文字列の横の長さを取得する
        /// </summary>
        //TODO:文字によって微妙にズレが出る問題
        float GetTextLength(string text, TMP_FontAsset font)
        {
            float length = 0;

            //1文字ごとに処理を行う
            for (int index = 0; index < text.Count(); index++)
            {
                //Unicodeを使ってFontAssetから文字を検索する
                byte[] data = System.Text.Encoding.Unicode.GetBytes(text[index].ToString());
                TMP_Character character = font.characterTable.FirstOrDefault(x => x.unicode == BitConverter.ToUInt16(data, 0));

                //検索した文字のhorizontalAdvanceを足し合わせる
                if (character != null) length += character.glyph.metrics.horizontalAdvance;
            }

            return length * multiplyTextLength;
        }

        //AnimationWindowでプレビューするAnimationを渡す(IAnimationClipSourceInterfaceの規約)
        public void GetAnimationClips(List<AnimationClip> results)
        {
            if (selectAnimationClip) results.Add(selectAnimationClip);
            if (decisionAnimationClip) results.Add(decisionAnimationClip);
            if (cancelAnimationClip) results.Add(cancelAnimationClip);
        }

        void OnDestroy()
        {
            //使い終わったら解放
           　if(animController != null) animController.Destroy();
        }

        /// <summary>
        /// 現在のステート
        /// </summary>
        enum State
        {
            /// <summary>
            /// 選んでいる時
            /// </summary>
            Select,
            /// <summary>
            /// 決定して次のメニューに移行している時
            /// </summary>
            Decision,
            /// <summary>
            /// キャンセルして前のメニューに戻る時
            /// </summary>
            Cancel
        }
    }
}