// ��� üũ ���
using UnityEngine;

public class CheckGimmickNode : BTNode
{
    private BossMonster boss;

    public CheckGimmickNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;
    }

    public override NodeStatus Execute()
    {
        if (boss == null)
        {
            Debug.LogWarning("[CheckGimmickNode] ����: ���� ��ü�� �������� ����.");
            return NodeStatus.Failure;
        }

        GimmickData currentGimmick = boss.CurrentPhaseGimmickData;

        if (currentGimmick == null)
        {
            Debug.Log("[CheckGimmickNode] ����: ���� ������� ����� �������� ����.");
            return NodeStatus.Failure;
        }

        Debug.Log($"[CheckGimmickNode] ���� ���: {currentGimmick.gimmickName}, Ȱ��ȭ ����: {currentGimmick.isEnabled}");

        if (!currentGimmick.isEnabled)
        {
            Debug.Log($"[CheckGimmickNode] {currentGimmick.gimmickName}�� ��Ȱ��ȭ ����.");
            return NodeStatus.Failure;
        }

        float healthRatio = (float)boss.CurrentHealth / boss.MaxHealth;
        Debug.Log($"[CheckGimmickNode] ���� ü�� ����: {healthRatio:F2}, Ʈ���� ü�� ����: {currentGimmick.triggerHealthThreshold:F2}");

        if (healthRatio <= currentGimmick.triggerHealthThreshold)
        {
            Debug.Log($"[CheckGimmickNode] ����! {currentGimmick.gimmickName} ����� Ȱ��ȭ��.");
            return NodeStatus.Success;
        }

        Debug.Log("[CheckGimmickNode] ����: ü�� ������ �����ϴ� ����� ����.");
        return NodeStatus.Failure;
    }
}