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
    public Transform Target { get; set; } // 타겟 속성 구현
    public BasicSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
        Target = null;
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
        Debug.Log("Initialize 호출됨: " + new System.Diagnostics.StackTrace());
    }


    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {// StartSkill 메서드 시작 부분에
        Debug.Log($"StartSkill: 보스={monsterData.MONSTERNAME}, 스킬쿨={monsterData.CurrentSkillCooldown}, 스킬지속시간={monsterData.CurrentSKillDuration}");
        Debug.Log("!!!!!!!!!!!" + "시ㅏ작했으");
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
        Debug.Log("@@@@@#@#!" + "들어왔으?");
        if (!isUsingSkill) return;
        Debug.Log("@@@@@#@#!" + "들어왔으!");
        skillTimer += Time.deltaTime;
        // UpdateSkill 메서드 안에서 조건 직전에
        Debug.Log($"조건 검사: skillTimer={skillTimer}, skillDuration={skillDuration}, hasExecutedSkill={hasExecutedSkill}");
        // 스킬 시전 시간이 되었고, 아직 실행되지 않았다면 실행
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
            Debug.Log("실행됐으");
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
        Debug.Log("실행됐으22222222");
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