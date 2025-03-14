// StatConstants.cs
public static class StatConstants
{
    // 베이스 스탯 상수
    public const int BASE_HP = 200;
    public const int BASE_GAGE = 0;
    public const int BASE_ATTACK_POWER = 10;
    public const float BASE_ATTACK_SPEED = 1f;
    public const float BASE_CRITICAL_CHANCE = 0.3f;
    public const float BASE_SPEED = 10f;
    public const float BASE_DAMAGE_RECEIVE_RATE = 1f;

    // 업그레이드당 증가량 상수
    public const int HP_PER_UPGRADE = 5;
    public const int GAGE_PER_UPGRADE = 2;
    public const int ATTACK_POWER_PER_UPGRADE = 2;
    public const float ATTACK_SPEED_PER_UPGRADE = 0.1f;
    public const float CRITICAL_CHANCE_PER_UPGRADE = 0.05f;
    public const float SPEED_PER_UPGRADE = 0.1f;
    public const float DAMAGE_REDUCE_PER_UPGRADE = 0.05f;
}