using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 따라갈 대상
    public Vector3 offset;        // 카메라와 타겟 사이 거리
    public float smoothSpeed = 0.125f;  // 부드럽게 따라가기 위한 보간 속도

    void LateUpdate()
    {
        if (target == null) return;

        // 따라갈 위치: X는 고정, Y는 플레이어 위치
        Vector3 desiredPosition = new Vector3(transform.position.x, target.position.y, transform.position.z) + offset;

        // 부드럽게 따라가기
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}
