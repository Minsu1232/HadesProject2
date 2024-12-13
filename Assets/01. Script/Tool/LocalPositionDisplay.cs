using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class LocalPositionDisplay : MonoBehaviour
{
    [ShowInInspector] // 인스펙터에 표시
    private Vector3 localPosition => transform.position; // 읽기 전용 필드

    [ShowInInspector, Range(0, 10)] // 슬라이더 표시
    public float speed = 5f;

    [Button("Log Message")] // 버튼 생성
    private void LogMessage()
    {
        Debug.Log("Odin Inspector 테스트 버튼 클릭됨!");
    }
}
