using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // 따라갈 플레이어
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10); // 기본 오프셋

    //private void OnEnable()
    //{
    //    if(playerTransform == null)
    //    {
    //        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
    //    }
    //}
    void LateUpdate()
    {
        if (playerTransform == null) return;

        // 플레이어 위치 + 기본 오프셋 + 흔들림 효과
        Vector3 shakeOffset = CameraShakeManager.GetShakeOffset();
        transform.position = playerTransform.position + offset + shakeOffset;

        // 항상 플레이어를 바라보도록 설정
        transform.LookAt(playerTransform);
    }
    // 흔들림 효과 트리거

}
