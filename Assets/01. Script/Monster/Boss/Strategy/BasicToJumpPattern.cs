using DG.Tweening;
using UnityEngine;
using static AttackData;

public class BasicToJumpPattern : BossPattern
{
    bool isExecutingPattern;
    protected override bool IsExecutingPattern => isExecutingPattern;
    private BasePhysicalAttackStrategy currentSubAttackStrategy;
    private const float MINIGAME_TIMEOUT = 3f; // 미니게임 제한 시간 설정
    private bool miniGameStarted = false;
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
        patternSequence = DOTween.Sequence().SetId("PatternSequence2");
        DOTween.logBehaviour = LogBehaviour.Verbose;
        Debug.Log("들어옴 스타트패턴");

        // 기본 공격
        currentSubAttackStrategy = basicAttack;
        patternSequence.AppendCallback(() =>
        {
            basicAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
        });

        patternSequence.AppendInterval(1f);

        // 점프 공격과 미니게임
        patternSequence.AppendCallback(() =>
        {
            Debug.Log("들어옴 점프");
            currentSubAttackStrategy = jumpAttack;
            jumpAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
            StartMiniGame(MiniGameType.Dodge);
            miniGameStarted = true;

            // 타임아웃 시퀀스 추가
            DOVirtual.DelayedCall(MINIGAME_TIMEOUT, () =>
            {
                if (miniGameStarted && isExecutingPattern)
                {
                    Debug.Log("미니게임 시간 초과 - 자동 실패");
                    HandleMiniGameComplete(MiniGameType.Dodge, MiniGameResult.Miss);
                    miniGameStarted = false;
                }
            });
        });

        patternSequence.OnComplete(() =>
        {
            Debug.Log("Tween 시퀀스 완료 - 미니게임 대기중");
        });

        isExecutingPattern = true;
        patternSequence.Play();
    }

    protected override void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
        if (!miniGameStarted) return; // 이미 타임아웃으로 처리된 경우 중복 처리 방지

        miniGameStarted = false;
        base.HandleMiniGameComplete(type, result);
    }
    protected override void CompletePattern()
    {
        miniGameStarted = false;
        base.CompletePattern();
    }
}