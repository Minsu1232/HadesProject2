// ������ ��ȯ üũ ���
using System;
using System.Diagnostics;
using UnityEngine;

public class CheckPhaseTransitionNode : BTNode
{
    private BossMonster boss;

    public CheckPhaseTransitionNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;

    }

    public override NodeStatus Execute()
    {
        if (boss == null) return NodeStatus.Failure;

        float healthRatio = (float)boss.CurrentHealth / boss.MaxHealth;


        // ���� ����� �ְ�, ü���� threshold ���ϸ�
        if (boss.CurrentPhase < boss.GetruntimePhaseData().Count &&
            healthRatio <= boss.CurrentPhaseData.phaseTransitionThreshold)
        {
            return NodeStatus.Success;  // ������ ��ȯ �ʿ�
        }

        return NodeStatus.Failure;  // ������ ��ȯ ���ʿ�
    }
}