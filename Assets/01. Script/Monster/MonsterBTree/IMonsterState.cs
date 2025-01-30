public interface IMonsterState
{
    public enum MonsterStateType
    {
        Spawn,
        Idle,
        Move,
        Attack,
        Skill,
        Hit,
        Groggy,
        PhaseTransition,
        Gimmick,
        Die
    }
    void Enter();
    void Execute();
    void Exit();
    bool CanTransition(); // 현재 상태에서 전환 가능한지
}