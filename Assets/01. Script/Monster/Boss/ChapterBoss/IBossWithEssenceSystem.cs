/// <summary>
/// ������ �ý����� ���� ��� ������ �����ؾ� �ϴ� �������̽�
/// </summary>
public interface IBossWithEssenceSystem
{
    /// <summary>
    /// ������ ������ �ý����� ��ȯ
    /// </summary>
    /// <returns>������ ����ϴ� ������ �ý���</returns>
    IBossEssenceSystem GetEssenceSystem();

    /// <summary>
    /// ������ ������ �ý��� Ÿ���� ��ȯ
    /// </summary>
    /// <returns>������ �ý��� Ÿ�� (����, ����, ���� ��)</returns>
    EssenceType GetEssenceType();

    // ������ ���� �޼��� �߰�
    void InflictEssence(float amount);
}

/// <summary>
/// ������ �ý����� ����
/// </summary>
public enum EssenceType
{
    Madness,    // �߼��� - ����
    Knowledge,  // ���� - ����
    Death,      // �𵥵� - ����
    Fusion      // ���� ���� - ����
}