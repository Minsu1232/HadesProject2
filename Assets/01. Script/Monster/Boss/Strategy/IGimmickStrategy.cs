public interface IGimmickStrategy
{
    void StartGimmick();
    void UpdateGimmick();
    bool IsGimmickComplete { get; }
    string GetGimmickAnimationTrigger();
    IGimmickStrategy GetGimmickStrategy();
    void SucessTrigget();
}