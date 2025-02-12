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

    private float patternLastAttackTime;  // 패턴만의 타이머
    protected virtual bool IsExecutingPattern { get; }

    protected BossPattern(
        MiniGameManager miniGameManager, 
        GameObject shockwaveEffectPrefab, 
        float shockwaveRadius, 
        BossData bossData, 
        Animator animator,
        CreatureAI owner,        
        AttackPatternData patternData
       )
    {
        bossStatus = owner.GetComponent<BossStatus>();
        basicAttack = new BasicAttackStrategy();
        jumpAttack = new JumpAttackStrategy(shockwaveEffectPrefab, shockwaveRadius,owner);
        bossData_ = bossData;
        this.animator = animator;
        this.miniGameManager = miniGameManager;
        this.owner = owner;
        this.patternManager = new BossPatternManager(bossStatus);
        this.patternData = patternData;        
        miniGameManager.OnMiniGameComplete += HandleMiniGameComplete;

        patternLastAttackTime = Time.time; // 추가: 패턴 타이머를 현재 시간으로 초기화
    }

    public override void Attack(Transform transform, Transform target, IMonsterClass monsterData)
    {
        if (!isRunning)
        {
            isAttacking = true;
            StartPattern(transform, target, monsterData);
            return;
        }
        ExecutePattern(transform, target, monsterData);
    }
    public override bool CanAttack(float distanceToTarget, IMonsterClass monsterData)
    {
        float timeSinceLastPattern = Time.time - patternLastAttackTime;
        return timeSinceLastPattern >= patternData.patternCooldown &&
               distanceToTarget <= monsterData.CurrentAttackRange;
    }
    protected abstract void StartPattern(Transform transform, Transform target, IMonsterClass monsterData);
    public abstract void ExecutePattern(Transform transform, Transform target, IMonsterClass monsterData);

    protected virtual void CompletePattern()
    {
        //// 패턴 종료 시 이벤트 구독 해제
        //miniGameManager.OnMiniGameComplete -= HandleMiniGameComplete;

        isRunning = false;
        if (patternSequence != null)
        {
            if (patternSequence.IsActive())
            {
                patternSequence.Kill();
            }
            patternSequence = null;
        }
        patternLastAttackTime = Time.time;
        isAttacking = false;
    }

    protected void StartMiniGame(MiniGameType type)
    {
        float difficulty = patternManager.GetPatternDifficulty(patternData);
        miniGameManager.StartMiniGame(type, difficulty);
    }

    protected virtual void HandleMiniGameComplete(MiniGameType type, MiniGameResult result)
    {
             
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

            CompletePattern();
        
    }

    public override void StopAttack()
    {
        CompletePattern();
        base.StopAttack();
    }

    public void Cleanup()
    {
        //if (miniGameManager != null)
        //{
        //    miniGameManager.OnMiniGameComplete -= HandleMiniGameComplete;
        //}
        //CompletePattern();
    }
    public void CleanAll()
    {
        if (miniGameManager != null)
        {
            miniGameManager.OnMiniGameComplete -= HandleMiniGameComplete;
        }
    }
}