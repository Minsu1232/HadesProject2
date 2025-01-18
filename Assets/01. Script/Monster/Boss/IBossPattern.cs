public interface IBossPattern
{
    void Initialize(BossAI boss, PhaseData phaseData);  // ���� �ʱ�ȭ
    void Execute();                                     // ���� ����
    void OnPhaseStart();                               // ������ ���۽� ȣ��
    void OnPhaseEnd();                                 // ������ ����� ȣ��
    bool CanTransition();                              // �ٸ� �������� ��ȯ ��������
}