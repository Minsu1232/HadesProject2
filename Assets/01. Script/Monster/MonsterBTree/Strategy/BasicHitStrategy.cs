
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
        
        // �⺻ ���ʹ� ��� ���ݿ� ����
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