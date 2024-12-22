using UnityEngine;

public class BasicIdleStrategy : IIdleStrategy
{
    private float detectionCheckInterval = 0.5f;  // ���� üũ �ֱ�
    private float nextDetectionCheck;

    public void OnIdle(Transform transform, MonsterClass monsterData)
    {
        nextDetectionCheck = Time.time + detectionCheckInterval;
    }

    public void UpdateIdle()
    {
        // ��� ���� ������Ʈ
        // ���߿� Idle �ִϸ��̼��̳� ��� ��� �߰� ����
    }

    public bool ShouldChangeState(float distanceToPlayer, MonsterClass monsterData)
    {
        if (Time.time >= nextDetectionCheck)
        {
            nextDetectionCheck = Time.time + detectionCheckInterval;

            // �÷��̾ ���� ���� �ȿ� ������ ���� ��ȯ
            if (distanceToPlayer <= monsterData.CurrentChaseRange)
            {
                return true;
            }
        }
        return false;
    }
}