using UnityEngine;

public class BasicHitStrategy : IHitStrategy
{
    private float hitStunDuration;  // �ǰ� ���� �ð�
    private float hitTimer;
    private bool isHitComplete;

    public bool IsHitComplete => isHitComplete;

    public void OnHit(Transform transform, MonsterClass monsterData, int damage)
    {
        hitStunDuration = monsterData.CurrentHitStunDuration;
        hitTimer = 0f;
        isHitComplete = false;

        // ���⿡ �ǰ� ȿ�� (����Ʈ, �ִϸ��̼� ��) �߰� ����
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