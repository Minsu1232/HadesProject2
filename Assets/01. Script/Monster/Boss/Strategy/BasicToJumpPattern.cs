using DG.Tweening;
using UnityEngine;
using static AttackData;

public class BasicToJumpPattern : BossPattern
{
    bool isExecutingPattern;
    protected override bool IsExecutingPattern => isExecutingPattern;
    private BasePhysicalAttackStrategy currentSubAttackStrategy;
    private const float MINIGAME_TIMEOUT = 3f; // �̴ϰ��� ���� �ð� ����
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
        Debug.Log("�̹̽�����");
    }

    protected override void StartPattern(Transform transform, Transform target, IMonsterClass monsterData)
    { 
        patternSequence = DOTween.Sequence().SetId("PatternSequence2");
        DOTween.logBehaviour = LogBehaviour.Verbose;
        Debug.Log("���� ��ŸƮ����");

        // �⺻ ����
        currentSubAttackStrategy = basicAttack;
        patternSequence.AppendCallback(() =>
        {
            basicAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
        });

        patternSequence.AppendInterval(1f);

        // ���� ���ݰ� �̴ϰ���
        patternSequence.AppendCallback(() =>
        {
            Debug.Log("���� ����");
            currentSubAttackStrategy = jumpAttack;
            jumpAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
            StartMiniGame(MiniGameType.Dodge);
            miniGameStarted = true;

            // Ÿ�Ӿƿ� ������ �߰�
            DOVirtual.DelayedCall(MINIGAME_TIMEOUT, () =>
            {
                if (miniGameStarted && isExecutingPattern)
                {
                    Debug.Log("�̴ϰ��� �ð� �ʰ� - �ڵ� ����");
                    HandleMiniGameComplete(MiniGameType.Dodge, MiniGameResult.Miss);
                    miniGameStarted = false;
                }
            });
        });

        patternSequence.OnComplete(() =>
        {
            Debug.Log("Tween ������ �Ϸ� - �̴ϰ��� �����");
        });

        isExecutingPattern = true;
        patternSequence.Play();
    }

    protected override void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
        if (!miniGameStarted) return; // �̹� Ÿ�Ӿƿ����� ó���� ��� �ߺ� ó�� ����

        miniGameStarted = false;
        base.HandleMiniGameComplete(type, result);
    }
    protected override void CompletePattern()
    {
        miniGameStarted = false;
        base.CompletePattern();
    }
}