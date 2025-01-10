
using UnityEngine;

public class BossHitStrategy : IHitStrategy
{
    private float hitStunDuration;
    private float hitTimer;
    private bool isHitComplete;
    private bool isInHitStun;

    public bool IsHitComplete => isHitComplete;

    public void OnHit(Transform transform, MonsterClass monsterData, int damage)
    {
        MonsterData data = monsterData.GetMonsterData();

        if (monsterData.CurrentArmor > 0)
        {
            isHitComplete = true;
            return;
        }

        hitStunDuration = monsterData.CurrentHitStunDuration * data.hitStunMultiplier;
        hitTimer = 0f;
        isHitComplete = false;
        isInHitStun = true;

        // 보스는 넉백 없음
    }

    public void UpdateHit()
    {
        if (isInHitStun)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitStunDuration)
            {
                isHitComplete = true;
                isInHitStun = false;
            }
        }
    }
}