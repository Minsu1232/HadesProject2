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
        Debug.Log("�̹̽�����");
    }

    protected override void StartPattern(Transform transform, Transform target, IMonsterClass monsterData)
    {

        patternSequence = DOTween.Sequence();
       
        Debug.Log("���� ��ŸƮ����");

        // ���� ���� ����
        currentSubAttackStrategy = jumpAttack;
        patternSequence.AppendCallback(() =>
        {
            jumpAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());
        });
        Debug.Log("���� ����");

        patternSequence.AppendInterval(1.5f);

        // �� ���� �⺻ ���ݰ� �̴ϰ���
        patternSequence.AppendCallback(() =>
        {
            Debug.Log("���� ������");
            currentSubAttackStrategy = basicAttack;
            basicAttack.Attack(transform, target, monsterData);
            animator.SetTrigger(GetAnimationTriggerName());

            StartMiniGame(MiniGameType.Dodge);
            miniGameStarted = true;

            // Ÿ�Ӿƿ� ó��
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