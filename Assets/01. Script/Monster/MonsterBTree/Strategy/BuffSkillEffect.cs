using UnityEngine;

public class BuffSkillEffect : ISkillEffect
{
    private BuffType buffType;
    private float duration;
    private float value;  // ���� ��ġ (������/���ҷ�)
    private MonsterStatus monsterStatus;
    private Transform target;

    public BuffSkillEffect(BuffType type, float duration, float value)
    {
        this.buffType = type;
        this.duration = duration;
        this.value = value;
    }

    public void Initialize(MonsterStatus status, Transform target)
    {
        this.monsterStatus = status;
        this.target = target;
    }
    public void Execute()
    {
        // ���� ���� ���� ����
    }

    public void OnComplete()
    {
        // ���� ���� ó��
    }
}