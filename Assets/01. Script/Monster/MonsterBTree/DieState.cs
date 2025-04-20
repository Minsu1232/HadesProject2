using UnityEngine;

public class DieState : MonsterBaseState
{
    private readonly IDieStrategy dieStrategy;
    bool isDie = false;
    public DieState(CreatureAI owner, IDieStrategy strategy) : base(owner)
    {
        dieStrategy = strategy;
    }

    public override void Enter()
    {        
        dieStrategy.OnDie(transform, monsterClass);
    }

    public override void Execute()
    {
        dieStrategy.UpdateDeath();
        if (!isDie)
        {
            owner.animator.SetTrigger("Die");
            isDie = true;
            Debug.Log("죽음 애니메이션 시작");
        }
        
       

    }

    public override bool CanTransition()
    {
        return false;  // 사망 상태에서는 다른 상태로 전환 불가
    }
}
