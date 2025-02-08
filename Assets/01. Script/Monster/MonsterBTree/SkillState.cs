using System;
using UnityEngine;
using static IMonsterState;

public class SkillState : MonsterBaseState
{
    private readonly ISkillStrategy skillStrategy;
    private Animator animator;
    private readonly float skillTimeoutDuration;
    private float skillTimer;
    private bool isSkillAnimationComplete;
    private bool hasSkillStarted;
    private bool hasSkillEffectApplied;
    private bool isInterrupted;

    // 스킬 상태 관리를 위한 열거형
    private enum SkillPhase
    {
        NotStarted,      // 스킬 시작 전
        Starting,        // 스킬 시작 애니메이션 재생 중
        Executing,       // 스킬 효과 실행 중
        Finishing,       // 스킬 종료 애니메이션 재생 중
        Completed,       // 스킬 정상 종료
        Interrupted      // 스킬 강제 중단
    }
    private SkillPhase currentPhase = SkillPhase.NotStarted;

    public SkillState(CreatureAI owner, ISkillStrategy strategy, float timeout = 5f) : base(owner)
    {
        skillStrategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        animator = owner.GetComponent<Animator>();
        skillTimeoutDuration = timeout;
        ResetSkillState();
    }

    private void ResetSkillState()
    {
        skillTimer = 0f;
        isSkillAnimationComplete = false;
        hasSkillStarted = false;
        hasSkillEffectApplied = false;
        isInterrupted = false;
        currentPhase = SkillPhase.NotStarted;
    }

    public override void Enter()
    {
        ResetSkillState();

        // 현재 애니메이션 상태 확인 및 처리
        var currentAnimState = animator.GetCurrentAnimatorStateInfo(0);
        if (currentAnimState.IsName("SkillAttack"))
        {
            animator.Play("SkillAttack", 0, 0f);
            LogStateTransition("Enter", "Restarting SkillAttack animation");
        }
        else
        {
            animator.SetTrigger("SkillAttack");
            LogStateTransition("Enter", "Setting SkillAttack trigger");
        }

        currentPhase = SkillPhase.Starting;
    }

    public override void Execute()
    {
        if (isInterrupted) return;

        skillTimer += Time.deltaTime;

        // 타임아웃 체크
        if (skillTimer >= skillTimeoutDuration)
        {
            LogStateTransition(currentPhase.ToString(), "Timeout");
            ForceCompleteSkill();
            return;
        }

        // 플레이어 방향 추적 (실행 중일 때만)
        if (player != null && currentPhase == SkillPhase.Executing)
        {
            UpdateRotation();
        }

        // 스킬 완료 조건 체크
        if (IsSkillComplete() && currentPhase != SkillPhase.Completed)
        {
            CompleteSkill();
        }
    }

    private void UpdateRotation()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    // 애니메이션 이벤트에서 호출될 메서드들
    public void OnSkillStart()
    {
        if (isInterrupted || currentPhase != SkillPhase.Starting) return;

        try
        {
            skillStrategy.StartSkill(transform, player, monsterClass);
            hasSkillStarted = true;
            currentPhase = SkillPhase.Executing;
            LogStateTransition("OnSkillStart", "Success");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnSkillStart: {e.Message}");
            ForceCompleteSkill();
        }
    }

    public void OnSkillEffect()
    {
        if (isInterrupted || currentPhase != SkillPhase.Executing) return;

        try
        {
            skillStrategy.UpdateSkill(transform, player, monsterClass);
            hasSkillEffectApplied = true;
            LogStateTransition("OnSkillEffect", "Success");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnSkillEffect: {e.Message}");
            ForceCompleteSkill();
        }
    }

    public void OnSkillAnimationComplete()
    {
        if (isInterrupted) return;

        isSkillAnimationComplete = true;
        currentPhase = SkillPhase.Finishing;
        LogStateTransition("OnSkillAnimationComplete", "Success");
    }

    private bool IsSkillComplete()
    {
        return isSkillAnimationComplete && skillStrategy.IsSkillComplete && hasSkillEffectApplied;
    }

    private void CompleteSkill()
    {
        currentPhase = SkillPhase.Completed;
        if (owner.GetAttackStrategy() is BasePhysicalAttackStrategy baseAttack)
        {
            baseAttack.UpdateLastAttackTime();  // 직접 필드 접근 대신 메서드 사용
        }
        owner.ChangeState(MonsterStateType.Move);
        animator.ResetTrigger("SkillAttack");
    }
    private void ForceCompleteSkill()
    {
        isSkillAnimationComplete = true;
        hasSkillEffectApplied = true;
        CompleteSkill();
        LogStateTransition("ForceCompleteSkill", "Forced completion");
    }

    public void InterruptSkill(InterruptReason reason)
    {
        if (isInterrupted) return;

        isInterrupted = true;
        currentPhase = SkillPhase.Interrupted;
        LogStateTransition("InterruptSkill", reason.ToString());

        switch (reason)
        {
            case InterruptReason.Damaged:
                var bossMonster = monsterClass as BossMonster;
                if (bossMonster != null && !bossMonster.CanBeInterrupted)
                {
                    return; // 보스이고 인터럽트가 불가능하면 여기서 종료
                }
                animator.ResetTrigger("SkillAttack");
                animator.Play("Hit");
                owner.ChangeState(MonsterStateType.Hit);
                break;

            case InterruptReason.PhaseChange:
            case InterruptReason.Death:
                ForceCompleteSkill();
                break;
        }
    }

    public override void Exit()
    {
        if (currentPhase != SkillPhase.Completed && currentPhase != SkillPhase.Interrupted)
        {
            LogStateTransition("Exit", "Forced exit");
            animator.ResetTrigger("SkillAttack");
        }
    }

    public override bool CanTransition()
    {
        return currentPhase == SkillPhase.Completed || currentPhase == SkillPhase.Interrupted;
    }

    private void LogStateTransition(string from, string to)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log($"[SkillState] {owner.gameObject.name}: {from} -> {to} (Time: {skillTimer:F2})");
        }
    }
}

public enum InterruptReason
{
    Damaged,
    PhaseChange,
    Death
}