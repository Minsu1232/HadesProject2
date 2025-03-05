using UnityEngine;
using System;

/// <summary>
/// 지속성 영역 효과를 가진 스킬들을 관리하는 전략 클래스
/// </summary>
public class AreaSkillStrategy : ISkillStrategy
{
    private ISkillEffect skillEffect;
    private ICreatureStatus monsterStatus;
    private bool isUsingSkill = false;
    private bool skillComplete = false;
    private float lastSkillTime;
    private float skillCoolTime;
    public Transform Target { get; set; } // 타겟 속성 구현
    // 스킬 상태 관리용 변수들
    private float skillExecutionTimer = 0f;    // 스킬이 실행된 후 경과 시간
    private bool skillExecuted = false;        // 스킬 실행 여부
    private bool effectApplied = false;        // 효과 적용 여부

    // 타이밍 관련 변수들
    private float totalDuration;               // 스킬 총 지속 시간
    private float effectDuration;              // 효과 지속 시간 (howlDuration과 같은 값)

    public float SkillRange { get; set; }
    public bool IsSkillComplete => skillComplete;
    public bool IsUsingSkill => isUsingSkill;
    public float GetLastSkillTime => lastSkillTime;

    protected CreatureAI owner;

    public AreaSkillStrategy(CreatureAI owner)
    {
        this.owner = owner;
        monsterStatus = owner.GetStatus();
        Target = owner.transform;
    }

    public void Initialize(ISkillEffect effect)
    {
        this.skillEffect = effect;

        // 모든 스킬 이펙트에 대해 동일한 방식으로 이벤트 구독
        effect.OnEffectCompleted += () => {
            Debug.Log("[AreaSkillStrategy] 효과 완료 콜백 수신");
            // 효과가 완료되면 스킬도 완료 처리
            CompleteSkill();
        };

        Debug.Log($"[AreaSkillStrategy] 초기화됨: {effect.GetType().Name}");
    }

    public void StartSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        try
        {
            Debug.Log($"[AreaSkillStrategy] 스킬 시작: 보스={monsterData.MONSTERNAME}");

            // 쿨타임 설정
            skillCoolTime = monsterData.CurrentSkillCooldown;

            // 효과 지속 시간 설정 (HowlSkillEffect라면 howlDuration 사용)
            if (skillEffect is HowlSkillEffect)
            {
                // BossData에서 howlDuration 값을 가져옴 (BossData로 캐스팅 필요)
                ICreatureData bossData = monsterData.GetMonsterData();
                if (bossData != null)
                {
                    effectDuration = bossData.howlDuration;
                    Debug.Log($"[AreaSkillStrategy] Howl 지속 시간: {effectDuration}초");
                }
                else
                {
                    effectDuration = 2.0f; // 기본값
                    Debug.Log("[AreaSkillStrategy] 경고: BossData 캐스팅 실패, 기본 지속 시간 2초 사용");
                }
            }
            else
            {
                // 다른 영역 효과 스킬은 skillDuration 또는 다른 값 사용
                effectDuration = monsterData.CurrentSKillDuration;
            }
            Debug.Log("!@#!@#!@#" + effectDuration);
            // 총 지속 시간은 효과 지속 시간 + 약간의 여유 시간
            totalDuration = effectDuration + 1.0f;

            // 스킬 이펙트 초기화
            skillEffect.Initialize(monsterStatus, target);

            // 스킬 상태 초기화
            isUsingSkill = true;
            skillComplete = false;
            skillExecuted = false;
            effectApplied = false;
            skillExecutionTimer = 0f;
            lastSkillTime = Time.time;

            Debug.Log($"[AreaSkillStrategy] 스킬 초기화 완료. 효과 지속 시간: {effectDuration}초, 총 지속 시간: {totalDuration}초");
        }
        catch (Exception e)
        {
            Debug.LogError($"[AreaSkillStrategy] StartSkill 오류: {e.Message}\n{e.StackTrace}");
            CompleteSkill(); // 오류 발생 시 스킬 완료 처리
        }
    }

    public void UpdateSkill(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isUsingSkill) return;

        try
        {
            // 스킬 실행 시간 업데이트
            skillExecutionTimer += Time.deltaTime;

            // 스킬 효과 실행 (처음 한 번만)
            if (!skillExecuted)
            {
                Debug.Log("[AreaSkillStrategy] 스킬 효과 실행");
                skillEffect.Execute();
                skillExecuted = true;

                // 여기서는 스킬 효과만 시작하고, 실제 효과 적용은 
                // 효과 내부에서 지연 처리됨 (HowlSkillEffect의 경우 DOTween으로)
            }

            // 총 지속 시간이 지나면 스킬 완료
            if (skillExecutionTimer >= totalDuration)
            {
                Debug.Log($"[AreaSkillStrategy] 스킬 지속 시간 종료: {skillExecutionTimer:F2}초 경과");
                CompleteSkill();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AreaSkillStrategy] UpdateSkill 오류: {e.Message}\n{e.StackTrace}");
            CompleteSkill(); // 오류 발생 시 스킬 완료 처리
        }
    }

    protected void CompleteSkill()
    {
        Debug.Log("[AreaSkillStrategy] 스킬 완료");
        isUsingSkill = false;
        skillComplete = true;
        skillExecuted = false;
        effectApplied = false;
        skillExecutionTimer = 0f;
        lastSkillTime = Time.time;
        // 스킬 이펙트의 OnComplete 호출
        if (skillEffect != null)
        {
            skillEffect.OnComplete();
        }
    }

    public bool CanUseSkill(float distanceToTarget, IMonsterClass monsterData)
    {
        return !isUsingSkill &&
               Time.time > lastSkillTime + skillCoolTime &&
               distanceToTarget <= SkillRange;
    }
}