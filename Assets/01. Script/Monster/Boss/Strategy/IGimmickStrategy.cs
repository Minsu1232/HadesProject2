using UnityEngine;

public interface IGimmickStrategy
{
    void StartGimmick();
    void UpdateGimmick();
    bool IsGimmickComplete { get; }
    string GetGimmickAnimationTrigger();
    IGimmickStrategy GetGimmickStrategy();
    void SucessTrigget();

    AudioClip GetOptionalRoarSound();
}

public enum GimmickSubState
{
    Moving,     // 지정된 위치로 이동 중
    Performing  // 실제 기믹 수행 중
}