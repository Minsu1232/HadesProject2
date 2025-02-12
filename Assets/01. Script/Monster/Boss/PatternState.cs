using UnityEngine;
using static IMonsterState;

public class PatternState : MonsterBaseState
{
    private readonly BossPattern pattern;
    private readonly BossData bossData;

    public PatternState(BossAI owner, BossPattern pattern) : base(owner)
    {
        this.pattern = pattern;
        this.bossData = (owner.GetStatus().GetMonsterClass() as BossMonster)?.GetBossData();
    }

    public override void Enter()
    {
        Debug.Log($"Entering Pattern State: {pattern.GetType().Name}");
        pattern.Attack(transform, player, monsterClass);
    }

    public override void Execute()
    {
        //// 패턴이 실행 중인지 체크
        //if (!pattern.IsAttacking)
        //{
           
        //    owner.ChangeState(MonsterStateType.Idle);
        //}
    }

    public override void Exit()
    {
        if (owner.GetAttackStrategy() is BasePhysicalAttackStrategy baseAttack)
        {
            baseAttack.UpdateLastAttackTime();  // 직접 필드 접근 대신 메서드 사용
            Debug.Log("지금 나감");
        }
        pattern.StopAttack();
        pattern.Cleanup();
    }

    public override bool CanTransition()
    {
        // 패턴 실행 중에는 다른 상태로 전환 못하게
        Debug.Log("curretn :" + pattern.IsAttacking);
        return !pattern.IsAttacking;
    }
}