using UnityEngine;
using static AttackData;
using static IMonsterState;

public class HitState : MonsterBaseState
{
    private readonly IHitStrategy hitStrategy;
    private int damageAmount;
    Animator animator;

    public HitState(CreatureAI owner, IHitStrategy strategy) : base(owner)
    {
        hitStrategy = strategy;
        animator = owner.GetComponent<Animator>();
    }

    public void SetDamage(int damage)
    {
        this.damageAmount = damage;
    }

    public override void Enter()
    {
        // 아머가 있는지 체크
        if (monsterClass.CurrentArmor > 0)
        {
            // 아머가 있으면 애니메이션 재생하지 않고 바로 이전 상태로
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }
        // 현재 공격 타입 확인


        //공격 타입에 따라 카메라 히트 스톱을 다르게 처리
      
            //HitStopManager.TriggerHitStop(
            //    monsterClass.CurrentCameraShakeDuration,
            //    monsterClass.CurrentCameraShakeIntensity
            //);
        
       
        // 아머가 없으면 피격 처리 및 애니메이션 재생
        hitStrategy.OnHit(transform, monsterClass, damageAmount);
        animator.SetTrigger("GetHit");
    }

    public override void Execute()
    {
        hitStrategy.UpdateHit();
        if (hitStrategy.IsHitComplete)
        {
            // 피격 상태가 끝나면 Move 상태로 전환
            owner.ChangeState(MonsterStateType.Move);
            animator.ResetTrigger("GetHit");
        }
    }

    public override bool CanTransition()
    {
        // // Hit 상태에서 다른 상태로 전환될 때만 IsHitComplete 체크
        //if (owner.GetCurrentState() is HitState)
        //{
        //    return hitStrategy.IsHitComplete;
        //}
        
        // 다른 상태에서 Hit 상태로 전환될 때는 항상 가능
        return true;
    }
}