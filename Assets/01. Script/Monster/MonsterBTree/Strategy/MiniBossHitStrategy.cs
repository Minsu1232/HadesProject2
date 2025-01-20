using static CartoonFX.CFXR_Effect;
using UnityEngine;

public class MiniBossHitStrategy : IHitStrategy
{
    private float hitStunDuration;
    private float hitTimer;
    private bool isHitComplete;
    private bool isInHitStun;
    private Vector3 knockbackDirection;

    public bool IsHitComplete => isHitComplete;

    public void OnHit(Transform transform, IMonsterClass monsterData, int damage)
    {
        ICreatureData data = monsterData.GetMonsterData();

        if (monsterData.CurrentArmor > 0)
        {
            isHitComplete = true;
            return;
        }

        hitStunDuration = monsterData.CurrentHitStunDuration * data.hitStunMultiplier;
        hitTimer = 0f;
        isHitComplete = false;
        isInHitStun = true;

   
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