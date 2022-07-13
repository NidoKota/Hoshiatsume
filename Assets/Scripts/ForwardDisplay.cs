using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//常にカメラの前に移動させる
#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class ForwardDisplay : MonoBehaviour
{
    public Transform cam;
    public Vector3 position;
    public bool useZDistance;
    public bool updatePosition;
    Vector3 centerPosition;

    void Start()
    {
        if (useZDistance) position.z = Vector3.Distance(transform.position, cam.position);
        transform.position = cam.right * position.x + cam.up * position.y + cam.forward * position.z + cam.position;
    }

    void LateUpdate()
    {
        if (cam)
        {
            if (updatePosition) transform.position = cam.right * position.x + cam.up * position.y + cam.forward * position.z + cam.position;
            centerPosition = cam.forward * position.z + cam.position;
            if (cam.position - transform.position != Vector3.zero) transform.rotation = Quaternion.LookRotation(centerPosition - cam.position, cam.up);
        }
    }
}
