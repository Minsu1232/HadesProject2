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
            // �� �������� ���� ���� ����
            Debug.Log(phaseData.phaseName);
          

            // ������ ��ȯ �� ��� ���� ������Ʈ
            bossAI.UpdatePhaseStrategies();
            //bossAI.SetupPhaseAttackStrategies(phaseData, bossData);
            if (boss.CurrentPhaseData.isInvulnerableDuringTransition)
            {
                bossUIManager.UpdatePhaseUI();
                boss.SetInvulnerable(false);
                boss.CurrentPhaseData.isInvulnerableDuringTransition = false;  // �� �κ��� �ʿ��ߴ� �ų׿�!
                Debug.Log("��ȭ�Ϸ�");

                Debug.Log(boss.CurrentPhase);
            }
        }
    }
}