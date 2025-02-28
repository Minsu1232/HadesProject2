using UnityEngine;

public class BossPhaseTransitionStrategy : IPhaseTransitionStrategy
{
    private readonly BossMonster boss;
    private readonly BossAI bossAI;
    private readonly BossData bossData;
    private BossUIManager bossUIManager;
    private float transitionTimer = 0f;
    private bool isComplete = false;
    PhaseData phaseData;
    public bool IsTransitionComplete => isComplete;

    public BossPhaseTransitionStrategy(BossMonster boss,BossAI bossAI, BossUIManager bossUIManager)
    {
        this.boss = boss;
        this.bossData = boss.GetBossData();
        this.bossAI = bossAI;        
        this.bossUIManager = bossUIManager;
    }

    public void StartTransition()
    {
        transitionTimer = 0f;
        isComplete = false;
        boss.IncreasePhase();
        phaseData = boss.CurrentPhaseData;

        boss.ModifyStats(
            attackAmount: (int)((boss.CurrentAttackPower * phaseData.damageMultiplier) - boss.CurrentAttackPower),
            speedAmount: (int)((boss.CurrentSpeed * phaseData.speedMultiplier) - boss.CurrentSpeed),
            defenseAmount: (int)((boss.CurrentDeffense * phaseData.defenseMultiplier) - boss.CurrentDeffense)
           
        );
       
       
    }

    public void UpdateTransition()
    {
        transitionTimer += Time.deltaTime;
        
        if (transitionTimer >= boss.CurrentPhaseData.transitionDuration)
        {
            isComplete = true;
            // 새 페이즈의 공격 전략 설정
            Debug.Log(phaseData.phaseName);
          

            // 페이즈 전환 시 모든 전략 업데이트
            bossAI.UpdatePhaseStrategies();
            //bossAI.SetupPhaseAttackStrategies(phaseData, bossData);
            if (boss.CurrentPhaseData.isInvulnerableDuringTransition)
            {
                bossUIManager.UpdatePhaseUI();
                boss.SetInvulnerable(false);
                boss.CurrentPhaseData.isInvulnerableDuringTransition = false;  // 이 부분은 필요했던 거네요!
                Debug.Log("변화완료");

                Debug.Log(boss.CurrentPhase);
            }
        }
    }
}