using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

//点数の計算時間の処理など
public class GameController : MonoBehaviour
{
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI starText;
    public Animator[] harts;
    public int starTargetCount;
    public float limitTime;
    
    public bool IsGameOver { get; private set; }
    public bool IsGameClear { get; private set; }
    public int StarCount { get; private set; }
    public int LifeCount { get; private set; } = 3;
    public bool firstStartTimelineFinished { get; private set; }
    public event Action<GameController> GameOverEvent;
    public event Action<GameController> GameClearEvent;

    CinemachineImpulseSource impulseSource;
    PlayableDirector firstTimeline;
    PlayableDirector startTimeline;
    PlayableDirector gameOverTimeline;
    PlayableDirector gameClearTimeline;
    PlayerMove playerMove;
    StageMove stageMove;
    Transform cam;
    TimeSpan ts;
    float liTimer;
    public static bool first = true;
    public static int lastPlayStageNum { get; private set; } = 1;

    void OnEnable()
    {
        playerMove = FindObjectOfType<PlayerMove>();
        stageMove = FindObjectOfType<StageMove>();
        firstTimeline = transform.Find("FirstTimeline").GetComponent<PlayableDirector>();
        startTimeline = transform.Find("StartTimeline").GetComponent<PlayableDirector>();
        gameOverTimeline = transform.Find("GameOverTimeline").GetComponent<PlayableDirector>();
        gameClearTimeline = transform.Find("GameClearTimeline").GetComponent<PlayableDirector>();
        cam = Camera.main.transform;
        impulseSource = cam.GetComponent<CinemachineImpulseSource>();

        liTimer = limitTime;
        lastPlayStageNum = int.Parse(SceneManager.GetActiveScene().name.Replace("Stage","")); 

        if (first)
        {
            firstTimeline.Play();
            first = false;
        }
        else startTimeline.Play();

        GameOverEvent += (gc) => gameOverTimeline.Play();

        GameClearEvent += (gc) =>
        {
            StageSelect.stageCleared |= 1 << lastPlayStageNum;

            gameClearTimeline.Play();
        };
    }

    void Update()
    {
        if (!IsGameOver && !IsGameClear)
        {
            ts = new TimeSpan(0, 0, 0, 0, (int)(liTimer * 1000));
            if (firstStartTimelineFinished)
            {
                if ((int)liTimer <= 10) timeText.color = Color.red;
                if (liTimer >= 0)
                {
                    liTimer -= Time.deltaTime;
                    timeText.text = $"{ts.ToString(@"mm\:ss"):F2}";
                }
                else
                {
                    timeText.text = "00:00";
                    int random = UnityEngine.Random.Range(0, 11);
                    if(random == 0 || random == 1 || random == 2 || random == 3) GameOverController.why = "時間がなくなった！";
                    else if (random == 4 || random == 5 || random == 6 || random == 7) GameOverController.why = "タイムアップだ！";
                    else if (random == 8) GameOverController.why = "おそすぎた！";
                    else if (random == 9) GameOverController.why = "ちこくした！";
                    else if (random == 10) GameOverController.why = "時は金なり！";

                    impulseSource.GenerateImpulseAt(cam.position, new Vector3(0, 1, 0));
                    DoGameOver();
                }
            }
            else timeText.text = $"{ts.ToString(@"mm\:ss"):F2}";

            LifeCount = Mathf.Clamp(LifeCount, 0, 3);

            if (LifeCount == 0)
            {
                int random = UnityEngine.Random.Range(0, 11);
                if (random == 0 || random == 1 || random == 2 || random == 3) GameOverController.why = "エイリアンにやられた！";
                else if (random == 4 || random == 5 || random == 6 || random == 7) GameOverController.why = "ちからつきた！";
                else if (random == 8) GameOverController.why = "ヤツにやられた！";
                else if (random == 9) GameOverController.why = "ダウンした！";
                else if (random == 10) GameOverController.why = "ハートがなくなった！";

                DoGameOver();
            }

            starText.text = $"{StarCount:0}/{starTargetCount}";

            if (StarCount >= starTargetCount)
            {
                GameClearEvent?.Invoke(this);
                IsGameClear = true;
            }
        }
    }

    void DoGameOver()
    {
        GameOverEvent?.Invoke(this);
        IsGameOver = true;
    }

    public void AddStar(int count)
    {
        StarCount += count;
    }

    public void Damege(int damege)
    {
        impulseSource.GenerateImpulseAt(cam.position, new Vector3(0, 1, 0));

        for (int i = LifeCount - 1; i >= LifeCount - damege; i--)
        {
            harts[i].GetComponent<Animator>().SetBool("HartBreak", true);
        }

        LifeCount -= damege;

        for (int i = harts.Length - 1; i >= LifeCount; i--)
        {
            harts[i].transform.Find("Hart0").GetComponent<Image>().enabled = true;
        }
    }

    public void GameOverTimelineFinished()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void GameClearTimelineFinished()
    {
        SceneManager.LoadScene("GameClear");
    }

    public void FirstStartTimelineFinished()
    {
        firstStartTimelineFinished = true;
    }
}
