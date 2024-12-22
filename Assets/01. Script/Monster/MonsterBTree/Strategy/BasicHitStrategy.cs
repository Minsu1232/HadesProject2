using UnityEngine;

public class BasicHitStrategy : IHitStrategy
{
    private float hitStunDuration;  // 피격 경직 시간
    private float hitTimer;
    private bool isHitComplete;

    public bool IsHitComplete => isHitComplete;

    public void OnHit(Transform transform, MonsterClass monsterData, int damage)
    {
        hitStunDuration = monsterData.CurrentHitStunDuration;
        hitTimer = 0f;
        isHitComplete = false;

        // 여기에 피격 효과 (이펙트, 애니메이션 등) 추가 가능
    }

    public void UpdateHit()
    {
        if (!isHitComplete)
        {
            hitTimer += Time.deltaTime;
            if (hitTimer >= hitStunDuration)
            {
                isHitComplete = true;
            }
        }
    }
}