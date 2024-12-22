using UnityEngine;

public class BasicSkillStrategy : ISkillStrategy
{
    private bool isUsingSkill;
    private bool skillComplete;
    private float lastSkillTime = -999f; // ó������ �ٷ� ��ų ��� �����ϵ���
    private float skillDamage;
    private float skillCoolTime;
    private float skillDuration;    // ��ų ���������� �ð�
    private float skillRange; // ��ų ���� ������ ����
    private float skillTimer;

    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public void StartSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        skillDamage = monsterData.CurrentSkillDamage;
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillDuration = monsterData.CurrentSKillDuration;
        skillRange = monsterData.CurrentSkillRange;
        isUsingSkill = true;
        skillComplete = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;
    }

    public void UpdateSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // ��ų ���� ����
        if (skillTimer <= skillDuration)
        {
            ExecuteSkillLogic(transform, target, monsterData);
        }
        else
        {
            CompleteSkill();
        }
    }

    private void ExecuteSkillLogic(Transform transform, Transform target, MonsterClass monsterData)
    {
        // ���⿡ ���� ��ų ȿ�� ����
        // ��: ���� ����, ����/����� ��
        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass != null)
        {            
            playerClass.TakeDamage((int)skillDamage, AttackData.AttackType.Charge);
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
        skillComplete = true;
    }

    public bool CanUseSkill(float distanceToTarget, MonsterClass monsterData)
    {
        return !isUsingSkill;
               //Time.time >= lastSkillTime + monsterClass.CurrentSkillCooldown &&
               //distanceToTarget <= monsterClass.CurrentAttackRange * 1.5f; // ��ų ��Ÿ��� �� �� ��
    }
}