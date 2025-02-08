using DG.Tweening;
using UnityEngine;
using static IMonsterState;

public abstract class BossPattern : BasePhysicalAttackStrategy
{
    protected BasicAttackStrategy basicAttack;
    protected JumpAttackStrategy jumpAttack;
    protected MiniGameManager miniGameManager;
    protected BossPatternManager patternManager;
    protected BossStatus bossStatus;
    protected AttackPatternData patternData;
    protected bool isRunning;
    protected Sequence patternSequence;
    protected BossData bossData_;
    protected Animator animator;
    protected CreatureAI owner;
    
    protected virtual bool IsExecutingPattern { get; }

    protected BossPattern(
        MiniGameManager miniGameManager, 
        GameObject shockwaveEffectPrefab, 
        float shockwaveRadius, 
        BossData bossData, 
        Animator animator,
        CreatureAI owner,        
        AttackPatternData patternData)
    {
        bossStatus = owner.GetComponent<BossStatus>();
        basicAttack = new BasicAttackStrategy();
        jumpAttack = new JumpAttackStrategy(shockwaveEffectPrefab, shockwaveRadius);
        bossData_ = bossData;
        this.animator = animator;
        this.miniGameManager = miniGameManager;
        this.owner = owner;
        this.patternManager = new BossPatternManager(bossStatus);
        this.patternData = patternData;
        
        miniGameManager.OnMiniGameComplete += HandleMiniGameComplete;
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isRunning)
        {
            StartPattern(transform, target, monsterData);
            return;
        }
        ExecutePattern(transform, target, monsterData);
    }

    protected abstract void StartPattern(Transform transform, Transform target, IMonsterClass monsterData);
    public abstract void ExecutePattern(Transform transform, Transform target, IMonsterClass monsterData);

    protected virtual void CompletePattern()
    {
        isRunning = false;
        if (patternSequence != null)
        {
            if (patternSequence.IsActive())
            {
                patternSequence.Kill();
            }
            patternSequence = null;
        }
        lastAttackTime = Time.time;
    }

    protected void StartMiniGame(MiniGameType type)
    {
        float difficulty = patternManager.GetPatternDifficulty(patternData);
        miniGameManager.StartMiniGame(type, difficulty);
    }

    protected virtual void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
        if (type == MiniGameType.Dodge)
        {
            CompletePattern();
            StopAttack();
            bool enterGroggy = patternManager.HandleMiniGameSuccess(result, patternData);
            miniGameManager.HandleDodgeReward(result);
            Debug.Log("헤이" + enterGroggy);
            if (enterGroggy)
            {
                Debug.Log("헤이222" + "들어옴");
                owner.ChangeState(MonsterStateType.Groggy);
                return;
            }

           
        }
    }

    public override void StopAttack()
    {
        CompletePattern();
        base.StopAttack();
    }

    public void Cleanup()
    {
        if (miniGameManager != null)
        {
            miniGameManager.OnMiniGameComplete -= HandleMiniGameComplete;
        }
        CompletePattern();
    }
}