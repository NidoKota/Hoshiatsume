using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SimpleQuickMenu;

//ステージを動かすこと全般
public class StageMove : MonoBehaviour
{
    public AnimationCurve angleCurve;
    public float yUp;

    public bool selectAngle { get; private set; }

    [NonSerialized]
    public int nextAngle;

    SimpleQuickMenuController menu;
    GameController gameController;
    Quaternion rot = Quaternion.identity;
    Vector2Int input;
    Vector2 inputBuffer;
    float angleSmoothingTime;
    float yu;
    float yInit;

    void Start()
    {
        menu = FindObjectOfType<SimpleQuickMenuController>();
        gameController = FindObjectOfType<GameController>();
        yInit = transform.position.y;
    }

    void Update()
    {
        input = new Vector2Int(Input.GetKeyDown(KeyCode.RightArrow) ? 1 : Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0, Input.GetKeyDown(KeyCode.UpArrow) ? 1 : Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0);

        if (!gameController.IsGameOver && !gameController.IsGameClear && gameController.firstStartTimelineFinished && !menu.View)
        {
            if (selectAngle)
            {
                angleSmoothingTime += (inputBuffer.x != 0 ? 1 : 0.5f) * Time.deltaTime;
                transform.rotation = Quaternion.Lerp(transform.rotation, rot, angleCurve.Evaluate(angleSmoothingTime));
                transform.position = new Vector2(transform.position.x, Mathf.Lerp(transform.position.y, yu, angleCurve.Evaluate(angleSmoothingTime)));

                if (angleSmoothingTime >= angleCurve.keys[angleCurve.length - 1].time)
                {
                    transform.rotation = rot;
                    angleSmoothingTime = 0;
                    selectAngle = false;
                }
            }
            else
            {
                if (input != Vector2.zero)
                {
                    inputBuffer = input.x != 0 ? new Vector2(input.x, 0) : new Vector2(0, input.y);

                    if (input.x == 1)
                        rot *= Quaternion.AngleAxis(90, Vector3.forward);
                    else if (input.x == -1)
                        rot *= Quaternion.AngleAxis(-90, Vector3.forward);
                    else if (input.y == 1)
                        rot *= Quaternion.AngleAxis(180, Vector3.forward);
                    else if (input.y == -1)
                        rot *= Quaternion.AngleAxis(-180, Vector3.forward);

                    if (rot.eulerAngles.z > -5 && rot.eulerAngles.z < 5)
                    {
                        nextAngle = 0;
                        yu = yInit;
                    }
                    else if (rot.eulerAngles.z > 85 && rot.eulerAngles.z < 95)
                    {
                        nextAngle = 90;
                        yu = (yUp + yInit) / 2;
                    }
                    else if (rot.eulerAngles.z > 175 && rot.eulerAngles.z < 185)
                    {
                        nextAngle = 180;
                        yu = yu = yUp;
                    }
                    else
                    {
                        nextAngle = 270;
                        yu = (yUp + yInit) / 2;
                    }

                    selectAngle = true;
                }
            }
        }
    }
}
