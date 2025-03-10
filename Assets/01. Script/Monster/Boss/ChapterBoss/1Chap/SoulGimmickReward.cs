using UnityEngine;

public class SoulGimmickReward : IGimmickReward
{
    private readonly BossMonster bossMonster;
    private const float PLAYER_SPEED_BUFF = 0.25f;     // ���� �� �÷��̾� �̵��ӵ� ���� 25%
    private const float PLAYER_ATTACK_BUFF = 0.2f; // ���� �� ���ݷ� ���� 20%
    private const float ESSENCE_RECOVERY = 15f;        // ���� �� ������ ȸ��
    private const float ESSENCE_INCREASE = 30f;        // ���� �� ������ ����

    public SoulGimmickReward(BossMonster bossMonster)
    {
        this.bossMonster = bossMonster;
    }

    public void ApplySuccess(BossAI boss, PlayerClass player)
    {
        // 1. �÷��̾� �̵��ӵ��� ���ݼӵ� ����
        player.ModifyPower(
            speedAmount: (int)(player.PlayerStats.Speed * PLAYER_SPEED_BUFF),
            attackAmount: (int)(player.PlayerStats.AttackPower * PLAYER_ATTACK_BUFF) 
        );
        
        // 2. ������ �ý��� ���� ������ ������ ����
        if (boss.GetBossMonster() is IBossWithEssenceSystem bossWithEssence)
        {
            bossWithEssence.InflictEssence(-ESSENCE_RECOVERY);
        }

        Debug.Log($"��ȥ ��� ����: �̵��ӵ� {PLAYER_SPEED_BUFF * 100}%, ���ݼӵ� {PLAYER_ATTACK_BUFF * 100}% ����");

        boss.ChangeState(IMonsterState.MonsterStateType.Groggy);
    }

    public void ApplyFailure(BossAI boss, PlayerClass player)
    {
        // 1. �⺻ ������ ó��
        player.TakeDamage((int)bossMonster.CurrentPhaseGimmickData.failDamage);

        // 2. ������ �ý��� ���� ������ ������ ����
        if (boss.GetBossMonster() is IBossWithEssenceSystem bossWithEssence)
        {
            bossWithEssence.InflictEssence(ESSENCE_INCREASE);
        }

        // 3. �÷��̾� �̵��ӵ� ���� (ª�� �ð�)
        player.ModifyPower(speedAmount: -player.PlayerStats.Speed*0.1f );

        Debug.Log($"��ȥ ��� ����: ������ {bossMonster.CurrentPhaseGimmickData.failDamage}, {-player.PlayerStats.Speed * 0.1f}����");
    }
}