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
            Debug.Log("���� �ִϸ��̼� ����");
        }
        
       

    }

    public override bool CanTransition()
    {
        return false;  // ��� ���¿����� �ٸ� ���·� ��ȯ �Ұ�
    }
}
