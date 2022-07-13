using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform enemyParent;
    
    PlayerMove playerMove;
    StageMove stageMove;

    void Start()
    {
        playerMove = FindObjectOfType<PlayerMove>();
        stageMove = FindObjectOfType<StageMove>();
    }
    
    void Update()
    {
        if(enemyParent) transform.position = new Vector2(enemyParent.position.x, enemyParent.position.y);
        if (!stageMove.selectAngle)
        {
            if (stageMove.nextAngle == 0)
            {
                if (transform.position.x < playerMove.transform.position.x) transform.localScale = new Vector2(-1, transform.localScale.y);
                else transform.localScale = new Vector2(1, transform.localScale.y);
            }
            else if (stageMove.nextAngle == 90)
            {
                if (transform.position.y < playerMove.transform.position.y) transform.localScale = new Vector2(-1, transform.localScale.y);
                else transform.localScale = new Vector2(1, transform.localScale.y);
            }
            else if (stageMove.nextAngle == 180)
            {
                if (transform.position.x < playerMove.transform.position.x) transform.localScale = new Vector2(1, transform.localScale.y);
                else transform.localScale = new Vector2(-1, transform.localScale.y);
            }
            else
            {
                if (transform.position.y < playerMove.transform.position.y) transform.localScale = new Vector2(1, transform.localScale.y);
                else transform.localScale = new Vector2(-1, transform.localScale.y);
            }
        }
    }
}
