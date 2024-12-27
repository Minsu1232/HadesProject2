using UnityEngine;

public class BasicSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private MonsterStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;    // ��ų ���/���� �ð�
    private float skillTimer;
    private bool hasExecutedSkill = false;  // ��ų�� ������ ����Ǿ����� üũ
    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public BasicSkillStrategy(MonsterAI owner)
    {
        monsterStatus = owner.GetStatus();
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
    }

    public void StartSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillDuration = monsterData.CurrentSKillDuration;
        skillEffect.Initialize(monsterStatus, target);

        // ��ų ���� ���¸� ����
        isUsingSkill = true;
        skillComplete = false;
        hasExecutedSkill = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;
    }

    public void UpdateSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // ��ų ���� �ð��� �Ǿ���, ���� ������� �ʾҴٸ� ����
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
            skillEffect.Execute();  // ���� ��ų ����
            hasExecutedSkill = true;
        }

        // ��ų�� ����Ǿ��ٸ� �Ϸ� ó��
        if (hasExecutedSkill)
        {
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
        skillComplete = true;
        hasExecutedSkill = false;
        skillTimer = 0;
    }

    public bool CanUseSkill(float distanceToTarget, MonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}