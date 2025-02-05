using UnityEngine;
using static IMonsterState;

public class ExecutePhaseTransitionNode : BTNode
{
    private BossMonster boss;
    private IPhaseTransitionStrategy transitionStrategy;

    public ExecutePhaseTransitionNode(CreatureAI owner) : base(owner)
    {
        boss = owner.GetStatus().GetMonsterClass() as BossMonster;
        transitionStrategy = owner.GetPhaseTransitionStrategy();
    }

    public override NodeStatus Execute()
    {
        Debug.Log($"ExecutePhase: IsInPhaseTransition = {boss.IsInPhaseTransition}");
        if (boss == null)
            return NodeStatus.Failure;
        if (!boss.IsInPhaseTransition)
        {
            Debug.Log($"ExecutePhase: 상태 전환 시도, 현재 페이즈 {boss.CurrentPhase}");
            owner.ChangeState(MonsterStateType.PhaseTransition);
        }
        return NodeStatus.Success;
    }
}