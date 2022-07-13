using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarMove : MonoBehaviour
{
    public float angle;
    HingeJoint2D hinge;
    StageMove stageMove;
    Rigidbody2D rb;
    JointAngleLimits2D limitsBuffer;
    JointAngleLimits2D limits;
    Vector2 u;

    void Start()
    {
        hinge = GetComponent<HingeJoint2D>();
        stageMove = FindObjectOfType<StageMove>();
        rb = FindObjectOfType<Rigidbody2D>();
        limitsBuffer = hinge.limits;
    }

    void Update()
    {
        if (stageMove.selectAngle)
        {

            limits.max = stageMove.transform.rotation.eulerAngles.z + limitsBuffer.max;
            limits.min = stageMove.transform.rotation.eulerAngles.z + limitsBuffer.min;

            hinge.limits = limits;
        }
    }
}
