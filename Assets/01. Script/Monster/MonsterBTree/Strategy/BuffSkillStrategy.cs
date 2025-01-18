using UnityEngine;

public class BuffSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private MonsterStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete;
    private float lastSkillTime;
    private float skillCoolTime;
    private float skillDuration;    // 스킬 모션/시전 시간
    private float skillTimer;
    private bool hasExecutedSkill = false;
    
    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public BuffSkillStrategy(CreatureAI owner)
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

        isUsingSkill = true;
        skillComplete = false;
        hasExecutedSkill = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;

        //// 버프 스킬 시작 이펙트나 애니메이션이 필요한 경우
        //if (monsterStatus.TryGetComponent<Animator>(out var animator))
        //{
        //    animator.SetTrigger("BuffSkill");
        //}
    }

    public void UpdateSkill(Transform transform, Transform target, MonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // 버프는 시전 중에 방향 전환이 필요할 수 있음 (타겟 방향으로)
        if (!hasExecutedSkill)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(directionToTarget),
                    Time.deltaTime * 5f
                );
            }
        }

        // 스킬 시전 시간이 되었고, 아직 실행되지 않았다면 실행
        if (skillTimer >= skillDuration && !hasExecutedSkill)
        {
            skillEffect.Execute();  // 버프 효과 실행
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

    public bool CanUseSkill(float distanceToTarget, MonsterClass monsterData)
    {
        // 기본 조건
        bool basicConditions = !isUsingSkill &&
                             Time.time > lastSkillTime + skillCoolTime &&
                             distanceToTarget <= SkillRange;

        // 버프 스킬 특화 조건 (예: HP가 특정 비율 이하일 때만 사용)
        bool buffConditions = true;
        if (monsterStatus != null)
        {
            var currentHP = monsterStatus.GetMonsterClass().CurrentHealth;
            var maxHP = monsterStatus.GetMonsterClass().GetMonsterData().initialHp;
            buffConditions = (currentHP / (float)maxHP) <= 0.7f; // 체력 70% 이하일 때만 사용
        }

        return basicConditions || buffConditions;
    }
}