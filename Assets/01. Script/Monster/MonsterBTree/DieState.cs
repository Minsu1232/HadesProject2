using UnityEngine;

public class DieState : MonsterBaseState
{
    private readonly IDieStrategy dieStrategy;

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

        if (dieStrategy.IsDeathComplete)
        {
            // 사망 상태가 완료되면 오브젝트 제거
            GameObject.Destroy(owner.gameObject);
        }
    }

    public override bool CanTransition()
    {
        return false;  // 사망 상태에서는 다른 상태로 전환 불가
    }
}
