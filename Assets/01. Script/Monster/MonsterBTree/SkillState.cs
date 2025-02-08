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

    // ��ų ���� ������ ���� ������
    private enum SkillPhase
    {
        NotStarted,      // ��ų ���� ��
        Starting,        // ��ų ���� �ִϸ��̼� ��� ��
        Executing,       // ��ų ȿ�� ���� ��
        Finishing,       // ��ų ���� �ִϸ��̼� ��� ��
        Completed,       // ��ų ���� ����
        Interrupted      // ��ų ���� �ߴ�
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

        // ���� �ִϸ��̼� ���� Ȯ�� �� ó��
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

        // Ÿ�Ӿƿ� üũ
        if (skillTimer >= skillTimeoutDuration)
        {
            LogStateTransition(currentPhase.ToString(), "Timeout");
            ForceCompleteSkill();
            return;
        }

        // �÷��̾� ���� ���� (���� ���� ����)
        if (player != null && currentPhase == SkillPhase.Executing)
        {
            UpdateRotation();
        }

        // ��ų �Ϸ� ���� üũ
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

    // �ִϸ��̼� �̺�Ʈ���� ȣ��� �޼����
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
            baseAttack.UpdateLastAttackTime();  // ���� �ʵ� ���� ��� �޼��� ���
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
                    return; // �����̰� ���ͷ�Ʈ�� �Ұ����ϸ� ���⼭ ����
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