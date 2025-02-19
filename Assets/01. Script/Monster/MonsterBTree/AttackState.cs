using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    private bool attackStarted = false;
    private string currentAnimTrigger;  // ���� ��� ���� �ִϸ��̼� Ʈ���� ����
    private bool isTransitioning = false;
    public AttackState(CreatureAI owner, IAttackStrategy strategy) : base(owner)
    {
        attackStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public override void Enter()
    {
        if (!attackStarted && !isTransitioning)
        {
            attackStrategy.StartAttack();
            attackStarted = true;
            attackStrategy.Attack(transform, player, monsterClass);
            currentAnimTrigger = attackStrategy.GetAnimationTriggerName();
            animator.SetTrigger(currentAnimTrigger);
        }
  
    }
    public override void Execute()
    {
        

        // ������ �Ϸ�Ǿ��ų� Ÿ�Ӿƿ��� ���
        if (!attackStrategy.IsAttacking)
        {
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }

        //// �ʿ��� ��� ���� �� Ÿ�� ���� ������Ʈ
        //if (player != null)
        //{
        //    Vector3 directionToPlayer = (player.position - transform.position).normalized;
        //    transform.rotation = Quaternion.Lerp(transform.rotation,
        //                                       Quaternion.LookRotation(directionToPlayer),
        //                                       Time.deltaTime * 5f);
        //}
    }


    public override void Exit()
    {
        Debug.Log("��������?");
        isTransitioning = true;
        attackStrategy.StopAttack();
        animator.ResetTrigger(currentAnimTrigger);
        attackStarted = false;
        isTransitioning = false;
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking && !isTransitioning;
    }
}