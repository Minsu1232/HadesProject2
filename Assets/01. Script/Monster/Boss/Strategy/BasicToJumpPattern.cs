using DG.Tweening;
using UnityEngine;
using static AttackData;

public class BasicToJumpPattern : BossPattern
{
    bool isExecutingPattern;
    protected override bool IsExecutingPattern => isExecutingPattern;
    private BasePhysicalAttackStrategy currentSubAttackStrategy;

    public BasicToJumpPattern(
        MiniGameManager miniGameManager,
        GameObject shockwaveEffectPrefab,
        float shockwaveRadius,
        BossData bossData,
        Animator animator,
        CreatureAI owner,        
        AttackPatternData patternData
    ) : base(miniGameManager, shockwaveEffectPrefab, shockwaveRadius, bossData, animator, owner, patternData)
    {
    }

    public override PhysicalAttackType AttackType =>
        currentSubAttackStrategy != null ? currentSubAttackStrategy.AttackType : PhysicalAttackType.Basic;

    public override string GetAnimationTriggerName() =>
        currentSubAttackStrategy != null ? currentSubAttackStrategy.GetAnimationTriggerName() : base.GetAnimationTriggerName();

    public override void ExecutePattern(Transform transform, Transform target, IMonsterClass monsterData)
    {
        Debug.Log("이미실행중");
    }

    protected override void StartPattern(Transform transform, Transform target, IMonsterClass monsterData)
    {
        patternSequence = DOTween.Sequence();
        Debug.Log("들어옴 스타트패턴");

        // 기본 공격
        currentSubAttackStrategy = basicAttack;
        
        patternSequence.AppendCallback(() => basicAttack.Attack(transform, target, monsterData));
        Debug.Log("들어옴 베이직");
        patternSequence.AppendInterval(1f);

        // 점프 공격과 미니게임
        patternSequence.AppendCallback(() =>
        {
            Debug.Log("들어옴 점프");
            
            currentSubAttackStrategy = jumpAttack;
            jumpAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
            StartMiniGame(MiniGameType.Dodge);
        });

        patternSequence.AppendInterval(1f);
        patternSequence.OnComplete(() =>
        {
            Debug.Log("Tween 시퀀스 완료 - 미니게임 대기중");
        });

        isExecutingPattern = true;
        patternSequence.Play();
    }

    protected override void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
        base.HandleMiniGameComplete(type, result);
    }
}