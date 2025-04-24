using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using Unity.VisualScripting;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }
    [Header("기본 설정")]
    [SerializeField] private bool usePortalSystem = true;

    [Header("몬스터 스폰 설정")]
    [SerializeField] private float spawnDelay = 0.3f;
    [SerializeField] private GameObject spawnEffectPrefab;
    [SerializeField] private float bossIntroDelay = 2f;

    [Header("포탈 설정")]
    [SerializeField] private StagePortal portalPrefab;
    [SerializeField] private GameObject prompt;
    [SerializeField] private float portalSpawnDelay = 2f;
    [SerializeField] private Vector3 portalSpawnOffset = new Vector3(0, 0.1f, 0);
    [SerializeField] private GameObject portalAppearEffectPrefab;

    [Header("전환 효과")]
    [SerializeField] private float transitionDelay = 0.5f;
    [SerializeField] private Color bossRoomColor = new Color(0.5f, 0, 0, 0.3f);
    public event System.Action<StageData> OnStageLoaded; // 챕터 보스방 도달 이벤트
    public static event System.Action OnStageCleared; // 스테이지 클리어 이벤트
    [SerializeField] private GameObject bossEssenceUI; // 챕터 보스방 EssenceUI

    [SerializeField] private Button returnToVillageButton; // 인스펙터에서 할당
    [SerializeField] private float autoReturnDelay = 20f; // 자동 귀환 시간 (20초)
    private int currentReturnCountdown;
    private DG.Tweening.Sequence returnCountdownSequence;


    [Header("패시브 어빌리티 설정")]
    [SerializeField] private AbilitySelectionPanel abilitySelectionPanel;

    private string currentStageID;
    private StageData currentStage;
    private List<ICreatureStatus> activeMonsters = new List<ICreatureStatus>();
    private bool isStageClear = false;
    private Vector3 stageCenter = Vector3.zero;
    private GameObject currentPortal;


    [SerializeField]
    private List<string> dungeonSceneNames = new List<string>
    {
        "Chapter1Dungeon", "Chapter2Dungeon", "Chapter3Dungeon", "Chapter4Dungeon"
    };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            // 기존 이벤트 리스너가 있다면 함께 유지
            if (DialogSystem.Instance != null)
            {
                DialogSystem.OnDialogEvent += HandleDialogEvents;
            }
            if (returnToVillageButton != null)
            {
                returnToVillageButton.onClick.AddListener(() => ReturnToVillage().ConfigureAwait(false));
            }
        }
        else
        {
            Destroy(gameObject);
        }
        portalPrefab.interactionPrompt = prompt;
    }
    private void OnDestroy()
    {       
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DialogSystem.OnDialogEvent -= HandleDialogEvents;
        returnCountdownSequence.Kill();
    }

    // 씬 로드 시 호출되는 이벤트 핸들러
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 등록된 던전 씬 확인
        if (dungeonSceneNames.Contains(scene.name))
        {   
            // 현재 챕터 ID 가져오기
            string chapterId = PlayerPrefs.GetString("CurrentChapterID", "");


            // 챕터 시도 횟수 직접 증가 (UpdateChapter 호출 없이)
            if (!string.IsNullOrEmpty(chapterId) && SaveManager.Instance != null)
            {
                ChapterProgressData chapterData = SaveManager.Instance.GetChapterData();
                if (chapterData != null)
                {
                    ChapterProgressData.ChapterData chapter = chapterData.GetChapterData(chapterId);
                    if (chapter != null)
                    {   
                        if(chapter.attemptCount == 0)
                        {
                            AchievementManager.Instance.UpdateAchievement(3001, 1);
                        }
                        // 시도 횟수만 증가
                        chapter.attemptCount++;
                        SaveManager.Instance.SaveChapterData();
                        Debug.Log($"챕터 {chapterId} 시도 횟수 증가: {chapter.attemptCount}회");
                    }
                }
            }

            // 스테이지 로드 로직
            if (DungeonDataManager.Instance.IsInitialized())
            {
                string stageID = PlayerPrefs.GetString("CurrentStageID", "1_1");

                Debug.Log($"씬 전환 후 스킬 구성 개수: {SkillConfigManager.Instance.GetAllSkillConfigs().Count}");

                // 특정 ID 확인
                var config = SkillConfigManager.Instance.GetSkillConfig(1);
                Debug.Log($"스킬 구성 ID 1: {(config != null ? "존재함" : "없음")}");

                // 던전 입장 시 패시브 어빌리티 매니저 초기화
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.InitializeDungeon();
                    Debug.Log("던전 입장: 패시브 어빌리티 매니저 초기화 완료");
                }
                else
                {
                    Debug.LogWarning("DungeonAbilityManager를 찾을 수 없습니다.");
                }

                LoadStage(stageID, false);
            }
            else
            {
                Debug.LogError("DungeonDataManager가 초기화되지 않았습니다.");
            }
        }
    }

    // 스테이지 로드 (페이드 효과 옵션)
    public void LoadStage(string stageID, bool withFadeEffect = true)
    {
        if (withFadeEffect && SceneTransitionManager.Instance != null)
        {
            // 보스 스테이지인지 확인
            StageData stageData = DungeonDataManager.Instance.GetStageData(stageID);
            bool isBossStage = stageData != null && stageData.isBossStage;

            // 페이드 효과 색상 설정
            Color fadeColor = isBossStage ? bossRoomColor : Color.black;

            // 페이드 인 효과
            SceneTransitionManager.Instance.FadeIn(() => {
                InternalLoadStage(stageID);

                // 스테이지 로드 후 페이드 아웃
                DOVirtual.DelayedCall(transitionDelay, () => {
                    SceneTransitionManager.Instance.FadeOut();
                });
            }, fadeColor);
        }
        else
        {
            // 페이드 없이 바로 로드
            InternalLoadStage(stageID);
        }
    }

    // 내부 스테이지 로드 로직
    private void InternalLoadStage(string stageID)
    {
        // 기존 몬스터 정리
        ClearActiveMonsters();

        // 포탈 제거
        if (currentPortal != null)
        {
            Destroy(currentPortal);
            currentPortal = null;
        }

        isStageClear = false;

        // 스테이지 데이터 가져오기
        currentStageID = stageID;
        currentStage = DungeonDataManager.Instance.GetStageData(stageID);

        if (currentStage == null)
        {
            Debug.LogError($"스테이지 데이터를 찾을 수 없음: {stageID}");
            return;
        }
        // 보스 스테이지인지 확인 및 UI 처리
        HandleBossStageUI(currentStage);
        // 플레이어 위치 설정
        GameObject player = GameInitializer.Instance.gameObject;
        if (player != null && currentStage.playerSpawnPosition != Vector3.zero)
        {

            // 대체 이동 방식
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                player.transform.position = currentStage.playerSpawnPosition;
                DOVirtual.DelayedCall(0.2f, () => {
                    rb.isKinematic = false;
                });
            }
            else
            {
                player.transform.position = currentStage.playerSpawnPosition;
            }

        }

        // 몬스터 스폰
        StartCoroutine(SpawnMonstersWithDelay());

        Debug.Log($"스테이지 로드 완료: {currentStage.stageName} (ID: {stageID})");

        // 스테이지 로드 완료 이벤트 발생
        OnStageLoaded?.Invoke(currentStage);
    }

    private void HandleBossStageUI(StageData stageData)
    {
        if (stageData.isBossStage)
        {
            if (bossEssenceUI != null)
            {
                // UI 활성화
                bossEssenceUI.SetActive(true);
                Debug.Log("보스 에센스 UI 활성화");
            }
            else
            {
                Debug.LogWarning("보스 에센스 UI를 찾을 수 없습니다.");
            }
        }
        else
        {
            // 일반 스테이지인 경우 UI 비활성화

            if (bossEssenceUI != null)
            {
                bossEssenceUI.SetActive(false);
            }
        }
    }

    // 몬스터 스폰 (딜레이로 순차적 등장)
    private IEnumerator SpawnMonstersWithDelay()
    {
        // 스테이지 중심점 계산 (포탈 스폰 위치용)
        CalculateStageCenter();

        // 보스 스테이지인 경우 연출 추가
        if (currentStage.isBossStage)
        {
            // 일정 시간 대기 (카메라 팬닝 등의 연출 가능)
            yield return new WaitForSeconds(bossIntroDelay);

            // 보스 입장 효과 (SceneTransitionManager의 FlashEffect 활용)
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FlashEffect(Color.red, 0.5f);
            }
        }

        // 고정 위치 몬스터 스폰
        foreach (var spawn in currentStage.fixedSpawns)
        {
            // 보스/미드보스는 약간의 대기 후 등장
            if (spawn.monsterID >= 1000 || (currentStage.isMidBossStage && spawn.isFixedPosition))
            {
                yield return new WaitForSeconds(0.5f);
            }

            MonsterFactoryBase factory = GetFactoryForMonsterId(spawn.monsterID);

            // 스폰 이펙트
            if (spawnEffectPrefab != null)
            {
                Instantiate(spawnEffectPrefab, spawn.position, Quaternion.identity);
            }

            // 잠시 딜레이 후 몬스터 스폰
            yield return new WaitForSeconds(spawnDelay);
            factory?.CreateMonster(spawn.position, OnMonsterCreated);
        }

        // 랜덤 위치 몬스터 스폰
        if (currentStage.randomMonsters.Count > 0 && currentStage.spawnPoints.Count > 0)
        {
            int count = Mathf.Min(currentStage.totalRandomSpawns, currentStage.spawnPoints.Count);
            List<Vector3> availablePoints = new List<Vector3>(currentStage.spawnPoints);

            for (int i = 0; i < count; i++)
            {
                if (availablePoints.Count == 0) break;

                // 랜덤 위치 선택
                int pointIndex = Random.Range(0, availablePoints.Count);
                Vector3 position = availablePoints[pointIndex];
                availablePoints.RemoveAt(pointIndex);

                // 가중치 기반 몬스터 선택
                MonsterSpawnInfo monster = SelectRandomMonster();
                if (monster == null) continue;

                // 스폰 이펙트
                if (spawnEffectPrefab != null)
                {
                    Instantiate(spawnEffectPrefab, position, Quaternion.identity);
                }

                // 딜레이 후 몬스터 스폰
                yield return new WaitForSeconds(spawnDelay);
                MonsterFactoryBase factory = GetFactoryForMonsterId(monster.monsterID);
                factory?.CreateMonster(position, OnMonsterCreated);
            }
        }
    }

    // 스테이지 중심점 계산 (포탈 용)
    private void CalculateStageCenter()
    {
        //포탈 위치가 지정되어 있으면 그 위치 사용
        if (currentStage.portalSpawnPosition != Vector3.zero)
        {
            stageCenter = currentStage.portalSpawnPosition;
            return;
        }

        // 아니면 자동 계산
        if (currentStage.fixedSpawns.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var spawn in currentStage.fixedSpawns)
            {
                sum += spawn.position;
            }
            stageCenter = sum / currentStage.fixedSpawns.Count;
        }
        else if (currentStage.spawnPoints.Count > 0)
        {
            Vector3 sum = Vector3.zero;
            foreach (var point in currentStage.spawnPoints)
            {
                sum += point;
            }
            stageCenter = sum / currentStage.spawnPoints.Count;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("PlayerObj");
            if (player != null)
            {
                stageCenter = player.transform.position;
            }
        }
    }

    // 가중치 기반 랜덤 몬스터 선택
    private MonsterSpawnInfo SelectRandomMonster()
    {
        if (currentStage.randomMonsters.Count == 0) return null;

        float totalWeight = 0f;
        foreach (var monster in currentStage.randomMonsters)
        {
            totalWeight += monster.spawnWeight;
        }

        float random = Random.Range(0f, totalWeight);
        float current = 0f;

        foreach (var monster in currentStage.randomMonsters)
        {
            current += monster.spawnWeight;
            if (random <= current)
            {
                return monster;
            }
        }

        return currentStage.randomMonsters[0];
    }

    // 몬스터 생성 콜백
    private void OnMonsterCreated(ICreatureStatus monster)
    {
        if (monster != null)
        {
            activeMonsters.Add(monster);
            Debug.Log($"몬스터 스폰됨: {monster.GetType().Name}");
        }
    }

    // 몬스터 ID에 따른 팩토리 반환
    private MonsterFactoryBase GetFactoryForMonsterId(int monsterId)
    {
        // 보스 몬스터 (ID가 1000 이상)
        if (monsterId >= 1000)
        {
            switch (monsterId)
            {
                case 1001:
                    return new CrabBossFactory();
                case 1002:
                    return new AlexanderBossFactory();
                default:
                    Debug.LogError($"알 수 없는 보스 ID: {monsterId}");
                    return new DummyMonsterFactory();
            }
        }

        // 일반 몬스터
        switch (monsterId)
        {
            case 2:
                return new DummyMonsterFactory();
            case 3:
                return new SPiderMonsterFactory();
            case 4:
                return new SlimeMonsterFactory();
            case 5:
                return new TurtleMonsterFactory();
            case 6:
                return new RayMonsterFactory();
            case 7:
                return new WarmMonsterFactory();
            case 8:
                return new RatMonsterFactory();
            default:
                Debug.LogWarning($"알 수 없는 몬스터 ID: {monsterId}, 기본 몬스터로 대체합니다.");
                return new DummyMonsterFactory();
        }
    }

    // 스테이지 클리어 확인
    public void CheckStageClear()
    {
        // 이미 클리어한 경우 스킵
        if (isStageClear) return;

        // 전체 몬스터 체크
        bool allDefeated = true;
        foreach (var monster in activeMonsters)
        {
            if (monster != null && monster.GetMonsterClass().IsAlive)
            {
                allDefeated = false;
                break;
            }
        }

        // 모든 몬스터가 처치됐다면 클리어 처리
        if (allDefeated && activeMonsters.Count > 0)
        {
            OnStageClear();
        }
    }

    // 스테이지 클리어 처리
    // 스테이지 클리어 처리
    private void OnStageClear()
    {
        Debug.Log($"스테이지 클리어: {currentStage.stageName}");
        isStageClear = true;
        OnStageCleared?.Invoke();
        // 현재 스테이지의 챕터 ID 가져오기
        string chapterId = PlayerPrefs.GetString("CurrentChapterID", "");

        // 현재 스테이지 기록 생성 (예: "1-5")
        string stageRecord = FormatStageRecord(currentStage.stageID);

        // 기록 업데이트 (SaveManager 사용)
        if (SaveManager.Instance != null && !string.IsNullOrEmpty(chapterId))
        {
            // completed 파라미터는 보스 스테이지인 경우만 true (다음 챕터 해금용)
            SaveManager.Instance.UpdateChapterProgress(chapterId, currentStage.isBossStage, stageRecord);
            Debug.Log($"챕터 {chapterId} 기록 업데이트: {stageRecord}");
        }

        if (currentStage.isBossStage)
        {
            Debug.Log("챕터 클리어! 보스 처치 다이얼로그를 시작합니다.");

            // 보스 처치 연출 효과
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.ScreenColorEffect(Color.white, 0.3f, 1.5f);
            }

            // 보스 타입에 따른 다이얼로그 ID 결정
            string bossDialogID = GetBossDefeatDialogID();

            if (!string.IsNullOrEmpty(bossDialogID) && DialogSystem.Instance != null)
            {
                // 이미 표시된 다이얼로그인지 확인
                bool alreadyShown = false;
                if (GameProgressManager.Instance != null)
                {
                    alreadyShown = GameProgressManager.Instance.IsDialogShown(bossDialogID);
                }

                if (!alreadyShown)
                {
                    // DialogSystem에 이벤트 리스너 추가 - 다이얼로그 완료 시 마을로 귀환
                    DialogSystem.OnDialogEvent += OnBossDefeatDialogComplete;

                    // 보스 처치 다이얼로그 시작
                    DialogSystem.Instance.StartDialog(bossDialogID);

                    // 다이얼로그 표시 마킹
                    if (GameProgressManager.Instance != null)
                    {
                        GameProgressManager.Instance.MarkDialogAsShown(bossDialogID);
                    }

                    // 보스 처치 플래그 설정 (예: "boss_defeated_chapter1")
                    string bossDefeatFlag = $"boss_defeated_chapter{currentStage.chapterID}";
                    if (GameProgressManager.Instance != null)
                    {
                        GameProgressManager.Instance.SetFlag(bossDefeatFlag, true);
                        Debug.Log($"보스 처치 플래그 설정: {bossDefeatFlag}");
                    }

                    return; // 다이얼로그 이벤트에서 마을로 귀환 처리
                }
            }

            StartReturnCountdown();
        }
        else
        {
            // 패시브 어빌리티 선택 UI 표시
            ShowAbilitySelection();
        }
    }

    // 스테이지 ID를 기록 형식으로 변환 (예: "1_5" -> "1-5")
    private string FormatStageRecord(string stageId)
    {
        if (string.IsNullOrEmpty(stageId) || !stageId.Contains("_"))
            return "1-1"; // 기본값

        string[] parts = stageId.Split('_');
        if (parts.Length >= 2)
        {
            return $"{parts[0]}-{parts[1]}"; // "챕터-스테이지" 형식
        }

        return "1-1"; // 기본값
    }
    private void StartReturnCountdown()
    {
        // 이미 실행 중인 시퀀스 정지
        if (returnCountdownSequence != null)
        {
            returnCountdownSequence.Kill();
        }

        // 초기값 설정
        currentReturnCountdown = Mathf.FloorToInt(autoReturnDelay);

        // 초기 알림
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification($"{currentReturnCountdown}초 후 마을로 귀환합니다", Color.white);
        }

        // DOTween 시퀀스 생성
        returnCountdownSequence = DOTween.Sequence();

        for (int i = 1; i <= currentReturnCountdown; i++)
        {
            int capturedI = i; // 클로저 문제 방지를 위한 값 캡처
            returnCountdownSequence.AppendCallback(() => {
                // 매 초마다 카운트다운 감소
                currentReturnCountdown--;

                // 5초 간격으로 알림 (20, 15, 10, 5초)
                if (currentReturnCountdown % 5 == 0 && currentReturnCountdown > 0)
                {
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowNotification($"{currentReturnCountdown}초 후 마을로 귀환합니다", Color.white);
                    }
                }
            }).AppendInterval(1f); // 1초 딜레이
        }

        // 카운트다운 완료 후 마을로 귀환
        returnCountdownSequence.OnComplete(() => {
            ReturnToVillage().ConfigureAwait(false);
        });

        // 시퀀스 실행
        returnCountdownSequence.Play();
    }
    private void HandleDialogEvents(string eventName)
    {
        // 무기 해금 이벤트 처리
        if (eventName == "UnlockChronofactuerWeapon")
        {
            // 플래그 설정 (마을에서 확인용)
            if (GameProgressManager.Instance != null)
            {
                GameProgressManager.Instance.SetFlag("weapon_unlocked_chronofactuer", true);
                Debug.Log("무기 해금 플래그 설정: weapon_unlocked_chronofactuer");
            }

            // 즉시 알림 표시
            if (UIManager.Instance != null)
            {
                Color goldColor = new Color(1f, 0.84f, 0f); // 금색
                UIManager.Instance.ShowNotification("새로운 무기 해금: 시간 절단자", goldColor);
                Debug.Log("넌왜두번되니??");
            }
        }
    }
    private string GetBossDefeatDialogID()
    {
        // 챕터 번호나 보스 ID로 적절한 다이얼로그 ID 반환
        int chapterNumber = currentStage.chapterID;

        switch (chapterNumber)
        {
            case 1:
                return "alexander_defeated";
            case 2:
                return "sirius_defeated";  // 추후 확장용
                                           // 추가 챕터용 case 추가 가능
            default:
                return "";
        }
    }
    // 다이얼로그 완료 이벤트 핸들러
    private void OnBossDefeatDialogComplete(string eventName)
    {
        // "DialogComplete:보스ID_defeated" 형식의 이벤트 확인
        if (eventName.StartsWith("DialogComplete:") &&
            eventName.Contains("_defeated"))
        {
            // 이벤트 리스너 제거
            DialogSystem.OnDialogEvent -= OnBossDefeatDialogComplete;

            StartReturnCountdown();
        }
    }

    // 챕터 번호 추출 메서드

    // 추가 보상 확률 체크 및 생성
    private void TryGenerateExtraReward()
    {
        // 추가 보상 확률 (기본값 0, 풍요의 모래시계 효과로 증가)
        float extraRewardChance = 0f;

        // 풍요의 모래시계 효과 적용
        if (TemporalDeviceManager.Instance != null)
        {
            TemporalDevice hourglassDevice = TemporalDeviceManager.Instance.GetDevice(2); // ID 2: 풍요의 모래시계
            if (hourglassDevice != null && hourglassDevice.IsUnlocked)
            {
                extraRewardChance = hourglassDevice.EffectValue; // CSV에 설정된 값 (0.1 = 10%)
                Debug.Log($"풍요의 모래시계 효과 적용: 추가 보상 확률 {extraRewardChance * 100}%");
            }
        }

        // 확률 체크
        if (Random.value < extraRewardChance)
        {
            Debug.Log("풍요의 모래시계 효과 발동: 추가 시간 조각 획득!");

            // 추가 시간 조각 생성
            int crystalAmount = Random.Range(5, 11); // 5-10개
            InventorySystem.Instance.AddItem(3001, crystalAmount);
            Debug.Log($"추가 보상: 시간 조각 {crystalAmount}개");

            // 효과 알림 (선택사항)
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"풍요의 모래시계 효과: 시간 조각 {crystalAmount}개 획득!");
            }
             
        }
    }
    // 패시브 어빌리티 선택 UI 표시 메서드 추가
    private void ShowAbilitySelection()
    {
        
        // AbilitySelectionPanel 컴포넌트를 찾거나 생성
        if (abilitySelectionPanel == null)
        {
            abilitySelectionPanel = FindObjectOfType<AbilitySelectionPanel>();
        }

        if (abilitySelectionPanel == null)
        {
            Debug.LogError("AbilitySelectionPanel을 찾을 수 없습니다.");

            // 포탈 바로 생성 (폴백 메커니즘)
            DOVirtual.DelayedCall(portalSpawnDelay, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
            return;
        }

        // 던전 어빌리티 매니저에서 선택 가능한 능력 목록 가져오기
        List<DungeonAbility> abilitySelection = DungeonAbilityManager.Instance.GetSmartAbilitySelection();

        if (abilitySelection.Count == 0)
        {
            Debug.LogWarning("선택 가능한 패시브 능력이 없습니다. 패널을 건너뛰고 포탈을 생성합니다.");

            // 포탈 바로 생성
            DOVirtual.DelayedCall(portalSpawnDelay, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
            return;
        }

        // 선택 완료 콜백 정의 (포탈 생성)
        System.Action onSelectionCompleted = () => {
            DOVirtual.DelayedCall(1f, () => {
                CheckItemsAndSpawnPortal(currentStage.nextStageID);
            });
        };

        // 패널 제목 설정 및 표시
        abilitySelectionPanel.SetTitle("패시브 능력 선택");
        abilitySelectionPanel.ShowSelectionPanel(abilitySelection, onSelectionCompleted);
    }

    // 아이템 확인 후 포탈 생성
    private void CheckItemsAndSpawnPortal(string nextStageID)
    {
        // 현재 아이템 레이어를 가진 오브젝트를 찾음
        Collider[] itemColliders = Physics.OverlapSphere(stageCenter, 20f, LayerMask.GetMask("Item"));

        if (itemColliders.Length == 0)
        {
            // 아이템이 없으면 바로 포탈 생성
            SpawnPortal(nextStageID);
        }
        else
        {
            Debug.Log($"아직 획득하지 않은 아이템이 {itemColliders.Length}개 있습니다. 포탈 생성을 대기합니다.");

            // 3초 후 다시 확인
            DOVirtual.DelayedCall(3f, () => {
                CheckItemsAndSpawnPortal(nextStageID);
            });
        }
    }

    // 포탈 생성
    private void SpawnPortal(string nextStageID)
    {
        TryGenerateExtraReward();
        if (currentPortal != null)
        {
            Destroy(currentPortal);
        }

        // 포탈 위치 계산
        Vector3 portalPosition = stageCenter + portalSpawnOffset;

        // 포탈 생성 이펙트
        if (portalAppearEffectPrefab != null)
        {
            Instantiate(portalAppearEffectPrefab, portalPosition, Quaternion.identity);
        }

        // 딜레이 후 포탈 생성
        DOVirtual.DelayedCall(0.5f, () => {
            // 포탈 프리팹 생성
            currentPortal = Instantiate(portalPrefab.gameObject, portalPosition, Quaternion.identity);

            // 크기 애니메이션
            currentPortal.transform.localScale = Vector3.zero;
            currentPortal.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            // StagePortal 컴포넌트 설정 (DungeonPortal -> StagePortal로 변경)
            StagePortal portalComponent = currentPortal.GetComponent<StagePortal>();
            if (portalComponent != null)
            {
                portalComponent.targetStageID = nextStageID;
            }

            // 회전 애니메이션
            StartCoroutine(RotatePortal(currentPortal.transform));
        });
    }

    // 포탈 회전 애니메이션
    private IEnumerator RotatePortal(Transform portalTransform)
    {
        float rotationSpeed = 30f;

        while (portalTransform != null)
        {
            portalTransform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }
    }

    // 빌리지로 귀환
    private async Task ReturnToVillage()
    {
        Debug.Log("빌리지로 귀환합니다.");
        // 빌리지 스폰 위치 설정 (고정 좌표 사용)
        PlayerPrefs.SetFloat("VillageSpawnX", -6.6f);
        PlayerPrefs.SetFloat("VillageSpawnY", 0.1f);
        PlayerPrefs.SetFloat("VillageSpawnZ", -20f);
        PlayerPrefs.SetInt("ReturnFromDungeon", 1); // 던전에서 돌아왔음을 표시

        // 사망으로 인한 귀환이 아님을 명시적으로 설정
        PlayerPrefs.SetInt("ReturnByDeath", 0);
        // 로딩 화면 표시 (있을 경우) - 페이드 효과 내장
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading("Village");
        }
        else
        {
            // 로딩 화면이 없는 경우만 SceneTransitionManager 사용
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeIn();
                await Task.Delay(500);
            }

            var operation = SceneManager.LoadSceneAsync("Village");
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.FadeOut();
            }
        }
    }

    // 활성화된 몬스터 정리
    private void ClearActiveMonsters()
    {
        // 몬스터와 연결된 게임 오브젝트 목록
        List<GameObject> monsterObjects = new List<GameObject>();

        // 모든 MonsterStatus 찾기
        MonsterStatus[] monsterStatuses = FindObjectsOfType<MonsterStatus>();
        foreach (var status in monsterStatuses)
        {
            monsterObjects.Add(status.gameObject);
        }

        // 모든 BossStatus 찾기
        BossStatus[] bossStatuses = FindObjectsOfType<BossStatus>();
        foreach (var status in bossStatuses)
        {
            monsterObjects.Add(status.gameObject);
        }

        // 모든 몬스터 게임 오브젝트 제거
        foreach (var obj in monsterObjects)
        {
            Destroy(obj);
        }

        activeMonsters.Clear();
    }
    // DungeonManager 클래스에 추가
    public List<ICreatureStatus> GetActiveMonsters()
    {
        // 살아있는 몬스터만 반환하거나, 전체 리스트 반환
        return new List<ICreatureStatus>(activeMonsters);
    }
    // 몬스터 처치 이벤트 처리
    public void OnMonsterDefeated(ICreatureStatus monster)
    {
        if (monster != null && activeMonsters.Contains(monster))
        {
            Debug.Log($"몬스터 처치됨: {monster.GetType().Name}");

            // 보스 처치 시 특별 처리
            if (monster is BossMonster || monster is AlexanderBoss)
            {
                Debug.Log("보스 처치! 스테이지 클리어 보상 지급");
                // 보스 처치 보상 로직
            }

            // 스테이지 클리어 체크
            CheckStageClear();
        }
    }
    public float GetDungeonProgress()
    {
        // 현재 스테이지 ID에서 번호 추출 (예: "1-5"에서 5 추출)
        if (currentStage != null)
        {
            int stageNumber = ExtractStageNumber(currentStage.stageID);
            int maxStageNumber = 10; // 각 챕터당 최대 스테이지 수
            float progress = Mathf.Clamp01((float)stageNumber / maxStageNumber);
            Debug.Log($"던전 진행도 계산: 스테이지 ID={currentStage.stageID}, 스테이지 번호={stageNumber}, 진행도={progress:F2}");
            return progress;
        }
        Debug.LogWarning("던전 진행도 계산 실패: currentStage가 null");
        return 0f;
    }

    public int GetCurrentStageNumber()
    {
        // 현재 스테이지 ID에서 번호만 추출 (예: "1-5"에서 5 추출)
        if (currentStage != null)
        {
            return ExtractStageNumber(currentStage.stageID);
        }
        return 1; // 기본값
    }

    private int ExtractStageNumber(string stageID)
    {
        // 스테이지 ID 형식: "1-5" (챕터-스테이지)
        if (!string.IsNullOrEmpty(stageID) && stageID.Contains("_"))
        {
            string[] parts = stageID.Split('_');
            if (parts.Length >= 2 && int.TryParse(parts[1], out int stageNumber))
            {
                Debug.Log($"스테이지 번호 추출: {stageID} -> {stageNumber}");
                return stageNumber;
            }
        }
        Debug.LogWarning($"스테이지 번호 추출 실패: {stageID}, 기본값 1 사용");
        return 1; // 파싱 실패 시 기본값
    }
    
}