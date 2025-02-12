using UnityEngine;

public class BasicSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;    // 스킬 모션/시전 시간
    private float skillTimer;
    private bool hasExecutedSkill = false;  // 스킬이 실제로 실행되었는지 체크
    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public BasicSkillStrategy(CreatureAI owner)
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

        // 스킬 시작 상태만 설정
        isUsingSkill = true;
        skillComplete = false;
        hasExecutedSkill = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;
       
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;
       
        skillTimer += Time.deltaTime;

        // 스킬 시전 시간이 되었고, 아직 실행되지 않았다면 실행
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
           
            skillEffect.Execute();  // 실제 스킬 실행
            hasExecutedSkill = true;
        }

        // 스킬이 실행되었다면 완료 처리
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

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}