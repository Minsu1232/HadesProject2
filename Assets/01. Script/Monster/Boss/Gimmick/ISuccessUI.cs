public interface ISuccessUI
{
    /// <summary>
    /// 기믹 시작 시 성공 카운터 UI를 초기화합니다.
    /// </summary>
    void InitializeSuccessUI(int maxSuccessCount);

    /// <summary>
    /// 현재 성공 횟수를 업데이트합니다.
    /// </summary>
    void UpdateSuccessCount(int currentSuccessCount);

    void UpdateTimeBar(float time);
    void UIOff();
}
