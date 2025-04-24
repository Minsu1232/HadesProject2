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


        // 플레이어와의 거리 계산
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 최소 거리 설정 (이 값은 보스 크기에 따라 조정)
        float minDistance = 2.0f;

        // 너무 가까이 있으면 회전하지 않음
        if (distanceToPlayer < minDistance)
            return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            // Y축만 회전하도록 수정
            Vector3 flatDirection = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            if (flatDirection.magnitude > 0.01f) // 방향 벡터가 너무 작지 않은지 확인
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
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