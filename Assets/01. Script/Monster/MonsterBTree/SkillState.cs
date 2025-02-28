using System;
using UnityEditor.Rendering;
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
    private bool isInterrupted;

    // 스킬 상태를 나타내는 열거형
    private enum SkillPhase
    {
        NotStarted,      // 스킬 시작 전
        Starting,        // 스킬 시작 애니메이션 재생 중
        Executing,       // 스킬 효과 실행 중 (여러 발)
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
        Debug.Log("스킬시간초기화");
        skillTimer = 0f;
        isSkillAnimationComplete = false;
        hasSkillStarted = false;
        isInterrupted = false;
        currentPhase = SkillPhase.NotStarted;
    }

    public override void Enter()
    {
        ResetSkillState();

        // 애니메이션이 이미 재생 중이면 재시작, 아니면 트리거 설정
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

        // 스킬 실행 중일 때 플레이어 방향 추적
        if (player != null && currentPhase == SkillPhase.Executing)
        {
            UpdateRotation();
        }

        // 스킬 완료 조건 체크:
        // 애니메이션이 완료되었고, 스킬 전략(내부에서 발사 횟수 관리)이 완료되었으면 종료
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

    // 애니메이션 이벤트에 의해 호출: 스킬 발동 시작 (한 번만 호출)
    public void OnSkillStart()
    {
        if (isInterrupted || currentPhase != SkillPhase.Starting) return;

        try
        {
            // 널 참조 디버깅
            if (skillStrategy == null) Debug.LogError("skillStrategy is null");
            if (transform == null) Debug.LogError("transform is null");
            if (player == null) Debug.LogError("player is null");
            if (monsterClass == null) Debug.LogError("monsterClass is null");

            // 조건 검사 추가
            if (player == null || monsterClass == null)
            {
                Debug.LogError("필수 참조가 없습니다: player 또는 monsterClass가 null입니다.");
                ForceCompleteSkill();
                return;
            }

            // 여기서 스킬 전략에서 내부적으로 발사 횟수를 관리하도록 시작
            skillStrategy.StartSkill(transform, player, monsterClass);
            hasSkillStarted = true;
            currentPhase = SkillPhase.Executing;
            LogStateTransition("OnSkillStart", "Skill started");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnSkillStart: {e.Message}\nStackTrace: {e.StackTrace}");
            ForceCompleteSkill();
        }
    }

    // 애니메이션 이벤트에 의해 호출: 다중 발사를 위해 매 프레임 혹은 원하는 프레임마다 호출
    public void OnSkillEffect()
    {
        if (isInterrupted || currentPhase != SkillPhase.Executing)
        {
            Debug.Log("돌아가요~");
            return;
        }

        try
        {
            // 널 참조 검사
            if (player == null || monsterClass == null)
            {
                Debug.LogError("OnSkillEffect: player 또는 monsterClass가 null입니다.");
                return;
            }

            // 스킬 전략 내부에서 발사 횟수를 카운팅하고,
            // 최대 횟수 도달 시 내부적으로 CompleteSkill을 처리하도록 한다.
            skillStrategy.UpdateSkill(transform, player, monsterClass);
            Debug.Log("쏴요~");
            LogStateTransition("OnSkillEffect", "Shot fired");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnSkillEffect: {e.Message}\nStackTrace: {e.StackTrace}");
            ForceCompleteSkill();
        }
    }

    // 애니메이션 이벤트에 의해 호출: 스킬 애니메이션이 완료되었음을 알림
    public void OnSkillAnimationComplete()
    {
        if (isInterrupted) return;

        isSkillAnimationComplete = true;
        currentPhase = SkillPhase.Finishing;
        LogStateTransition("OnSkillAnimationComplete", "Animation complete");
    }

    // 스킬 완료 조건: 애니메이션 완료와 스킬 전략에서 발사 횟수 등 완료 여부가 모두 충족되면 완료
    private bool IsSkillComplete()
    {
        return isSkillAnimationComplete && skillStrategy.IsSkillComplete;
    }

    private void CompleteSkill()
    {
        currentPhase = SkillPhase.Completed;
        //if (owner.GetAttackStrategy() is BasePhysicalAttackStrategy baseAttack)
        //{
        //    baseAttack.UpdateLastAttackTime();  // 공격 후 타이밍 업데이트
        //}
        owner.ChangeState(MonsterStateType.Move);
        animator.ResetTrigger("SkillAttack");
        LogStateTransition("CompleteSkill", "Skill completed");
    }

    private void ForceCompleteSkill()
    {
        isSkillAnimationComplete = true;
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
                    return; // 보스면 인터럽트 불가능
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
        // 필요시 디버그 로그 활성화
        // Debug.Log($"[SkillState] {owner.gameObject.name}: {from} -> {to} (Time: {skillTimer:F2})");
    }
}

public enum InterruptReason
{
    Damaged,
    PhaseChange,
    Death
}