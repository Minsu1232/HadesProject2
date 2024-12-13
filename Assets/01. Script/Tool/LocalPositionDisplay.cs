using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class LocalPositionDisplay : MonoBehaviour
{
    [ShowInInspector] // �ν����Ϳ� ǥ��
    private Vector3 localPosition => transform.position; // �б� ���� �ʵ�

    [ShowInInspector, Range(0, 10)] // �����̴� ǥ��
    public float speed = 5f;

    [Button("Log Message")] // ��ư ����
    private void LogMessage()
    {
        Debug.Log("Odin Inspector �׽�Ʈ ��ư Ŭ����!");
    }
}
