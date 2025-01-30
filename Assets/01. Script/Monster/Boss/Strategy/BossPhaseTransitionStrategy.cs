using UnityEngine;

public class BossPhaseTransitionStrategy : IPhaseTransitionStrategy
{
    private readonly BossMonster boss;
    private readonly BossData bossData;
    private float transitionTimer = 0f;
    private bool isComplete = false;

    public bool IsTransitionComplete => isComplete;

    public BossPhaseTransitionStrategy(BossMonster boss)
    {
        this.boss = boss;
        this.bossData = boss.GetBossData();
    }

    public void StartTransition()
    {
        transitionTimer = 0f;
        isComplete = false;
        boss.IncreasePhase();
        PhaseData phaseData = boss.CurrentPhaseData;

        boss.ModifyStats(
            attackAmount: (int)((boss.CurrentAttackPower * phaseData.damageMultiplier) - boss.CurrentAttackPower),
            speedAmount: (int)((boss.CurrentSpeed * phaseData.speedMultiplier) - boss.CurrentSpeed),
            defenseAmount: (int)((boss.CurrentDeffense * phaseData.defenseMultiplier) -  boss.CurrentDeffense)
        );
        if (boss.CurrentPhaseData.isInvulnerableDuringTransition)
        {
            boss.SetInvulnerable(true);
            Debug.Log("��ȭ����");
        }
       
    }

    public void UpdateTransition()
    {
        transitionTimer += Time.deltaTime;
        
        if (transitionTimer >= boss.CurrentPhaseData.transitionDuration)
        {
            isComplete = true;
            if (boss.CurrentPhaseData.isInvulnerableDuringTransition)
            {
                boss.SetInvulnerable(false);
                boss.CurrentPhaseData.isInvulnerableDuringTransition = false;  // �� �κ��� �ʿ��ߴ� �ų׿�!
                Debug.Log("��ȭ�Ϸ�");
            }
        }
    }
}