using UnityEngine;
using DG.Tweening;

public class SoulGimmickStrategy : IGimmickStrategy
{
    private readonly BossAI boss;
    private readonly GimmickData data;
    private readonly GameObject soulEntityPrefab;
    private readonly IGimmickReward gimmickReward;
    private readonly ISuccessUI successUI;
    private AudioClip roarSound;

    private float elapsedTime;
    private int brightSoulSuccessCount;
    private int darkSoulFailCount;
    private bool isComplete;
    private float nextSpawnTime;
    private bool isInProgress = false;

    private const int MAX_SOULS_AT_ONCE = 10; // 동시에 존재할 수 있는 최대 영혼 수
    private int currentSoulsCount = 0;


    private bool hasGimmickStarted = false;
    private bool isGimmickStartDelayInProgress = false;
    public bool IsGimmickComplete => isComplete;

    public SoulGimmickStrategy(BossAI boss, GimmickData data, GameObject prefab, ISuccessUI successUI, AudioClip roarSound = null)
    {
        this.boss = boss;
        this.data = data;
        this.soulEntityPrefab = prefab;
        this.gimmickReward = new SoulGimmickReward(boss.GetBossMonster());
        this.successUI = successUI;
        this.roarSound = roarSound;
    }

    public void StartGimmick()
    {
        isInProgress = false;



        elapsedTime = 0f;
        brightSoulSuccessCount = 0;
        darkSoulFailCount = 0;
        isComplete = false;
        nextSpawnTime = Time.time;
        currentSoulsCount = 0;
        isInProgress = true;

        // 보스 위치 설정
        if (data.useCustomPosition)
        {
            boss.transform.DOMove(data.gimmickPosition, 2f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
 {
     // 도착 시 트리거 호출
     boss.animator.SetTrigger("GimmickDestination");
     // 기타 도착 후 처리...
 });
        }

        // 보스 무적 설정
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(true);
        }

        // UI 초기화
        if (successUI != null)
        {
            successUI.InitializeSuccessUI(data.successCount);
            successUI.UpdateSuccessCount(0);
        } 
    }
        
    

    public void UpdateGimmick()
    {
        if (!isInProgress || isComplete)
            return;

        // 실제 기믹 시작 2초 지연
        if (!hasGimmickStarted && !isGimmickStartDelayInProgress)
        {
            isGimmickStartDelayInProgress = true;
            DOVirtual.DelayedCall(2f, () => {
                hasGimmickStarted = true;
                isGimmickStartDelayInProgress = false;
                // 여기서 기믹 시작 효과/사운드 추가 가능
            });
            return;
        }

        // 기믹이 아직 시작되지 않았으면 타이머 업데이트를 건너뜀
        if (!hasGimmickStarted)
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
        if (brightSoulSuccessCount >= data.successCount)
        {
            SucceedGimmick();
            return;
        }

        // 실패 조건 체크
        if (darkSoulFailCount >= data.successCount)
        {
            FailGimmick();
            return;
        }

        // 일정 간격으로 영혼 생성
        if (Time.time >= nextSpawnTime && currentSoulsCount < MAX_SOULS_AT_ONCE)
        {
            SpawnSoulEntity();
            nextSpawnTime = Time.time + data.preparationTime;
        }
    }

    private void SpawnSoulEntity()
    {
        // 영혼 생성 위치 계산
        Vector3 spawnPosition = GetSoulSpawnPosition();

        // 영혼 개체 생성
        GameObject soulGO = Object.Instantiate(soulEntityPrefab, spawnPosition, Quaternion.identity);
        SoulEntity soul = soulGO.GetComponent<SoulEntity>();

        if (soul != null)
        {
            soul.Initialize(
                data.areaRadius,
                data.damage,
                data.moveSpeed,
                HazardSpawnType.Random,
                TargetType.Boss,
                0.5f,
                data.areaRadius,
                this
            );

            soul.StartMove();
            currentSoulsCount++;
        }
    }

    private Vector3 GetSoulSpawnPosition()
    {
        // 원형 경계에서 랜덤한 위치 계산
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = data.areaRadius;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        return new Vector3(x, 0.5f, z) + boss.transform.position;
    }

    private void SucceedGimmick()
    {
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("기믹 성공");

        // 성공 보상 적용 - 리워드 클래스에서 모든 로직 처리
        gimmickReward.ApplySuccess(boss, player);

        // 무적 설정 해제
        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적 해제");
        }

        successUI.UIOff();

        // 남아있는 영혼 제거
        CleanupRemainingEntities();
    }

    private void FailGimmick()
    {
        isComplete = true;
        isInProgress = false;
        data.isEnabled = false;
        Debug.Log("기믹 실패");

        // 실패 페널티 적용 - 리워드 클래스에서 모든 로직 처리
        gimmickReward.ApplyFailure(boss, GameInitializer.Instance.GetPlayerClass());

        if (data.makeInvulnerable)
        {
            boss.GetBossMonster().SetInvulnerable(false);
            Debug.Log("무적 해제");
        }

        successUI.UIOff();

        // 남아있는 영혼 제거
        CleanupRemainingEntities();
    }

    private void CleanupRemainingEntities()
    {
        // 씬에 남아있는 영혼 개체 모두 제거
        SoulEntity[] entities = Object.FindObjectsOfType<SoulEntity>();
        foreach (var entity in entities)
        {
            Object.Destroy(entity.gameObject);
        }

        currentSoulsCount = 0;
    }

    // 밝은 영혼이 보스에 도달했을 때 호출
    public void SucessTrigget()
    {
        brightSoulSuccessCount++;
        currentSoulsCount--;
        Debug.Log("밝은 영혼 성공: " + brightSoulSuccessCount);

        if (successUI != null)
        {
            successUI.UpdateSuccessCount(brightSoulSuccessCount);
        }

        if (brightSoulSuccessCount >= data.successCount)
        {
            SucceedGimmick();
        }
    }

    // 어두운 영혼이 보스에 도달했을 때 호출
    public void DarkSoulReachedBoss()
    {
        darkSoulFailCount++;
        currentSoulsCount--;
        Debug.Log("어두운 영혼 실패: " + darkSoulFailCount);

        if (darkSoulFailCount >= data.successCount)
        {
            FailGimmick();
        }
    }

    public string GetGimmickAnimationTrigger()
    {
        return "Roar";
    }

    public AudioClip GetOptionalRoarSound()
    {
        return roarSound;
    }

    public IGimmickStrategy GetGimmickStrategy()
    {
        return this;
    }
}