using UnityEngine;

/// <summary>
/// ������ ������ �����Ҹ� �����ϴ� �������̽�
/// </summary>
public interface IBossEssenceHazard
{
    /// <summary>
    /// ������ �̸�
    /// </summary>
    string HazardName { get; }

    /// <summary>
    /// ������ Ȱ��ȭ �Ӱ谪
    /// </summary>
    float ActivationThreshold { get; }

    /// <summary>
    /// ������ ������ ���
    /// </summary>
    float DamageMultiplier { get; }

    /// <summary>
    /// ������ �ý����� �����Ͽ� �ʱ�ȭ
    /// </summary>
    /// <param name="essenceSystem">������ ������ �ý���</param>
    void Initialize(IBossEssenceSystem essenceSystem);

    /// <summary>
    /// ������ Ȱ��ȭ
    /// </summary>
    /// <param name="position">Ȱ��ȭ ��ġ</param>
    /// <param name="intensity">���� (0.0~1.0)</param>
    void ActivateHazard(Vector3 position, float intensity);

    /// <summary>
    /// ������ ��Ȱ��ȭ
    /// </summary>
    void DeactivateHazard();

    /// <summary>
    /// ������ ��ġ ���濡 ���� ������ ���� ������Ʈ
    /// </summary>
    /// <param name="essenceValue">���� ������ ��ġ</param>
    void UpdateHazardIntensity(float essenceValue);
}