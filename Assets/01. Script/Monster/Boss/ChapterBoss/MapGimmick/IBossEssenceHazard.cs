using UnityEngine;

/// <summary>
/// 보스의 에센스 위험요소를 정의하는 인터페이스
/// </summary>
public interface IBossEssenceHazard
{
    /// <summary>
    /// 위험요소 이름
    /// </summary>
    string HazardName { get; }

    /// <summary>
    /// 위험요소 활성화 임계값
    /// </summary>
    float ActivationThreshold { get; }

    /// <summary>
    /// 위험요소 데미지 배수
    /// </summary>
    float DamageMultiplier { get; }

    /// <summary>
    /// 에센스 시스템을 연결하여 초기화
    /// </summary>
    /// <param name="essenceSystem">보스의 에센스 시스템</param>
    void Initialize(IBossEssenceSystem essenceSystem);

    /// <summary>
    /// 위험요소 활성화
    /// </summary>
    /// <param name="position">활성화 위치</param>
    /// <param name="intensity">강도 (0.0~1.0)</param>
    void ActivateHazard(Vector3 position, float intensity);

    /// <summary>
    /// 위험요소 비활성화
    /// </summary>
    void DeactivateHazard();

    /// <summary>
    /// 에센스 수치 변경에 따른 위험요소 강도 업데이트
    /// </summary>
    /// <param name="essenceValue">현재 에센스 수치</param>
    void UpdateHazardIntensity(float essenceValue);
}