using static IMonsterState;
using UnityEngine;

public class GroggyState : MonsterBaseState
{
    private readonly IGroggyStrategy groggyStrategy;
    Animator animator;

    public GroggyState(CreatureAI owner, IGroggyStrategy strategy) : base(owner)
    {
        groggyStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        groggyStrategy.OnGroggy(transform, monsterClass);
        animator.SetBool("IsGroggy", true);  // SetTrigger ��� SetBool ���
    }

    public override void Execute()
    {
        groggyStrategy.UpdateGroggy();
        if (groggyStrategy.IsGroggyComplete)
        {
            animator.SetBool("IsGroggy", false);  // �׷α� ���°� ������ bool ����
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition()
    {
        return groggyStrategy.IsGroggyComplete;
    }
}