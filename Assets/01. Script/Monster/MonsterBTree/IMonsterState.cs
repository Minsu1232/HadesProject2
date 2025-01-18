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
        Die
    }
    void Enter();
    void Execute();
    void Exit();
    bool CanTransition(); // ���� ���¿��� ��ȯ ��������
}