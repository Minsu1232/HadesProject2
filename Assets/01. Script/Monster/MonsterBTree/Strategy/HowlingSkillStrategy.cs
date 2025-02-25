using UnityEngine;

/// <summary>
/// ���¢�� ��ų ���� Ŭ����
/// ���� �ֺ��� ���� �ĵ��� ������ �÷��̾�� ������ �ִ� ��ų
/// </summary>
public class HowlingSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;
    private float skillTimer;

    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public HowlingSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
    }

    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillDuration = monsterData.CurrentSKillDuration;
        skillEffect.Initialize(monsterStatus, target);

        // ��ų ���� ���¸� ����
        isUsingSkill = true;
        skillComplete = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;

        Debug.Log("���¢�� ��ų ����");
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // ��ų ���� �ð��� ���� �� ȿ�� ����
        if (skillTimer >= skillDuration * 0.5f && !skillComplete)
        {
            skillEffect.Execute();
            skillComplete = true;
        }

        // ��ų ���ӽð��� ������ �Ϸ�
        if (skillTimer >= skillDuration)
        {
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
        skillTimer = 0;
        Debug.Log("���¢�� ��ų �Ϸ�");
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}