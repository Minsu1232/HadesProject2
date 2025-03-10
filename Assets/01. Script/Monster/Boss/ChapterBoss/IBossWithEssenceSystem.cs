/// <summary>
/// 에센스 시스템을 가진 모든 보스가 구현해야 하는 인터페이스
/// </summary>
public interface IBossWithEssenceSystem
{
    /// <summary>
    /// 보스의 에센스 시스템을 반환
    /// </summary>
    /// <returns>보스가 사용하는 에센스 시스템</returns>
    IBossEssenceSystem GetEssenceSystem();

    /// <summary>
    /// 보스의 에센스 시스템 타입을 반환
    /// </summary>
    /// <returns>에센스 시스템 타입 (광기, 지식, 망자 등)</returns>
    EssenceType GetEssenceType();

    // 에센스 영향 메서드 추가
    void InflictEssence(float amount);
}

/// <summary>
/// 에센스 시스템의 유형
/// </summary>
public enum EssenceType
{
    Madness,    // 야수족 - 광기
    Knowledge,  // 용족 - 지식
    Death,      // 언데드 - 죽음
    Fusion      // 최종 보스 - 융합
}