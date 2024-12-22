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

        // ���� ������ ������� üũ
        if (!attackStrategy.CanAttack(distanceToPlayer, monsterClass))
        {   
            attackStrategy.StopAttack();
            Debug.Log("���� ���� ���");
            owner.ChangeState(MonsterStateType.Move);
            return;
        }

        // ���� ����
        Debug.Log("���� ����");
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