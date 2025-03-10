using UnityEngine;

public class SoulGimmickReward : IGimmickReward
{
    private readonly BossMonster bossMonster;
    private const float PLAYER_SPEED_BUFF = 0.25f;     // 성공 시 플레이어 이동속도 증가 25%
    private const float PLAYER_ATTACK_BUFF = 0.2f; // 성공 시 공격력 증가 20%
    private const float ESSENCE_RECOVERY = 15f;        // 성공 시 에센스 회복
    private const float ESSENCE_INCREASE = 30f;        // 실패 시 에센스 증가

    public SoulGimmickReward(BossMonster bossMonster)
    {
        this.bossMonster = bossMonster;
    }

    public void ApplySuccess(BossAI boss, PlayerClass player)
    {
        // 1. 플레이어 이동속도와 공격속도 버프
        player.ModifyPower(
            speedAmount: (int)(player.PlayerStats.Speed * PLAYER_SPEED_BUFF),
            attackAmount: (int)(player.PlayerStats.AttackPower * PLAYER_ATTACK_BUFF) 
        );
        
        // 2. 에센스 시스템 가진 보스는 에센스 감소
        if (boss.GetBossMonster() is IBossWithEssenceSystem bossWithEssence)
        {
            bossWithEssence.InflictEssence(-ESSENCE_RECOVERY);
        }

        Debug.Log($"영혼 기믹 성공: 이동속도 {PLAYER_SPEED_BUFF * 100}%, 공격속도 {PLAYER_ATTACK_BUFF * 100}% 증가");

        boss.ChangeState(IMonsterState.MonsterStateType.Groggy);
    }

    public void ApplyFailure(BossAI boss, PlayerClass player)
    {
        // 1. 기본 데미지 처리
        player.TakeDamage((int)bossMonster.CurrentPhaseGimmickData.failDamage);

        // 2. 에센스 시스템 가진 보스는 에센스 증가
        if (boss.GetBossMonster() is IBossWithEssenceSystem bossWithEssence)
        {
            bossWithEssence.InflictEssence(ESSENCE_INCREASE);
        }

        // 3. 플레이어 이동속도 감소 (짧은 시간)
        player.ModifyPower(speedAmount: -player.PlayerStats.Speed*0.1f );

        Debug.Log($"영혼 기믹 실패: 데미지 {bossMonster.CurrentPhaseGimmickData.failDamage}, {-player.PlayerStats.Speed * 0.1f}감소");
    }
}