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
        // �ƸӰ� �ִ��� üũ
        if (monsterClass.CurrentArmor > 0)
        {
            // �ƸӰ� ������ �ִϸ��̼� ������� �ʰ� �ٷ� ���� ���·�
            owner.ChangeState(MonsterStateType.Idle);
            return;
        }
        // ���� ���� Ÿ�� Ȯ��


        //���� Ÿ�Կ� ���� ī�޶� ��Ʈ ������ �ٸ��� ó��
      
            //HitStopManager.TriggerHitStop(
            //    monsterClass.CurrentCameraShakeDuration,
            //    monsterClass.CurrentCameraShakeIntensity
            //);
        
       
        // �ƸӰ� ������ �ǰ� ó�� �� �ִϸ��̼� ���
        hitStrategy.OnHit(transform, monsterClass, damageAmount);
        animator.SetTrigger("GetHit");
    }

    public override void Execute()
    {
        hitStrategy.UpdateHit();
        if (hitStrategy.IsHitComplete)
        {
            // �ǰ� ���°� ������ Move ���·� ��ȯ
            owner.ChangeState(MonsterStateType.Move);
            animator.ResetTrigger("GetHit");
        }
    }

    public override bool CanTransition()
    {
        // // Hit ���¿��� �ٸ� ���·� ��ȯ�� ���� IsHitComplete üũ
        //if (owner.GetCurrentState() is HitState)
        //{
        //    return hitStrategy.IsHitComplete;
        //}
        
        // �ٸ� ���¿��� Hit ���·� ��ȯ�� ���� �׻� ����
        return true;
    }
}