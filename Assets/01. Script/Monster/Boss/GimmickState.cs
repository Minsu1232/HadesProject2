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
        animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(),true);        
        gimmickStrategy.StartGimmick();
    }

    public override void Execute()
    {
        gimmickStrategy.UpdateGimmick();
        if (gimmickStrategy.IsGimmickComplete)
        {
            animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(), false);
            owner.ChangeState(MonsterStateType.Groggy);
        }
    }

    public override bool CanTransition() => gimmickStrategy.IsGimmickComplete;
}