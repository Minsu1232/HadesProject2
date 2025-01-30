// 페이즈 전환 체크 노드
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


        // 다음 페이즈가 있고, 체력이 threshold 이하면
        if (boss.CurrentPhase < boss.GetruntimePhaseData().Count &&
            healthRatio <= boss.CurrentPhaseData.phaseTransitionThreshold)
        {
            return NodeStatus.Success;  // 페이즈 전환 필요
        }

        return NodeStatus.Failure;  // 페이즈 전환 불필요
    }
}