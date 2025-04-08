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
    // CameraFollow.cs�� �Ʒ� �޼���� �߰�
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }

    public Vector3 GetOffset()
    {
        return offset;
    }

    // ī�޶� Ÿ�� ���� �޼��� �߰�
    public void SetTemporaryTarget(Transform newTarget, Vector3 newOffset)
    {
        if (newTarget != null)
        {
            playerTransform = newTarget;
            offset = newOffset;
        }
    }

    // ���� Ÿ������ ����
    public void ResetToPlayer()
    {
        if (GameInitializer.Instance != null)
        {
            playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
            offset = new Vector3(0, 10, -10); // �⺻ ���������� ����
        }
    }
}
