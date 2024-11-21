[System.Serializable]
public class AttackData
{
  
    public AttackType attackType;    // ���� Ÿ��
    public float hitStopDuration;    // ��Ʈ��ž ���� �ð�
    public float shakeIntensity;     // ��鸲 ����
    public float shakeDuration;      // ��鸲 ���� �ð�

    public AttackData(AttackType type, float hitStop, float intensity, float duration)
    {
        attackType = type;
        hitStopDuration = hitStop;
        shakeIntensity = intensity;
        shakeDuration = duration;
    }

    public enum AttackType
    {
        Normal,    // �⺻ ����
        Charge     // ������
    }

    public AttackData normalAttack = new AttackData(AttackType.Normal, 0.1f, 0.2f, 0.2f);
    public AttackData chargeAttack = new AttackData(AttackType.Charge, 0.3f, 0.5f, 0.5f);
}