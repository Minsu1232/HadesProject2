public interface ISuccessUI
{
    /// <summary>
    /// ��� ���� �� ���� ī���� UI�� �ʱ�ȭ�մϴ�.
    /// </summary>
    void InitializeSuccessUI(int maxSuccessCount);

    /// <summary>
    /// ���� ���� Ƚ���� ������Ʈ�մϴ�.
    /// </summary>
    void UpdateSuccessCount(int currentSuccessCount);

    void UpdateTimeBar(float time);
    void UIOff();
}
