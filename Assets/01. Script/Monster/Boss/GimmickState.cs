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
        // 선택적 사운드 재생
        bossUIManager.ShowGimmickWarning();
        AudioClip clip = gimmickStrategy.GetOptionalRoarSound();
        if (clip != null)
        {
            // 필요에 따라 위치나 볼륨을 조절할 수 있음.
            AudioSource.PlayClipAtPoint(clip, owner.transform.position);
        }
        gimmickStrategy.StartGimmick();
    }

    public override void Execute()
    {
        Debug.Log("들어옴@@@@@@@@@@@@@@@@@@@" + gimmickStrategy.IsGimmickComplete);
        gimmickStrategy.UpdateGimmick();
        if (gimmickStrategy.IsGimmickComplete)
        {
            Debug.Log("들어옴@@@@@@@@@@@@@@@@@@@");
            animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(), false);            


        }
    }

    public override bool CanTransition()
    {
        animator.SetBool(gimmickStrategy.GetGimmickAnimationTrigger(), !gimmickStrategy.IsGimmickComplete);
        return gimmickStrategy.IsGimmickComplete;
    } 
}