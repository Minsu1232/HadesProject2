using Unity.VisualScripting;
using UnityEngine;

public class BasicDieStrategy : IDieStrategy
{
    private float deathDuration;  // ��� ���� �ð�
    private float deathTimer;
    private bool isDeathComplete;

    public bool IsDeathComplete => isDeathComplete;

    public void OnDie(Transform transform, MonsterClass monsterData)
    {
        deathDuration = monsterData.CurrentDeathDuration;
        deathTimer = 0f;
        isDeathComplete = false;
        
        // ���⿡ ��� ȿ�� (����Ʈ, �ִϸ��̼� ��) �߰� ����
    }

    public void UpdateDeath()
    {
        if (!isDeathComplete)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= deathDuration)
            {
                isDeathComplete = true;
                // ��� ó�� (������Ʈ ���� ��)
                
            }
        }
    }
}