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

        // 디졸브 컨트롤러 찾기
        dissolveController = transform.GetComponent<SimpleDissolveController>();
        if (dissolveController != null)
        {// 렌더러 재검색 (새로 생성된 아웃라인 포함)
            dissolveController.RefreshRenderers();
            // 디졸브 효과 시작 (오브젝트 제거는 여기서 하지 않음)
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

                // 사망 처리 (오브젝트 제거)
                if (dissolveController != null && dissolveController.gameObject != null)
                {
                    Object.Destroy(dissolveController.gameObject);
                }
            }
        }
    }
}