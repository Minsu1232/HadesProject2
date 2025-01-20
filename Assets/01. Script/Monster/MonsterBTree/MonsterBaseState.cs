using UnityEngine;

public abstract class MonsterBaseState : IMonsterState
{
    protected CreatureAI owner;
    protected ICreatureStatus status;       // MonsterStatus 대신 인터페이스 사용
    protected IMonsterClass monsterClass;   // MonsterClass 대신 인터페이스 사용
    protected Transform transform;       // 몬스터의 Transform
    protected Transform player;          // 플레이어 Transform

    public MonsterBaseState(CreatureAI owner)  // 생성자도 CreatureAI로 변경
    {
        this.owner = owner;
        this.status = owner.GetStatus();
        this.monsterClass = status.GetMonsterClass();
        this.transform = owner.transform;
        this.player = GameInitializer.Instance.GetPlayerClass().playerTransform;
    }

    public virtual void Enter()
    {
        // 상태 진입 시 기본 동작
    }

    public virtual void Execute()
    {
        // 상태 업데이트 시 기본 동작
    }

    public virtual void Exit()
    {
        // 상태 종료 시 기본 동작
    }

    public virtual bool CanTransition()
    {
        // 기본적으로 상태 전환 가능
        return true;
    }

    // 유틸리티 메서드들
    protected float GetDistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    protected bool IsInRange(float range)
    {
        return GetDistanceToPlayer() <= range;
    }

    protected Vector3 GetDirectionToPlayer()
    {
        return (player.position - transform.position).normalized;
    }
}