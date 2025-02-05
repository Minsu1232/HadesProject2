using UnityEngine;

public class BossPhaseTransitionStrategy : IPhaseTransitionStrategy
{
    private readonly BossMonster boss;
    private readonly BossAI bossAI;
    private readonly BossData bossData;
    private float transitionTimer = 0f;
    private bool isComplete = false;
    PhaseData phaseData;
    public bool IsTransitionComplete => isComplete;

    public BossPhaseTransitionStrategy(BossMonster boss,BossAI bossAI)
    {
        this.boss = boss;
        this.bossData = boss.GetBossData();
        this.bossAI = bossAI;
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
            if (boss.CurrentPhaseData.isInvulnerableDuringTransition)
            { // 새 페이즈의 공격 전략 설정
                bossAI.SetupPhaseAttackStrategies(phaseData, bossData);
                boss.SetInvulnerable(false);
                boss.CurrentPhaseData.isInvulnerableDuringTransition = false;  // 이 부분은 필요했던 거네요!
                Debug.Log("변화완료");
            }
        }
    }
}