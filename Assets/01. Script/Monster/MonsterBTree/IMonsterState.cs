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
        Die
    }
    void Enter();
    void Execute();
    void Exit();
    bool CanTransition(); // ���� ���¿��� ��ȯ ��������
}