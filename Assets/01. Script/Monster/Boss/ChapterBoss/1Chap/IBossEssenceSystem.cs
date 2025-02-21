public interface IBossEssenceSystem
{
    // Properties
    string BossEssenceName { get; }
    float CurrentEssence { get; }     // 현재 본질 수치
    float MaxEssence { get; }         // 최대 본질 수치
    bool IsInEssenceState { get; }    // 70% 이상 상태인지
    bool IsMaxEssence { get; }        // 100% 상태인지

    // Methods
    void IncreaseEssence(float amount);    // 본질 증가
    void DecreaseEssence(float amount);    // 본질 감소
    void UpdateEssence();                  // 본질 상태 업데이트

    // Events
    event System.Action<float> OnEssenceChanged;         // 본질 수치 변경시
    event System.Action OnEssenceStateChanged;           // 70% 도달/해제시
    event System.Action OnMaxEssenceStateChanged;        // 100% 도달/해제시
}