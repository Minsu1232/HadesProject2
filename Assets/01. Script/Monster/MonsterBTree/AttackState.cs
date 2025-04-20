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

        // BossMultiAttackStrategy 이벤트 구독
        if (strategy is BossMultiAttackStrategy multiStrategy) // 보스몬스터 or 멀티전략컨테이너용
        {
            multiStrategy.OnStrategyStateChanged += () =>
            {
                string triggerName = attackStrategy.GetAnimationTriggerName();
                animator.SetTrigger(triggerName);
                Debug.Log("구독" + triggerName) ;
                // 차지 준비 상태 파라미터 설정             
            };
        }
        else if(strategy is ChargeAttackStrategy chargeStrategy) // 일반몬스터용
        {
            chargeStrategy.OnChargeStateChanged += () =>
            {

                string triggerName = attackStrategy.GetAnimationTriggerName();
                animator.SetTrigger(triggerName);
            };
        }
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

            Debug.Log(attackStrategy.ToString());
        }
  
    }
    public override void Execute()
    {
        // 디버그 로그 추가
       
        if (attackStrategy is ChargeAttackStrategy chargeStrategy) // 일반몬스터옹
        {
            chargeStrategy.UpdateCharge(transform);
        }
        // BossMultiAttackStrategy를 통해 업데이트
   else if (attackStrategy is BossMultiAttackStrategy multiStrategy) // 보스용
        {
            multiStrategy.UpdateStrategy(transform);
        }
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