using static IMonsterState;
using UnityEngine;
using static BossUIManager;

public class GroggyState : MonsterBaseState
{
    private readonly IGroggyStrategy groggyStrategy;
    BossUIManager bossUIManager_;
    Animator animator;

    public GroggyState(CreatureAI owner, IGroggyStrategy strategy) : base(owner)
    {
        groggyStrategy = strategy;
        animator = owner.GetComponent<Animator>();
        
        
    }
    public GroggyState(CreatureAI owner, IGroggyStrategy strategy,BossUIManager bossUIManager) : base(owner)
    {
        bossUIManager_ = bossUIManager;
        groggyStrategy = strategy;
        animator = owner.GetComponent<Animator>();

    }

    public override void Enter()
    {
        groggyStrategy.OnGroggy(transform, monsterClass);
        
        animator.SetBool("IsGroggy", true);  // SetTrigger 대신 SetBool 사용
        Debug.Log("그로가");
        bossUIManager_?.AddState(StateType.Groggy, groggyStrategy.GroggyDuration);
    }

    public override void Execute()
    {
        groggyStrategy.UpdateGroggy();
        if (groggyStrategy.IsGroggyComplete)
        {
            animator.SetBool("IsGroggy", false);  // 그로기 상태가 끝나면 bool 해제
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition()
    {
        return groggyStrategy.IsGroggyComplete;
    }
}