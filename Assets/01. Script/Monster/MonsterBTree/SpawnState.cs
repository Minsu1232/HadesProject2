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
            // ���� �Ϸ� �� Idle ���·� ��ȯ
            owner.ChangeState(MonsterStateType.Idle);
        }
    }

    public override bool CanTransition()
    {
        return spawnStrategy.IsSpawnComplete;
    }
}