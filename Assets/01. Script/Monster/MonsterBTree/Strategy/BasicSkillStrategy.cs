using UnityEngine;

public class BasicSkillStrategy : ISkillStrategy
{
    private bool isUsingSkill;
    private bool skillComplete;
    private float lastSkillTime = -999f; // 처음에는 바로 스킬 사용 가능하도록
    private float skillDamage;
    private float skillCoolTime;
    private float skillDuration;    // 스킬 시전까지의 시간
    private float skillRange; // 스킬 시전 가능한 범위
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

        // 스킬 진행 로직
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
        // 여기에 실제 스킬 효과 구현
        // 예: 범위 공격, 버프/디버프 등
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
               //distanceToTarget <= monsterClass.CurrentAttackRange * 1.5f; // 스킬 사거리는 좀 더 김
    }
}