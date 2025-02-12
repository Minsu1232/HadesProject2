using DG.Tweening;
using UnityEngine;

public class JumpToBasicPattern : BossPattern
{
    private const float MINIGAME_TIMEOUT = 2f;
    private bool miniGameStarted = false;
    bool isExecutingPattern;
    protected override bool IsExecutingPattern => isExecutingPattern;
    private BasePhysicalAttackStrategy currentSubAttackStrategy;

    public JumpToBasicPattern(
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
        currentSubAttackStrategy != null ? currentSubAttackStrategy.AttackType : PhysicalAttackType.Jump;

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

        // 점프 공격 먼저
        currentSubAttackStrategy = jumpAttack;
        patternSequence.AppendCallback(() =>
        {
            jumpAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
        });
        Debug.Log("들어옴 점프");

        patternSequence.AppendInterval(1.5f);

        // 그 다음 기본 공격과 미니게임
        patternSequence.AppendCallback(() =>
        {
            Debug.Log("들어옴 베이직");
            currentSubAttackStrategy = basicAttack;
            basicAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());

            StartMiniGame(MiniGameType.Dodge);
            miniGameStarted = true;

            // 타임아웃 처리
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
        if (!miniGameStarted) return;

        miniGameStarted = false;
        base.HandleMiniGameComplete(type, result);
    }

    protected override void CompletePattern()
    {
        miniGameStarted = false;
        base.CompletePattern();
    }


}