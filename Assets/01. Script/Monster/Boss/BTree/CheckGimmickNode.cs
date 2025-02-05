// 기믹 체크 노드
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
            Debug.LogWarning("[CheckGimmickNode] 실패: 보스 객체가 존재하지 않음.");
            return NodeStatus.Failure;
        }

        GimmickData currentGimmick = boss.CurrentPhaseGimmickData;

        if (currentGimmick == null)
        {
            Debug.Log("[CheckGimmickNode] 실패: 현재 페이즈에서 기믹이 존재하지 않음.");
            return NodeStatus.Failure;
        }

        Debug.Log($"[CheckGimmickNode] 현재 기믹: {currentGimmick.gimmickName}, 활성화 여부: {currentGimmick.isEnabled}");

        if (!currentGimmick.isEnabled)
        {
            Debug.Log($"[CheckGimmickNode] {currentGimmick.gimmickName}은 비활성화 상태.");
            return NodeStatus.Failure;
        }

        float healthRatio = (float)boss.CurrentHealth / boss.MaxHealth;
        Debug.Log($"[CheckGimmickNode] 현재 체력 비율: {healthRatio:F2}, 트리거 체력 비율: {currentGimmick.triggerHealthThreshold:F2}");

        if (healthRatio <= currentGimmick.triggerHealthThreshold)
        {
            Debug.Log($"[CheckGimmickNode] 성공! {currentGimmick.gimmickName} 기믹이 활성화됨.");
            return NodeStatus.Success;
        }

        Debug.Log("[CheckGimmickNode] 실패: 체력 조건을 충족하는 기믹이 없음.");
        return NodeStatus.Failure;
    }
}