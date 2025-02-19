using static IMonsterState;
using UnityEngine;

public class AttackState : MonsterBaseState
{
    private readonly IAttackStrategy attackStrategy;
    private Animator animator;
    private bool attackStarted = false;
    private string currentAnimTrigger;  // 현재 사용 중인 애니메이션 트리거 저장
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
        

        // 공격이 완료되었거나 타임아웃된 경우
        if (!attackStrategy.IsAttacking)
        {
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }

        //// 필요한 경우 공격 중 타겟 방향 업데이트
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
        Debug.Log("오긴했으?");
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