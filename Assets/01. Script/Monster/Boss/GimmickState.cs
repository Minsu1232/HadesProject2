using static IMonsterState;
using UnityEngine;

public class GimmickState : MonsterBaseState
{
    private readonly IGimmickStrategy gimmickStrategy;
    private Animator animator;
    BossUIManager bossUIManager;
    public GimmickState(CreatureAI owner, IGimmickStrategy strategy, BossUIManager bossUIManager) : base(owner)
    {
        gimmickStrategy = strategy;
        animator = owner.GetComponent<Animator>();
        this.bossUIManager = bossUIManager;
    }

    public override void Enter()
    {
        animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(),true);
        // ������ ���� ���
        bossUIManager.ShowGimmickWarning();
        AudioClip clip = gimmickStrategy.GetOptionalRoarSound();
        if (clip != null)
        {
            // �ʿ信 ���� ��ġ�� ������ ������ �� ����.
            AudioSource.PlayClipAtPoint(clip, owner.transform.position);
        }
        gimmickStrategy.StartGimmick();
    }

    public override void Execute()
    {
        Debug.Log("����@@@@@@@@@@@@@@@@@@@" + gimmickStrategy.IsGimmickComplete);
        gimmickStrategy.UpdateGimmick();
        if (gimmickStrategy.IsGimmickComplete)
        {
            Debug.Log("����@@@@@@@@@@@@@@@@@@@");
            animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(), false);            


        }
    }

    public override bool CanTransition()
    {
        animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(), !gimmickStrategy.IsGimmickComplete);
        return gimmickStrategy.IsGimmickComplete;
    } 
}