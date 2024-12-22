using Unity.VisualScripting;
using UnityEngine;

public class BasicDieStrategy : IDieStrategy
{
    private float deathDuration;  // 사망 연출 시간
    private float deathTimer;
    private bool isDeathComplete;

    public bool IsDeathComplete => isDeathComplete;

    public void OnDie(Transform transform, MonsterClass monsterData)
    {
        deathDuration = monsterData.CurrentDeathDuration;
        deathTimer = 0f;
        isDeathComplete = false;
        
        // 여기에 사망 효과 (이펙트, 애니메이션 등) 추가 가능
    }

    public void UpdateDeath()
    {
        if (!isDeathComplete)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= deathDuration)
            {
                isDeathComplete = true;
                // 사망 처리 (오브젝트 제거 등)
                
            }
        }
    }
}