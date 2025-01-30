// 미니게임 결과와 타입 정의
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

// 기본 인터페이스
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