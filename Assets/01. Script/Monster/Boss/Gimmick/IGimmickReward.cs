// 기믹 보상 인터페이스
public interface IGimmickReward
{
    void ApplySuccess(BossAI boss, PlayerClass player);
    void ApplyFailure(BossAI boss, PlayerClass player);
}