using UnityEngine;

public class HazardGimmickStrategy : IGimmickStrategy
{
    private readonly BossAI boss;          // 보스 AI
    private readonly GimmickData data;          // 기믹 데이터
    private readonly GameObject hazardPrefab; // 생성할 위험 오브젝트 프리팹
    private readonly IGimmickReward gimmickReward; // 보상 시스템

    private float elapsedTime;                  // 진행 시간
    private int hitCount;                       // 명중 횟수
    private bool isComplete;                    // 완료 여부
    private float nextSpawnTime;                // 다음 생성 시간
    private bool isInProgress = false;  // 기믹 진행중 상태 추가

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

        // 기믹 시작 시 무적 설정
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(true);
        }

    }

    public void UpdateGimmick()
    {
        if (isComplete) return;

        elapsedTime += Time.deltaTime;

        // 제한시간 체크
        if (elapsedTime >= data.duration)
        {
            if (isInProgress)  // 아직 진행중이면 실패
            {
                FailGimmick();
            }
            return;
        }

        // 성공 조건 체크
        if (hitCount >= data.successCount)
        {
            SucceedGimmick();
            return;
        }

        // 오브젝트 생성 타이밍 체크
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
                return boss.transform.position.y + 5f; // 보스 위치보다 높게
            case HazardSpawnType.Random:
                return 5f; // 기본 높이
            case HazardSpawnType.FixedPoints:
                return 4f; // 고정 위치용 높이
            default:
                return 3f;
        }
    }
    private Vector3 GetHazardSpawnPosition()
    {
        // 스폰 위치 계산 로직
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
                // 고정 위치 로직
                position = new Vector3(0, GetSpawnHeight(), 0);  // 예시
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
        Debug.Log("성공");

        // 보상 적용
        gimmickReward.ApplySuccess(boss, GameInitializer.Instance.GetPlayerClass());
        // 무적 해제는 data.makeInvulnerable 체크 후 실행
        if (data.makeInvulnerable)
        {   
            
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적OFF");
        }
    }

    private void FailGimmick()
    {
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("실패");
        // 무적 해제
        // 패널티 적용
        gimmickReward.ApplyFailure(boss, GameInitializer.Instance.GetPlayerClass());

        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적OFF");
        }
    }
      public void SucessTrigget()
   {
       hitCount++;
       if (hitCount >= data.successCount)  // 3 대신 데이터의 successCount 사용
       {
           SucceedGimmick();
       }
        Debug.Log("성공횟수 ::" + hitCount);
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