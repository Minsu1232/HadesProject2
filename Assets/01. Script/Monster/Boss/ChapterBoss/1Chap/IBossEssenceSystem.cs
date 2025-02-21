public interface IBossEssenceSystem
{
    // Properties
    string BossEssenceName { get; }
    float CurrentEssence { get; }     // ���� ���� ��ġ
    float MaxEssence { get; }         // �ִ� ���� ��ġ
    bool IsInEssenceState { get; }    // 70% �̻� ��������
    bool IsMaxEssence { get; }        // 100% ��������

    // Methods
    void IncreaseEssence(float amount);    // ���� ����
    void DecreaseEssence(float amount);    // ���� ����
    void UpdateEssence();                  // ���� ���� ������Ʈ

    // Events
    event System.Action<float> OnEssenceChanged;         // ���� ��ġ �����
    event System.Action OnEssenceStateChanged;           // 70% ����/������
    event System.Action OnMaxEssenceStateChanged;        // 100% ����/������
}