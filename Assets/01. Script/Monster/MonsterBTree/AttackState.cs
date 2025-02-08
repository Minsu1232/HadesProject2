using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    private bool isFirstEnter = true;

    public AttackState(CreatureAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
       
        //���� ���� �� ���ο� ���� ����(BossMultiAttackStrategy�� ��쿡��)
        //if (isFirstEnter && attackStrategy is BossMultiAttackStrategy bossStrategy)
        //{
        //    bossStrategy.ChangePattern();
        //}
        //isFirstEnter = false;

    
        // ���� ����
        attackStrategy.Attack(transform, player, monsterClass);

        animator.SetTrigger(attackStrategy.GetAnimationTriggerName());
        Debug.Log(attackStrategy.GetAnimationTriggerName());
        attackStrategy.StartAttack();
    }

    public override void Execute()
    {
        //// ���� ������ ����(��, ������ ������ ��ȯ ������ ����)�Ǿ����� Ȯ��
        //if (CanTransition())
        //{
        //    // ������ ���� ���, �ִϸ����� Ʈ���Ÿ� �����ϰ� ���� ���·� ��ȯ
        //    animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());

        //    // �ʿ信 ���� ���� �Ŀ� �߰� ������ ���� �� �ֽ��ϴ�.
        //    // ��: ��ٿ�, �÷��̾���� �Ÿ� üũ ��

        //    owner.ChangeState(MonsterStateType.Move); // Move ���·� ��ȯ (��Ȳ�� �°� ����)
        //}


    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
        isFirstEnter = true;  // ���� ������ ���� ����
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}