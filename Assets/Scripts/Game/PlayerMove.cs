using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using SimpleQuickMenu;

public class PlayerMove : MonoBehaviour
{
    [Header("Move Settings")]
    public float gravity;
    public float maxDownSpeed;
    public float angleSmooth;
    public AnimationCurve flashSettingCurve;  //ダメージを受けた時の点滅の仕方
    public GameObject starEffect;

    [Header("GameOver Settings")]
    public float goAddAngleSpeed;
    public float goIniDownSpeed;
    public float goAddDownSpeed;

    GameObject sprite;
    GameController gameController;
    SimpleQuickMenuController menu;
    Rigidbody2D rb;
    StageMove stageMove;
    CapsuleCollider2D coll;
    float flashTime;
    int flashNumber;
    bool effectGravity;
    bool canDamage = true;

    RigidbodyStateType _rigidbodyStateType = (RigidbodyStateType)(-1);

    void Start()
    {
        gameController = FindObjectOfType<GameController>();
        stageMove = FindObjectOfType<StageMove>();
        menu = FindObjectOfType<SimpleQuickMenuController>();
        sprite = transform.Find("Sprite").gameObject;
        coll = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        gameController.GameOverEvent += (gc) =>
        {
            rigidbodyStateType = RigidbodyStateType.RotateMove;
            rb.velocity = new Vector2(0, -goIniDownSpeed);
            rb.angularVelocity = 0;
            coll.isTrigger = true;
        };
    }

    void Update()
    {
        if (!gameController.IsGameOver && !gameController.IsGameClear)
        {
            transform.up = Vector3.Lerp(transform.up, Vector3.up, angleSmooth * Time.deltaTime);

            if (stageMove.selectAngle)
            {
                rigidbodyStateType = RigidbodyStateType.NoGravity;
                rb.velocity = Vector2.zero;
            }
            else
            {
                rigidbodyStateType = RigidbodyStateType.Move;
            }

            //ダメージを与えられない時にダメージ処理を行う
            if (!canDamage && !menu.View)
            {
                //点滅させる
                flashTime += Time.deltaTime;
                flashNumber++;
                if (flashNumber >= (int)flashSettingCurve.Evaluate(flashTime))
                {
                    sprite.SetActive(!sprite.activeSelf);
                    flashNumber = flashNumber - (int)flashSettingCurve.Evaluate(flashTime);
                }
                //点滅終了
                if (flashTime >= flashSettingCurve.keys[flashSettingCurve.length - 1].time)
                {
                    sprite.SetActive(true);
                    flashTime = 0;
                    canDamage = true;
                }
            }

        }
        else sprite.SetActive(true);

        if (menu.View) sprite.SetActive(true);

        if (gameController.IsGameClear) rigidbodyStateType = RigidbodyStateType.FreezeAll;
    }

    void FixedUpdate()
    {
        if (effectGravity) rb.AddForce(new Vector2(0, gravity));

        if (gameController.IsGameOver)
        {
            rb.AddTorque(goAddAngleSpeed);
            rb.AddForce(Vector2.down * goAddDownSpeed);
        }
        else rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -maxDownSpeed, Mathf.Infinity));
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !gameController.IsGameOver && !gameController.IsGameClear)
        {
            if (canDamage)
            {
                canDamage = false;
                gameController.Damege(1);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Star") && !gameController.IsGameOver && !gameController.IsGameClear)
        {
            Destroy(Instantiate(starEffect, collision.transform.position, starEffect.transform.rotation, GameObject.Find("Stage/Effects").transform), 1.5f);
            gameController.AddStar(1);
            Destroy(collision.gameObject);
        }
    }

    /// <summary>
    /// 物理演算のステート
    /// </summary>
    enum RigidbodyStateType
    {
        /// <summary>
        /// 無重力回転不可完全停止
        /// </summary>
        FreezeAll,

        /// <summary>
        /// 無重力回転不可移動可
        /// </summary>
        NoGravity,

        /// <summary>
        /// 重力あり回転不可移動可
        /// </summary>
        Move,

        /// <summary>
        /// 重力あり回転可移動可
        /// </summary>
        RotateMove
    }

    /// <summary>
    /// 物理演算のステート
    /// </summary>
    RigidbodyStateType rigidbodyStateType
    {
        get { return _rigidbodyStateType; }
        set
        {
            if (value != _rigidbodyStateType)
            {
                switch (value)
                {
                    case RigidbodyStateType.FreezeAll:
                        rb.constraints = RigidbodyConstraints2D.FreezeAll;
                        effectGravity = false;
                        break;
                    case RigidbodyStateType.NoGravity:
                        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                        effectGravity = false;
                        break;
                    case RigidbodyStateType.Move:
                        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                        effectGravity = true;
                        break;
                    case RigidbodyStateType.RotateMove:
                        rb.constraints = RigidbodyConstraints2D.None;
                        effectGravity = true;
                        break;
                }
                _rigidbodyStateType = value;
            }
        }
    }
}