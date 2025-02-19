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
    private Transform monsterTransform;  // �߰�
    public BuffSkillEffect(BuffType[] buffTypes, float[] durations, float[] values, GameObject buffPrefab, Transform monsterTransform)
    {
        this.buffTypes = buffTypes;
        this.durations = durations;
        this.values = values;
        this.buffPrefab = buffPrefab;
        this.monsterTransform = monsterTransform;  // ������ Transform ����
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

        // �⺻�� ���� - �迭 ���̰� ���� ���� ��츦 ���
        if (durations == null || durations.Length == 0)
        {
            durations = new float[] { 5f };  // �⺻ ���ӽð� 5��
        }
        if (values == null || values.Length == 0)
        {
            values = new float[] { 50f };    // �⺻ ��ġ�� 50
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
                // Rage�� ���ݷ°� ���ݼӵ��� ���ÿ� ����
                monsterStatus.ApplyTemporaryModifier("attackpower", (int)value, duration);
                monsterStatus.ApplyTemporaryModifier("attackspeed", value * 0.5f, duration);

                break;

            case BuffType.Invincible:
                monsterStatus.ApplyTemporaryModifier("defense", int.MaxValue, duration);

                break;

            // �����
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
                // ���� �ӵ��� 0���� ����
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
            //    // �̵� �ӵ��� ���� �ӵ��� ��� ����
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
        // ��� �ӽ� ȿ���� ApplyTemporaryModifier���� �ڵ����� ���ŵ�
        // �߰����� ������ �ʿ��� ��� ���⿡ ����
    }
}