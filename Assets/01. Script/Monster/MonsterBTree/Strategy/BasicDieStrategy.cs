using UnityEngine;

public class BasicDieStrategy : IDieStrategy
{
    private float deathDuration;
    private float deathTimer;
    private bool isDeathComplete;
    private SimpleDissolveController dissolveController;

    public bool IsDeathComplete => isDeathComplete;

    public void OnDie(Transform transform, IMonsterClass monsterData)
    {
        deathDuration = monsterData.CurrentDeathDuration;
        deathTimer = 0f;
        isDeathComplete = false;

        // ������ ��Ʈ�ѷ� ã��
        dissolveController = transform.GetComponent<SimpleDissolveController>();
        if (dissolveController != null)
        {// ������ ��˻� (���� ������ �ƿ����� ����)
            dissolveController.RefreshRenderers();
            // ������ ȿ�� ���� (������Ʈ ���Ŵ� ���⼭ ���� ����)
            dissolveController.destroyAfterDissolve = false;
            dissolveController.dissolveTime = deathDuration;
            dissolveController.OnMonsterDeath();
        }
    }

    public void UpdateDeath()
    {
        if (!isDeathComplete)
        {
            deathTimer += Time.deltaTime;
            if (deathTimer >= deathDuration)
            {
                isDeathComplete = true;

                // ��� ó�� (������Ʈ ����)
                if (dissolveController != null && dissolveController.gameObject != null)
                {
                    Object.Destroy(dissolveController.gameObject);
                }
            }
        }
    }
}