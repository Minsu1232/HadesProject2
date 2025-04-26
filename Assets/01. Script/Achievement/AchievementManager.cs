using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<Achievement> achievements = new List<Achievement>();

    // 이벤트 정의
    public delegate void AchievementCompletedHandler(Achievement achievement);
    public event AchievementCompletedHandler OnAchievementCompleted;

    // 업적 데이터 새로고침 이벤트
    public event System.Action OnAchievementDataRefreshed;

    // 게임 업적 ID와 스팀 업적 API 이름 매핑
    private Dictionary<int, string> steamAchievementMapping = new Dictionary<int, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAchievements();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (InventorySystem.Instance != null)
        {
            InventorySystem.Instance.OnFirstFragmentFound += HandleFirstFragmentFound;
        }
    }

    // 업적 초기화
    private void InitializeAchievements()
    {
        // CSV에서 업적 기본 정보 로드
        LoadAchievementsFromCSV();

        // 현재 슬롯의 진행도 로드
        LoadAchievementProgress();

        // 스팀 업적 동기화 - 로컬에서 달성한 업적을 스팀에도 반영
        SyncAchievementsWithSteam();
    }

    private void HandleFirstFragmentFound()
    {
        // 파편 관련 업적 업데이트
        UpdateAchievement(4001, 1); // 첫 기억
        UpdateAchievement(4002, 1); // 기억 수집가

        // 다른 업적도 필요한 경우 여기서 처리
        UpdateAchievement(3002, 1); // 첫 번째 장벽
        UpdateAchievement(2002, 1); // 불사신

        Debug.Log("첫 파편 획득 관련 업적이 업데이트되었습니다.");
    }

    // CSV에서 업적 정보 로드
    private void LoadAchievementsFromCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Achievements.csv");

        if (!File.Exists(filePath))
        {
            Debug.LogError("업적 CSV 파일을 찾을 수 없습니다: " + filePath);
            return;
        }

        try
        {
            // 기존 업적 목록 초기화
            achievements.Clear();
            steamAchievementMapping.Clear();

            // CSV 파일 읽기
            string[] lines = File.ReadAllLines(filePath);

            // 헤더 제외하고 데이터 파싱
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                if (values.Length < 9) // SteamAchievementID 열이 추가되어 최소 9개 필드 필요
                {
                    Debug.LogWarning($"CSV 라인 {i + 1}에 데이터가 부족합니다. 건너뜁니다.");
                    continue;
                }

                try
                {
                    int id = int.Parse(values[0]);
                    string name = values[1];
                    string description = values[2];
                    AchievementCategory category = (AchievementCategory)int.Parse(values[3]);
                    int progressRequired = int.Parse(values[4]);
                    string rewardItemIdStr = values[5];
                    string rewardAmountStr = values[6];
                    bool isHidden = bool.Parse(values[7]);
                    string steamAchievementId = values[8].Trim();

                    // 파이프 기호로 분리
                    string[] itemIdSplit = rewardItemIdStr.Split('|');
                    string[] amountSplit = rewardAmountStr.Split('|');

                    // 주 보상 처리
                    int rewardItemId = int.Parse(itemIdSplit[0]);
                    int rewardAmount = int.Parse(amountSplit[0]);

                    // 업적 객체 생성
                    Achievement achievement = new Achievement
                    {
                        id = id,
                        name = name,
                        description = description,
                        category = category,
                        progressCurrent = 0,
                        progressRequired = progressRequired,
                        rewardItemId = rewardItemId,
                        rewardAmount = rewardAmount,
                        isCompleted = false,
                        isRewardClaimed = false,
                        isHidden = isHidden,
                        additionalRewards = new List<RewardInfo>()
                    };

                    // 추가 보상 처리 (있는 경우)
                    if (itemIdSplit.Length > 1)
                    {
                        for (int j = 1; j < itemIdSplit.Length; j++)
                        {
                            if (j < amountSplit.Length)
                            {
                                achievement.additionalRewards.Add(new RewardInfo(
                                    int.Parse(itemIdSplit[j]),
                                    int.Parse(amountSplit[j]),
                                    "" // 설명은 동적으로 생성
                                ));
                            }
                        }
                    }

                    achievements.Add(achievement);

                    // 스팀 업적 ID 매핑 추가 (비어있지 않은 경우)
                    if (!string.IsNullOrEmpty(steamAchievementId))
                    {
                        steamAchievementMapping[id] = steamAchievementId;
                        Debug.Log($"스팀 업적 매핑 추가: 게임 ID {id} -> 스팀 ID {steamAchievementId}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"CSV 라인 {i + 1} 파싱 중 오류 발생: {e.Message}");
                }
            }

            Debug.Log($"CSV에서 {achievements.Count}개의 업적 로드 완료");
            Debug.Log($"스팀 업적 매핑 {steamAchievementMapping.Count}개 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV 파일 로드 중 오류 발생: {e.Message}");
        }
    }

    // 저장된 업적 진행도 로드
    private void LoadAchievementProgress()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData saveData = SaveManager.Instance.GetPlayerData();

            // 저장된 진행도가 리스트에 있으면 딕셔너리로 변환
            saveData.ConvertListToDictionary();

            // 저장된 업적 진행도가 있으면 로드
            if (saveData.achievementProgress != null)
            {
                foreach (var savedProgress in saveData.achievementProgress)
                {
                    // ID로 업적 찾기
                    Achievement achievement = GetAchievement(savedProgress.Key);
                    if (achievement != null)
                    {
                        // 진행도, 완료 여부, 보상 수령 여부 업데이트
                        achievement.progressCurrent = savedProgress.Value.progress;
                        achievement.isCompleted = savedProgress.Value.isCompleted;
                        achievement.isRewardClaimed = savedProgress.Value.isRewardClaimed;
                    }
                }
            }

            // 기존 데이터를 기반으로 업적 진행도 업데이트
            UpdateAchievementsFromSaveData(saveData);
        }
    }

    // 스팀 업적과 동기화 (로컬 업적을 스팀에 반영)
    private void SyncAchievementsWithSteam()
    {
        // 스팀 매니저 초기화 확인
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("스팀 매니저가 초기화되지 않았습니다. 업적 동기화를 건너뜁니다.");
            return;
        }

        // 완료된 업적을 스팀에 알림
        foreach (var achievement in achievements)
        {
            if (achievement.isCompleted)
            {
                // 스팀 업적 ID 찾기
                if (steamAchievementMapping.TryGetValue(achievement.id, out string steamAchievementId))
                {
                    // 스팀 업적 해제
                    SteamworksManager.Instance.UnlockAchievement(steamAchievementId);
                    Debug.Log($"스팀 업적 동기화: {achievement.name} (ID: {steamAchievementId})");
                }
            }
        }
    }

    // 세이브 데이터 기반 업적 업데이트
    private void UpdateAchievementsFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null) return;

        // 죽음 관련 업적 (ID: 2001)
        UpdateAchievement(2001, saveData.deathCount);

        // 방문한 장소 관련 업적 (ID: 5002)
        if (saveData.locationVisits != null)
        {
            UpdateAchievement(5002, saveData.locationVisits.Count);
        }
    }

    // 업적 진행도 저장
    private void SaveAchievementProgress()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData saveData = SaveManager.Instance.GetPlayerData();

            // 업적 진행도 저장 구조 초기화
            if (saveData.achievementProgress == null)
                saveData.achievementProgress = new Dictionary<int, AchievementProgress>();

            // 모든 업적 진행도 저장
            foreach (var achievement in achievements)
            {
                saveData.achievementProgress[achievement.id] = new AchievementProgress(
                    achievement.progressCurrent,
                    achievement.isCompleted,
                    achievement.isRewardClaimed
                );
            }

            // 딕셔너리를 리스트로 변환 (직렬화를 위해)
            saveData.ConvertDictionaryToList();

            // 변경된 데이터 저장
            SaveManager.Instance.SavePlayerData();
        }
    }

    // ID로 업적 찾기
    public Achievement GetAchievement(int achievementId)
    {
        return achievements.Find(a => a.id == achievementId);
    }

    // 카테고리별 업적 가져오기
    public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
    {
        return achievements.Where(a => a.category == category).ToList();
    }

    // 업적 진행도 업데이트
    public void UpdateAchievement(int achievementId, int newProgress)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null)
        {
            bool wasCompleted = achievement.UpdateProgress(newProgress);

            // 업적 완료 이벤트 발생
            if (wasCompleted)
            {
                AchievementCompleted(achievement);
            }

            // 변경 사항 저장
            SaveAchievementProgress();
        }
    }

    // 업적 진행도 증가
    public void IncrementAchievement(int achievementId, int amount = 1)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null)
        {
            bool wasCompleted = achievement.IncrementProgress(amount);
            Debug.Log($"업적 ID: {achievementId}, 현재 진행도: {achievement.progressCurrent}, 요구치: {achievement.progressRequired}, 완료됨: {wasCompleted}");

            // 업적 완료 이벤트 발생 (완료된 경우에만)
            if (wasCompleted)
            {
                AchievementCompleted(achievement);
            }

            // 변경 사항 저장
            SaveAchievementProgress();
        }
    }

    // 업적 달성 처리
    private void AchievementCompleted(Achievement achievement)
    {
        Debug.Log($"업적 달성: {achievement.name}");

        // 이벤트 발생
        OnAchievementCompleted?.Invoke(achievement);

        // 알림 표시
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification($"업적 달성: {achievement.name}", Color.yellow);
        }

        // 스팀 업적 해제
        UnlockSteamAchievement(achievement.id);
    }

    // 스팀 업적 해제
    private void UnlockSteamAchievement(int achievementId)
    {
        // 스팀 연동 확인
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("스팀 매니저가 초기화되지 않아 스팀 업적을 해제할 수 없습니다.");
            return;
        }

        // 매핑된 스팀 업적 API 이름 찾기
        if (steamAchievementMapping.TryGetValue(achievementId, out string steamAchievementId))
        {
            // 스팀 업적 해제
            bool success = SteamworksManager.Instance.UnlockAchievement(steamAchievementId);
            if (success)
            {
                Debug.Log($"스팀 업적 해제 성공: {steamAchievementId}");
            }
            else
            {
                Debug.LogWarning($"스팀 업적 해제 실패: {steamAchievementId}");
            }
        }
        else
        {
            Debug.LogWarning($"게임 업적 ID {achievementId}에 대응하는 스팀 업적 ID를 찾을 수 없습니다.");
        }
    }

    // 보상 수령
    public void ClaimReward(int achievementId)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.isCompleted && !achievement.isRewardClaimed)
        {
            // 보상 지급
            GrantReward(achievement);

            // 보상 수령 처리
            achievement.isRewardClaimed = true;

            // 저장
            SaveAchievementProgress();

            // 알림
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"보상 수령: {achievement.GetRewardDescription()}", Color.green);
            }
        }
    }

    // 보상 지급
    private void GrantReward(Achievement achievement)
    {
        // 주 보상 지급
        if (achievement.rewardItemId > 0 && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(achievement.rewardItemId, achievement.rewardAmount);
            Debug.Log($"주 보상 지급: ID {achievement.rewardItemId}, 수량 {achievement.rewardAmount}");
        }

        // 추가 보상 지급
        foreach (var reward in achievement.additionalRewards)
        {
            if (reward.itemId > 0 && InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(reward.itemId, reward.amount);
                Debug.Log($"추가 보상 지급: ID {reward.itemId}, 수량 {reward.amount}");
            }
        }
    }

    // 완료된 업적 개수 가져오기
    public int GetCompletedAchievementCount()
    {
        return achievements.Count(a => a.isCompleted);
    }

    // 전체 업적 개수 가져오기
    public int GetTotalAchievementCount()
    {
        return achievements.Count;
    }

    // 완료율 계산
    public float GetCompletionPercentage()
    {
        if (achievements.Count == 0) return 0f;
        return (float)GetCompletedAchievementCount() / GetTotalAchievementCount() * 100f;
    }

    // 슬롯 변경 시 데이터 새로고침
    public void RefreshAchievements()
    {
        // 기존 업적 진행도 초기화
        foreach (var achievement in achievements)
        {
            achievement.progressCurrent = 0;
            achievement.isCompleted = false;
            achievement.isRewardClaimed = false;
        }

        // 현재 슬롯의 데이터 로드
        LoadAchievementProgress();

        // UI 갱신 알림
        OnAchievementDataRefreshed?.Invoke();
    }
}