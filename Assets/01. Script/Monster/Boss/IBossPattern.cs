public interface IBossPattern
{
    void Initialize(BossAI boss, PhaseData phaseData);  // 패턴 초기화
    void Execute();                                     // 패턴 실행
    void OnPhaseStart();                               // 페이즈 시작시 호출
    void OnPhaseEnd();                                 // 페이즈 종료시 호출
    bool CanTransition();                              // 다른 패턴으로 전환 가능한지
}