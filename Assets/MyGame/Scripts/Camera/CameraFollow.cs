using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;
    [Range(1,10)] public float smoothness;

    public Vector2 maxPos, minPos;

    private void FixedUpdate()
    {
        Follow();
    }

    void Follow()
    {
        Vector3 targetPos = target.position + offset;

        Vector3 bounderies = new Vector3(
            Mathf.Clamp(targetPos.x, minPos.x, maxPos.x),
            Mathf.Clamp(targetPos.y, minPos.y, maxPos.y),
            Mathf.Clamp(targetPos.z, transform.position.z, transform.position.z));

        Vector3 smoothen = Vector3.Lerp(transform.position, bounderies, smoothness*Time.fixedDeltaTime);
        transform.position = smoothen;
    }
}
