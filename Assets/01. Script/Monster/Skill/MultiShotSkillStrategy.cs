using UnityEngine;

public class MultiShotSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private float lastSkillTime;
    private float skillCoolTime;

    // 추가: 발사 횟수 관리 변수
    private int shotsFired = 0;
    private int maxShots;

    public float SkillRange { get; set; }
    public bool IsSkillComplete => !isUsingSkill;  // 스킬 진행 중이면 false
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    public MultiShotSkillStrategy(CreatureAI owner)
    {
        monsterStatus = owner.GetStatus();
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;
    }

    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        maxShots = monsterData.CurrentMultiShotCount;
        skillCoolTime = monsterData.CurrentSkillCooldown;
        skillEffect.Initialize(monsterStatus, target);
        isUsingSkill = true;
        shotsFired = 0;  // 초기화
        lastSkillTime = Time.time;
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        // 스킬 효과 실행 (예: 총알 생성 등)
        skillEffect.Execute();
        shotsFired++;

        // 최대 발사 횟수에 도달하면 스킬 종료
        if (shotsFired >= maxShots)
        {
            Debug.Log("완료되어버림");
            CompleteSkill();
        }
    }

    private void CompleteSkill()
    {
        isUsingSkill = false;
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}
