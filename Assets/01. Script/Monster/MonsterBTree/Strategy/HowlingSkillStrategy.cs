using UnityEngine;

/// <summary>
/// 울부짖음 스킬 전략 클래스
/// 보스 주변에 원형 파동을 일으켜 플레이어에게 영향을 주는 스킬
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

        // 스킬 시작 상태만 설정
        isUsingSkill = true;
        skillComplete = false;
        skillTimer = 0f;
        lastSkillTime = Time.time;

        Debug.Log("울부짖음 스킬 시작");
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        skillTimer += Time.deltaTime;

        // 스킬 시전 시간이 됐을 때 효과 실행
        if (skillTimer >= skillDuration * 0.5f && !skillComplete)
        {
            skillEffect.Execute();
            skillComplete = true;
        }

        // 스킬 지속시간이 끝나면 완료
        if (skillTimer >= skillDuration)
        {
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
        skillTimer = 0;
        Debug.Log("울부짖음 스킬 완료");
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}