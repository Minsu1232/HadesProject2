using UnityEngine;

public class BuffSkillEffect : ISkillEffect
{
    private BuffType[] buffTypes;
    private float[] durations;
    private float[] values;
    private ICreatureStatus monsterStatus;
    private Transform target;
    private GameObject buffPrefab;
    bool isNopr;
    private Transform monsterTransform;  // 추가
    public BuffSkillEffect(BuffType[] buffTypes, float[] durations, float[] values, GameObject buffPrefab, Transform monsterTransform)
    {
        this.buffTypes = buffTypes;
        this.durations = durations;
        this.values = values;
        this.buffPrefab = buffPrefab;
        this.monsterTransform = monsterTransform;  // 몬스터의 Transform 저장
    }

    public void Initialize(ICreatureStatus status, Transform target)
    {
        this.monsterStatus = status;
        this.target = target;

        if (buffTypes == null || buffTypes.Length == 0)
        {
            Debug.LogError("BuffSkillEffect: No buff types specified!");
            return;
        }

        // 기본값 설정 - 배열 길이가 맞지 않을 경우를 대비
        if (durations == null || durations.Length == 0)
        {
            durations = new float[] { 5f };  // 기본 지속시간 5초
        }
        if (values == null || values.Length == 0)
        {
            values = new float[] { 50f };    // 기본 수치값 50
        }
    }

    public void Execute()
    {
        if (monsterStatus == null)
        {
            Debug.LogError("BuffSkillEffect: MonsterStatus is not initialized!");
            return;
        }

        for (int i = 0; i < buffTypes.Length; i++)
        {
            float duration = i < durations.Length ? durations[i] : durations[0];
            float value = i < values.Length ? values[i] : values[0];

            ApplyBuff(buffTypes[i], value, duration);
            SpawnBuffEffect();


        }


    }

    private void ApplyBuff(BuffType buffType, float value, float duration)
    {
        switch (buffType)
        {
            case BuffType.AttackUp:
                monsterStatus.ApplyTemporaryModifier("attackpower", (int)value, duration);

                break;

            case BuffType.DefenseUp:
                monsterStatus.ApplyTemporaryModifier("defense", (int)value, duration);

                break;

            case BuffType.SpeedUp:
                monsterStatus.ApplyTemporaryModifier("speed", (int)value, duration);

                break;

            case BuffType.AttackSpeedUp:
                monsterStatus.ApplyTemporaryModifier("attackspeed", value, duration);

                break;

            case BuffType.Heal:
                monsterStatus.ModifyHealth((int)value);

                break;

            case BuffType.Rage:
                // Rage는 공격력과 공격속도를 동시에 증가
                monsterStatus.ApplyTemporaryModifier("attackpower", (int)value, duration);
                monsterStatus.ApplyTemporaryModifier("attackspeed", value * 0.5f, duration);

                break;

            case BuffType.Invincible:
                monsterStatus.ApplyTemporaryModifier("defense", int.MaxValue, duration);

                break;

            // 디버프
            case BuffType.AttackDown:
                monsterStatus.ApplyTemporaryModifier("attackpower", -(int)value, duration);

                break;

            case BuffType.DefenseDown:
                monsterStatus.ApplyTemporaryModifier("defense", -(int)value, duration);

                break;

            case BuffType.SpeedDown:
                monsterStatus.ApplyTemporaryModifier("speed", -(int)value, duration);

                break;

            case BuffType.Stun:
                // 현재 속도를 0으로 만듦
                int currentSpeed = monsterStatus.GetMonsterClass().CurrentSpeed;
                monsterStatus.ApplyTemporaryModifier("speed", -currentSpeed, duration);

                break;

            //case BuffType.Poison:
            //    StartDotEffect(DamageType.Poison, (int)value, duration);
            //    SpawnBuffEffect("PoisonVFX");
            //    break;

            //case BuffType.Burn:
            //    StartDotEffect(DamageType.Burn, (int)value, duration);
            //    SpawnBuffEffect("BurnVFX");
            //    break;

            //case BuffType.Freeze:
            //    // 이동 속도와 공격 속도를 모두 감소
            //    monsterStatus.ApplyTemporaryModifier("speed", -(int)(value * 0.8f), duration);
            //    monsterStatus.ApplyTemporaryModifier("attackspeed", -value * 0.5f, duration);
            //    SpawnBuffEffect("FreezeVFX");
            //    break;

            default:
                Debug.LogWarning($"Unhandled buff type: {buffType}");
                break;
        }
    }

    private void SpawnBuffEffect()
    {

        if (buffPrefab != null && monsterTransform != null)
        {
            GameObject effect = GameObject.Instantiate(buffPrefab,
                monsterTransform.transform);

            GameObject.Destroy(effect, 2f);
        }
        else
        {
            Debug.LogWarning("BuffSkillEffect: buffPrefab or monsterTransform is null!");
        }
    }


    public void OnComplete()
    {
        // 모든 임시 효과는 ApplyTemporaryModifier에서 자동으로 제거됨
        // 추가적인 정리가 필요한 경우 여기에 구현
    }
}