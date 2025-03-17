using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // ���� �÷��̾�
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -10); // �⺻ ������

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

        // �÷��̾� ��ġ + �⺻ ������ + ��鸲 ȿ��
        Vector3 shakeOffset = CameraShakeManager.GetShakeOffset();
        transform.position = playerTransform.position + offset + shakeOffset;

        // �׻� �÷��̾ �ٶ󺸵��� ����
        transform.LookAt(playerTransform);
    }
    // ��鸲 ȿ�� Ʈ����

}
