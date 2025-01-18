using static IMonsterState;
using UnityEngine;

public class GimmickState : MonsterBaseState
{
    private readonly IGimmickStrategy gimmickStrategy;
    private Animator animator;

    public GimmickState(CreatureAI owner, IGimmickStrategy strategy) : base(owner)
    {
        gimmickStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        animator.SetTrigger(gimmickStrategy.GetGimmickAnimationTrigger());
        gimmickStrategy.StartGimmick();
    }

    public override void Execute()
    {
        gimmickStrategy.UpdateGimmick();
        if (gimmickStrategy.IsGimmickComplete)
        {
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition() => gimmickStrategy.IsGimmickComplete;
}