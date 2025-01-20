
using UnityEngine;

public class BasicHitStrategy : IHitStrategy
{
   private float hitStunDuration;
    private float hitTimer;
    private bool isHitComplete;
    private bool isInHitStun;
    
    public bool IsHitComplete => isHitComplete;

    public void OnHit(Transform transform, IMonsterClass monsterData, int damage)
    {
        ICreatureData data = monsterData.GetMonsterData();
        
        // 기본 몬스터는 모든 공격에 경직
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