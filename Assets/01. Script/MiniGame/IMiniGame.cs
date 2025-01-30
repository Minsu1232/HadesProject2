// �̴ϰ��� ����� Ÿ�� ����
public enum MiniGameResult
{
    Perfect,
    Good,
    Miss,
    Cancel
}

public enum MiniGameType
{
    None,
    Dodge,
    Parry,
    QuickTime
}

// �⺻ �������̽�
public interface IMiniGame
{
    MiniGameType Type { get; }
    bool IsComplete { get; }
    void Initialize(float difficulty);
    void Start();
    void Update();
    void Cancel();
    MiniGameResult GetResult();
}