using UnityEngine;

public class BuffSkillEffect : ISkillEffect
{
    private BuffType buffType;
    private float duration;
    private float value;  // 버프 수치 (증가량/감소량)
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
        // 버프 적용 로직 구현
    }

    public void OnComplete()
    {
        // 버프 종료 처리
    }
}