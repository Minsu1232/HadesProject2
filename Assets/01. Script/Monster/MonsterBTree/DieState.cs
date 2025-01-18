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
            // ��� ���°� �Ϸ�Ǹ� ������Ʈ ����
            GameObject.Destroy(owner.gameObject);
        }
    }

    public override bool CanTransition()
    {
        return false;  // ��� ���¿����� �ٸ� ���·� ��ȯ �Ұ�
    }
}
