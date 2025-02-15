using UnityEngine;
using DG.Tweening; // DOTween 네임스페이스 추가

public class HazardGimmickStrategy : IGimmickStrategy
{
    private readonly BossAI boss;             // 보스 AI
    private readonly GimmickData data;          // 기믹 데이터
    private readonly GameObject hazardPrefab;   // 생성할 위험 오브젝트 프리팹
    private readonly IGimmickReward gimmickReward; // 보상 시스템
    private AudioClip roarSound;                // 기믹 전략에서 사용할 사운드 (선택적)
    private readonly ISuccessUI successUI;      // 성공 UI 모듈 (ISuccessUI 인터페이스)

    private float elapsedTime;  // 기믹 진행 시간
    private int hitCount;       // 성공 횟수
    private bool isComplete;    // 기믹 완료 여부
    private float nextSpawnTime;// 다음 오브젝트 생성 시간
    private bool isInProgress = false; // 기믹 진행 중 여부

    public bool IsGimmickComplete => isComplete;

    // 생성자: SuccessUI 모듈을 외부에서 주입하여 사용
    public HazardGimmickStrategy(BossAI boss, GimmickData data, GameObject prefab, ISuccessUI successUI, AudioClip roarSound = null)
    {
        this.boss = boss;
        this.data = data;
        this.hazardPrefab = prefab;
        this.gimmickReward = new HazardGimmickReward(boss.GetBossMonster());
        this.roarSound = roarSound;
        this.successUI = successUI;
    }

    public HazardGimmickStrategy() { }

    public AudioClip GetOptionalRoarSound()
    {
        return roarSound;
    }

    public void StartGimmick()
    {
        //// 카메라 쉐이크 효과 실행 (예: 1초 동안 흔들림)
        //HitStopManager.TriggerHitStop(1f, 1.5f);
        isInProgress = false;

        // HitStop 종료 후 1초 딜레이 후 기믹 초기화
        DOVirtual.DelayedCall(1f, () =>
        {
            elapsedTime = 0f;
            hitCount = 0;
            isComplete = false;
            nextSpawnTime = Time.time;
            isInProgress = true;

            // 기믹 시작 시 무적 설정 (옵션)
            if (data.makeInvulnerable)
            {
                boss.GetBossMonster().SetInvulnerable(true);
            }

            // 기믹 시작 시 SuccessUI 초기화:
            // 최대 성공 횟수(data.successCount)를 기준으로 동적으로 아이콘 생성
            if (successUI != null)
            {
                successUI.InitializeSuccessUI(data.successCount);
                successUI.UpdateSuccessCount(0);
            }
        });
    }

    public void UpdateGimmick()
    {
        if (!isInProgress || isComplete)
            return;

        elapsedTime += Time.deltaTime;

        // 남은 시간의 비율 계산 (1.0 -> 0.0)
        float remainingTimeRatio = 1f - (elapsedTime / data.duration);

        // UI 업데이트
        if (successUI != null)
        {
            successUI.UpdateTimeBar(remainingTimeRatio);
        }

        // 기믹 제한 시간 초과 시 실패 처리
        if (elapsedTime >= data.duration)
        {
            if (isInProgress)
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

        // 일정 간격으로 위험 오브젝트 생성
        if (Time.time >= nextSpawnTime)
        {
            SpawnHazardObject();
            nextSpawnTime = Time.time + data.preparationTime;
        }
    }

    private void SpawnHazardObject()
    {
        Vector3 spawnPosition = GetHazardSpawnPosition();
        GameObject hazardGO = Object.Instantiate(hazardPrefab, spawnPosition, Quaternion.identity);
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
                return boss.transform.position.y + 5f;
            case HazardSpawnType.Random:
                return 5f;
            case HazardSpawnType.FixedPoints:
                return 4f;
            default:
                return 3f;
        }
    }

    private Vector3 GetHazardSpawnPosition()
    {
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
                position = new Vector3(0, GetSpawnHeight(), 0);
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
        Debug.Log("기믹 성공");

        // 성공 보상 적용
        gimmickReward.ApplySuccess(boss, player);

        // 기믹 시작 시 무적 설정을 해제
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적 해제");
        }
        successUI.UIOff();
    }

    private void FailGimmick()
    {
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("기믹 실패");

        // 실패 패널티 적용
        gimmickReward.ApplyFailure(boss, GameInitializer.Instance.GetPlayerClass());

        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적 해제");
        }
        successUI.UIOff();
    }

    // 성공 이벤트 발생 시 호출: 기믹 내 성공 횟수를 증가시키고 UI 업데이트
    public void SucessTrigget()
    {
        hitCount++;
        Debug.Log("성공 횟수: " + hitCount);
        if (successUI != null)
        {
            successUI.UpdateSuccessCount(hitCount);
        }
        if (hitCount >= data.successCount)
        {
            SucceedGimmick();
        }
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
