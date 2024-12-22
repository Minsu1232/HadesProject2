using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;

    public AttackState(MonsterAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
    }

    public override void Enter()
    {
        attackStrategy.StartAttack();
    }

    public override void Execute()
    {
        float distanceToPlayer = GetDistanceToPlayer();

        // 공격 범위를 벗어났는지 체크
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {   
            attackStrategy.StopAttack();
            Debug.Log("공격 범위 벗어남");
            owner.ChangeState(MonsterStateType.Move);
            return;
        }

        // 공격 실행
        Debug.Log("공격 실행");
        attackStrategy.Attack(transform, player, monsterClass);

    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}