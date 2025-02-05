using UnityEngine;

public class HazardGimmickReward : IGimmickReward
{
    private readonly BossMonster bossMonster;
    private const float ATTACK_BUFF_MULTIPLIER = 0.3f;    // 공격력 버프 30%
    private const float BUFF_DURATION = 10f;              // 버프 지속시간
    private const float BOSS_DEBUFF_PERCENT = 0.2f;       // 보스 디버프 20%
    private const float BOSS_BUFF_PERCENT = 0.2f;         // 실패시 보스 버프 20%

    public HazardGimmickReward(BossMonster bossMonster)
    {
        this.bossMonster = bossMonster;
    }

    public void ApplySuccess(BossAI boss, PlayerClass player)
    {
        // 1. 플레이어 버프
        int attackBuff = (int)(player.PlayerStats.AttackPower * ATTACK_BUFF_MULTIPLIER);
        player.ModifyPower(attackAmount: attackBuff);

        // 2. 보스 디버프 (방어력과 속도 20% 감소)
        int defenseDebuff = -(int)(bossMonster.CurrentDeffense * BOSS_DEBUFF_PERCENT);
        int speedDebuff = -(int)(bossMonster.CurrentSpeed * BOSS_DEBUFF_PERCENT);

        bossMonster.ModifyStats(
            defenseAmount: defenseDebuff,
            speedAmount: speedDebuff
        );
        

        Debug.Log($"기믹 성공 보상 적용: 플레이어 공격력 {attackBuff} 증가, 보스 방어/속도 {BOSS_DEBUFF_PERCENT * 100}% 감소");
    }

    public void ApplyFailure(BossAI boss, PlayerClass player)
    {
        // 1. 데미지 처리 (CurrentPhaseGimmickData의 failDamage 사용)
        player.TakeDamage((int)bossMonster.CurrentPhaseGimmickData.failDamage);

        // 2. 보스 버프 (공격력과 속도 20% 증가)
        int attackBuff = (int)(bossMonster.CurrentAttackPower * BOSS_BUFF_PERCENT);
        int speedBuff = (int)(bossMonster.CurrentSpeed * BOSS_BUFF_PERCENT);

        bossMonster.ModifyStats(
            attackAmount: attackBuff,
            speedAmount: speedBuff
        );

        Debug.Log($"기믹 실패 패널티 적용: 데미지 {bossMonster.CurrentPhaseGimmickData.failDamage}, 보스 공격/속도 {BOSS_BUFF_PERCENT * 100}% 증가");
    }
 
}