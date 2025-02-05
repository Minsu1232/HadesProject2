using UnityEngine;

public class HazardGimmickStrategy : IGimmickStrategy
{
    private readonly BossAI boss;          // ���� AI
    private readonly GimmickData data;          // ��� ������
    private readonly GameObject hazardPrefab; // ������ ���� ������Ʈ ������
    private readonly IGimmickReward gimmickReward; // ���� �ý���

    private float elapsedTime;                  // ���� �ð�
    private int hitCount;                       // ���� Ƚ��
    private bool isComplete;                    // �Ϸ� ����
    private float nextSpawnTime;                // ���� ���� �ð�
    private bool isInProgress = false;  // ��� ������ ���� �߰�

    public bool IsGimmickComplete => isComplete;

    public HazardGimmickStrategy(BossAI boss, GimmickData data, GameObject prefab)
    {
       this.boss = boss;
       this.data = data;
       this.hazardPrefab = prefab;
        this.gimmickReward = new HazardGimmickReward(boss.GetBossMonster());

    }
    public HazardGimmickStrategy()
    {

    }
    public void StartGimmick()
    {
        elapsedTime = 0f;
        hitCount = 0;
        isComplete = false;
        nextSpawnTime = 0f;
        isInProgress = true;

        // ��� ���� �� ���� ����
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(true);
        }

    }

    public void UpdateGimmick()
    {
        if (isComplete) return;

        elapsedTime += Time.deltaTime;

        // ���ѽð� üũ
        if (elapsedTime >= data.duration)
        {
            if (isInProgress)  // ���� �������̸� ����
            {
                FailGimmick();
            }
            return;
        }

        // ���� ���� üũ
        if (hitCount >= data.successCount)
        {
            SucceedGimmick();
            return;
        }

        // ������Ʈ ���� Ÿ�̹� üũ
        if (Time.time >= nextSpawnTime)
        {
            SpawnHazardObject();
            nextSpawnTime = Time.time + data.preparationTime;
        }
    }

    private void SpawnHazardObject()
    {
        Vector3 spawnPosition = GetHazardSpawnPosition();

        GameObject hazardGO = GameObject.Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
        HazardObject hazard = hazardGO.GetComponent<HazardObject>();

        hazard.Initialize(
            data.areaRadius,
            data.damage,
            data.moveSpeed,
            data.hazardSpawnType,
            data.targetType,
            GetSpawnHeight(),
            data.areaRadius,            
            this
        );

        hazard.StartMove();
    }


    private float GetSpawnHeight()
    {
        switch (data.hazardSpawnType)
        {
            case HazardSpawnType.AbovePlayer:
                return GameInitializer.Instance.GetPlayerClass().playerTransform.transform.position.y + 20f;
            case HazardSpawnType.AboveBoss:
                return boss.transform.position.y + 5f; // ���� ��ġ���� ����
            case HazardSpawnType.Random:
                return 5f; // �⺻ ����
            case HazardSpawnType.FixedPoints:
                return 4f; // ���� ��ġ�� ����
            default:
                return 3f;
        }
    }
    private Vector3 GetHazardSpawnPosition()
    {
        // ���� ��ġ ��� ����
        Vector3 position;

        switch (data.hazardSpawnType)
        {
            case HazardSpawnType.AbovePlayer:
                var playerPos = GameInitializer.Instance.GetPlayerClass().playerTransform.transform.position;
                position = new Vector3(playerPos.x, GetSpawnHeight(), playerPos.z);
                break;

            case HazardSpawnType.Random:
                float randomX = Random.Range(-data.areaRadius, data.areaRadius);
                float randomZ = Random.Range(-data.areaRadius, data.areaRadius);
                position = new Vector3(randomX, GetSpawnHeight(), randomZ);
                break;

            case HazardSpawnType.FixedPoints:
                // ���� ��ġ ����
                position = new Vector3(0, GetSpawnHeight(), 0);  // ����
                break;

            default:
                position = Vector3.zero;
                position.y = GetSpawnHeight();
                break;
        }

        return position;
    }
    private void SucceedGimmick()
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("����");

        // ���� ����
        gimmickReward.ApplySuccess(boss, GameInitializer.Instance.GetPlayerClass());
        // ���� ������ data.makeInvulnerable üũ �� ����
        if (data.makeInvulnerable)
        {   
            
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("����OFF");
        }
    }

    private void FailGimmick()
    {
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("����");
        // ���� ����
        // �г�Ƽ ����
        gimmickReward.ApplyFailure(boss, GameInitializer.Instance.GetPlayerClass());

        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("����OFF");
        }
    }
      public void SucessTrigget()
   {
       hitCount++;
       if (hitCount >= data.successCount)  // 3 ��� �������� successCount ���
       {
           SucceedGimmick();
       }
        Debug.Log("����Ƚ�� ::" + hitCount);
   }
    public string GetGimmickAnimationTrigger()
    {
        return "Roar";
    }

    public IGimmickStrategy GetGimmickStrategy()
    {
        throw new System.NotImplementedException();
    }
}