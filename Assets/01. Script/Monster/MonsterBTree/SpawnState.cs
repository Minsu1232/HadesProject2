using static IMonsterState;

public class SpawnState : MonsterBaseState
{
    private readonly ISpawnStrategy spawnStrategy;

    public SpawnState(MonsterAI owner, ISpawnStrategy strategy) : base(owner)
    {
        spawnStrategy = strategy;
    }

    public override void Enter()
    {
        spawnStrategy.OnSpawn(transform, monsterClass);
    }

    public override void Execute()
    {
        if (spawnStrategy.IsSpawnComplete)
        {
            // 스폰 완료 후 Idle 상태로 전환
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition()
    {
        return spawnStrategy.IsSpawnComplete;
    }
}