// ICreatureAI.cs
using static AttackData;
using static IMonsterState;

public interface ICreatureAI
{
    void ChangeState(MonsterStateType newState);
    IMonsterState GetCurrentState();
    void OnDamaged(int damage, AttackType attackType);
    MonsterStatus GetStatus();
}