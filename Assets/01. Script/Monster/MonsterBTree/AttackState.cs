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
       
        //상태 진입 시 새로운 패턴 선택(BossMultiAttackStrategy인 경우에만)
        //if (isFirstEnter && attackStrategy is BossMultiAttackStrategy bossStrategy)
        //{
        //    bossStrategy.ChangePattern();
        //}
        //isFirstEnter = false;

    
        // 공격 실행
        attackStrategy.Attack(transform, player, monsterClass);

        animator.SetTrigger(attackStrategy.GetAnimationTriggerName());
        Debug.Log(attackStrategy.GetAnimationTriggerName());
        attackStrategy.StartAttack();
    }

    public override void Execute()
    {
        //// 공격 전략이 종료(즉, 공격이 끝나고 전환 가능한 상태)되었는지 확인
        //if (CanTransition())
        //{
        //    // 공격이 끝난 경우, 애니메이터 트리거를 리셋하고 다음 상태로 전환
        //    animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());

        //    // 필요에 따라 공격 후에 추가 로직을 넣을 수 있습니다.
        //    // 예: 쿨다운, 플레이어와의 거리 체크 등

        //    owner.ChangeState(MonsterStateType.Move); // Move 상태로 전환 (상황에 맞게 수정)
        //}


    }

    public override void Exit()
    {
        attackStrategy.StopAttack();
        animator.ResetTrigger(attackStrategy.GetAnimationTriggerName());
        isFirstEnter = true;  // 다음 진입을 위해 리셋
    }

    public override bool CanTransition()
    {
        return !attackStrategy.IsAttacking;
    }
}