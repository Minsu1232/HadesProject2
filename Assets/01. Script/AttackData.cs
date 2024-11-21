[System.Serializable]
public class AttackData
{
  
    public AttackType attackType;    // 공격 타입
    public float hitStopDuration;    // 히트스탑 지속 시간
    public float shakeIntensity;     // 흔들림 강도
    public float shakeDuration;      // 흔들림 지속 시간

    public AttackData(AttackType type, float hitStop, float intensity, float duration)
    {
        attackType = type;
        hitStopDuration = hitStop;
        shakeIntensity = intensity;
        shakeDuration = duration;
    }

    public enum AttackType
    {
        Normal,    // 기본 공격
        Charge     // 강공격
    }

    public AttackData normalAttack = new AttackData(AttackType.Normal, 0.1f, 0.2f, 0.2f);
    public AttackData chargeAttack = new AttackData(AttackType.Charge, 0.3f, 0.5f, 0.5f);
}