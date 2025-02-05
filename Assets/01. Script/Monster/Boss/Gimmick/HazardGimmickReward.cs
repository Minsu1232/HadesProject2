using UnityEngine;

public class HazardGimmickReward : IGimmickReward
{
    private readonly BossMonster bossMonster;
    private const float ATTACK_BUFF_MULTIPLIER = 0.3f;    // ���ݷ� ���� 30%
    private const float BUFF_DURATION = 10f;              // ���� ���ӽð�
    private const float BOSS_DEBUFF_PERCENT = 0.2f;       // ���� ����� 20%
    private const float BOSS_BUFF_PERCENT = 0.2f;         // ���н� ���� ���� 20%

    public HazardGimmickReward(BossMonster bossMonster)
    {
        this.bossMonster = bossMonster;
    }

    public void ApplySuccess(BossAI boss, PlayerClass player)
    {
        // 1. �÷��̾� ����
        int attackBuff = (int)(player.PlayerStats.AttackPower * ATTACK_BUFF_MULTIPLIER);
        player.ModifyPower(attackAmount: attackBuff);

        // 2. ���� ����� (���°� �ӵ� 20% ����)
        int defenseDebuff = -(int)(bossMonster.CurrentDeffense * BOSS_DEBUFF_PERCENT);
        int speedDebuff = -(int)(bossMonster.CurrentSpeed * BOSS_DEBUFF_PERCENT);

        bossMonster.ModifyStats(
            defenseAmount: defenseDebuff,
            speedAmount: speedDebuff
        );
        

        Debug.Log($"��� ���� ���� ����: �÷��̾� ���ݷ� {attackBuff} ����, ���� ���/�ӵ� {BOSS_DEBUFF_PERCENT * 100}% ����");
    }

    public void ApplyFailure(BossAI boss, PlayerClass player)
    {
        // 1. ������ ó�� (CurrentPhaseGimmickData�� failDamage ���)
        player.TakeDamage((int)bossMonster.CurrentPhaseGimmickData.failDamage);

        // 2. ���� ���� (���ݷ°� �ӵ� 20% ����)
        int attackBuff = (int)(bossMonster.CurrentAttackPower * BOSS_BUFF_PERCENT);
        int speedBuff = (int)(bossMonster.CurrentSpeed * BOSS_BUFF_PERCENT);

        bossMonster.ModifyStats(
            attackAmount: attackBuff,
            speedAmount: speedBuff
        );

        Debug.Log($"��� ���� �г�Ƽ ����: ������ {bossMonster.CurrentPhaseGimmickData.failDamage}, ���� ����/�ӵ� {BOSS_BUFF_PERCENT * 100}% ����");
    }
 
}