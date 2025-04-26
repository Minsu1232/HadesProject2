using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<Achievement> achievements = new List<Achievement>();

    // �̺�Ʈ ����
    public delegate void AchievementCompletedHandler(Achievement achievement);
    public event AchievementCompletedHandler OnAchievementCompleted;

    // ���� ������ ���ΰ�ħ �̺�Ʈ
    public event System.Action OnAchievementDataRefreshed;

    // ���� ���� ID�� ���� ���� API �̸� ����
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

    // ���� �ʱ�ȭ
    private void InitializeAchievements()
    {
        // CSV���� ���� �⺻ ���� �ε�
        LoadAchievementsFromCSV();

        // ���� ������ ���൵ �ε�
        LoadAchievementProgress();

        // ���� ���� ����ȭ - ���ÿ��� �޼��� ������ �������� �ݿ�
        SyncAchievementsWithSteam();
    }

    private void HandleFirstFragmentFound()
    {
        // ���� ���� ���� ������Ʈ
        UpdateAchievement(4001, 1); // ù ���
        UpdateAchievement(4002, 1); // ��� ������

        // �ٸ� ������ �ʿ��� ��� ���⼭ ó��
        UpdateAchievement(3002, 1); // ù ��° �庮
        UpdateAchievement(2002, 1); // �һ��

        Debug.Log("ù ���� ȹ�� ���� ������ ������Ʈ�Ǿ����ϴ�.");
    }

    // CSV���� ���� ���� �ε�
    private void LoadAchievementsFromCSV()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Achievements.csv");

        if (!File.Exists(filePath))
        {
            Debug.LogError("���� CSV ������ ã�� �� �����ϴ�: " + filePath);
            return;
        }

        try
        {
            // ���� ���� ��� �ʱ�ȭ
            achievements.Clear();
            steamAchievementMapping.Clear();

            // CSV ���� �б�
            string[] lines = File.ReadAllLines(filePath);

            // ��� �����ϰ� ������ �Ľ�
            for (int i = 1; i < lines.Length; i++)
            {
                string[] values = lines[i].Split(',');

                if (values.Length < 9) // SteamAchievementID ���� �߰��Ǿ� �ּ� 9�� �ʵ� �ʿ�
                {
                    Debug.LogWarning($"CSV ���� {i + 1}�� �����Ͱ� �����մϴ�. �ǳʶݴϴ�.");
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

                    // ������ ��ȣ�� �и�
                    string[] itemIdSplit = rewardItemIdStr.Split('|');
                    string[] amountSplit = rewardAmountStr.Split('|');

                    // �� ���� ó��
                    int rewardItemId = int.Parse(itemIdSplit[0]);
                    int rewardAmount = int.Parse(amountSplit[0]);

                    // ���� ��ü ����
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

                    // �߰� ���� ó�� (�ִ� ���)
                    if (itemIdSplit.Length > 1)
                    {
                        for (int j = 1; j < itemIdSplit.Length; j++)
                        {
                            if (j < amountSplit.Length)
                            {
                                achievement.additionalRewards.Add(new RewardInfo(
                                    int.Parse(itemIdSplit[j]),
                                    int.Parse(amountSplit[j]),
                                    "" // ������ �������� ����
                                ));
                            }
                        }
                    }

                    achievements.Add(achievement);

                    // ���� ���� ID ���� �߰� (������� ���� ���)
                    if (!string.IsNullOrEmpty(steamAchievementId))
                    {
                        steamAchievementMapping[id] = steamAchievementId;
                        Debug.Log($"���� ���� ���� �߰�: ���� ID {id} -> ���� ID {steamAchievementId}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"CSV ���� {i + 1} �Ľ� �� ���� �߻�: {e.Message}");
                }
            }

            Debug.Log($"CSV���� {achievements.Count}���� ���� �ε� �Ϸ�");
            Debug.Log($"���� ���� ���� {steamAchievementMapping.Count}�� �ε� �Ϸ�");
        }
        catch (Exception e)
        {
            Debug.LogError($"CSV ���� �ε� �� ���� �߻�: {e.Message}");
        }
    }

    // ����� ���� ���൵ �ε�
    private void LoadAchievementProgress()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData saveData = SaveManager.Instance.GetPlayerData();

            // ����� ���൵�� ����Ʈ�� ������ ��ųʸ��� ��ȯ
            saveData.ConvertListToDictionary();

            // ����� ���� ���൵�� ������ �ε�
            if (saveData.achievementProgress != null)
            {
                foreach (var savedProgress in saveData.achievementProgress)
                {
                    // ID�� ���� ã��
                    Achievement achievement = GetAchievement(savedProgress.Key);
                    if (achievement != null)
                    {
                        // ���൵, �Ϸ� ����, ���� ���� ���� ������Ʈ
                        achievement.progressCurrent = savedProgress.Value.progress;
                        achievement.isCompleted = savedProgress.Value.isCompleted;
                        achievement.isRewardClaimed = savedProgress.Value.isRewardClaimed;
                    }
                }
            }

            // ���� �����͸� ������� ���� ���൵ ������Ʈ
            UpdateAchievementsFromSaveData(saveData);
        }
    }

    // ���� ������ ����ȭ (���� ������ ������ �ݿ�)
    private void SyncAchievementsWithSteam()
    {
        // ���� �Ŵ��� �ʱ�ȭ Ȯ��
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾҽ��ϴ�. ���� ����ȭ�� �ǳʶݴϴ�.");
            return;
        }

        // �Ϸ�� ������ ������ �˸�
        foreach (var achievement in achievements)
        {
            if (achievement.isCompleted)
            {
                // ���� ���� ID ã��
                if (steamAchievementMapping.TryGetValue(achievement.id, out string steamAchievementId))
                {
                    // ���� ���� ����
                    SteamworksManager.Instance.UnlockAchievement(steamAchievementId);
                    Debug.Log($"���� ���� ����ȭ: {achievement.name} (ID: {steamAchievementId})");
                }
            }
        }
    }

    // ���̺� ������ ��� ���� ������Ʈ
    private void UpdateAchievementsFromSaveData(PlayerSaveData saveData)
    {
        if (saveData == null) return;

        // ���� ���� ���� (ID: 2001)
        UpdateAchievement(2001, saveData.deathCount);

        // �湮�� ��� ���� ���� (ID: 5002)
        if (saveData.locationVisits != null)
        {
            UpdateAchievement(5002, saveData.locationVisits.Count);
        }
    }

    // ���� ���൵ ����
    private void SaveAchievementProgress()
    {
        if (SaveManager.Instance != null)
        {
            PlayerSaveData saveData = SaveManager.Instance.GetPlayerData();

            // ���� ���൵ ���� ���� �ʱ�ȭ
            if (saveData.achievementProgress == null)
                saveData.achievementProgress = new Dictionary<int, AchievementProgress>();

            // ��� ���� ���൵ ����
            foreach (var achievement in achievements)
            {
                saveData.achievementProgress[achievement.id] = new AchievementProgress(
                    achievement.progressCurrent,
                    achievement.isCompleted,
                    achievement.isRewardClaimed
                );
            }

            // ��ųʸ��� ����Ʈ�� ��ȯ (����ȭ�� ����)
            saveData.ConvertDictionaryToList();

            // ����� ������ ����
            SaveManager.Instance.SavePlayerData();
        }
    }

    // ID�� ���� ã��
    public Achievement GetAchievement(int achievementId)
    {
        return achievements.Find(a => a.id == achievementId);
    }

    // ī�װ��� ���� ��������
    public List<Achievement> GetAchievementsByCategory(AchievementCategory category)
    {
        return achievements.Where(a => a.category == category).ToList();
    }

    // ���� ���൵ ������Ʈ
    public void UpdateAchievement(int achievementId, int newProgress)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null)
        {
            bool wasCompleted = achievement.UpdateProgress(newProgress);

            // ���� �Ϸ� �̺�Ʈ �߻�
            if (wasCompleted)
            {
                AchievementCompleted(achievement);
            }

            // ���� ���� ����
            SaveAchievementProgress();
        }
    }

    // ���� ���൵ ����
    public void IncrementAchievement(int achievementId, int amount = 1)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null)
        {
            bool wasCompleted = achievement.IncrementProgress(amount);
            Debug.Log($"���� ID: {achievementId}, ���� ���൵: {achievement.progressCurrent}, �䱸ġ: {achievement.progressRequired}, �Ϸ��: {wasCompleted}");

            // ���� �Ϸ� �̺�Ʈ �߻� (�Ϸ�� ��쿡��)
            if (wasCompleted)
            {
                AchievementCompleted(achievement);
            }

            // ���� ���� ����
            SaveAchievementProgress();
        }
    }

    // ���� �޼� ó��
    private void AchievementCompleted(Achievement achievement)
    {
        Debug.Log($"���� �޼�: {achievement.name}");

        // �̺�Ʈ �߻�
        OnAchievementCompleted?.Invoke(achievement);

        // �˸� ǥ��
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowNotification($"���� �޼�: {achievement.name}", Color.yellow);
        }

        // ���� ���� ����
        UnlockSteamAchievement(achievement.id);
    }

    // ���� ���� ����
    private void UnlockSteamAchievement(int achievementId)
    {
        // ���� ���� Ȯ��
        if (SteamworksManager.Instance == null || !SteamworksManager.Instance.Initialized)
        {
            Debug.LogWarning("���� �Ŵ����� �ʱ�ȭ���� �ʾ� ���� ������ ������ �� �����ϴ�.");
            return;
        }

        // ���ε� ���� ���� API �̸� ã��
        if (steamAchievementMapping.TryGetValue(achievementId, out string steamAchievementId))
        {
            // ���� ���� ����
            bool success = SteamworksManager.Instance.UnlockAchievement(steamAchievementId);
            if (success)
            {
                Debug.Log($"���� ���� ���� ����: {steamAchievementId}");
            }
            else
            {
                Debug.LogWarning($"���� ���� ���� ����: {steamAchievementId}");
            }
        }
        else
        {
            Debug.LogWarning($"���� ���� ID {achievementId}�� �����ϴ� ���� ���� ID�� ã�� �� �����ϴ�.");
        }
    }

    // ���� ����
    public void ClaimReward(int achievementId)
    {
        Achievement achievement = GetAchievement(achievementId);
        if (achievement != null && achievement.isCompleted && !achievement.isRewardClaimed)
        {
            // ���� ����
            GrantReward(achievement);

            // ���� ���� ó��
            achievement.isRewardClaimed = true;

            // ����
            SaveAchievementProgress();

            // �˸�
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowNotification($"���� ����: {achievement.GetRewardDescription()}", Color.green);
            }
        }
    }

    // ���� ����
    private void GrantReward(Achievement achievement)
    {
        // �� ���� ����
        if (achievement.rewardItemId > 0 && InventorySystem.Instance != null)
        {
            InventorySystem.Instance.AddItem(achievement.rewardItemId, achievement.rewardAmount);
            Debug.Log($"�� ���� ����: ID {achievement.rewardItemId}, ���� {achievement.rewardAmount}");
        }

        // �߰� ���� ����
        foreach (var reward in achievement.additionalRewards)
        {
            if (reward.itemId > 0 && InventorySystem.Instance != null)
            {
                InventorySystem.Instance.AddItem(reward.itemId, reward.amount);
                Debug.Log($"�߰� ���� ����: ID {reward.itemId}, ���� {reward.amount}");
            }
        }
    }

    // �Ϸ�� ���� ���� ��������
    public int GetCompletedAchievementCount()
    {
        return achievements.Count(a => a.isCompleted);
    }

    // ��ü ���� ���� ��������
    public int GetTotalAchievementCount()
    {
        return achievements.Count;
    }

    // �Ϸ��� ���
    public float GetCompletionPercentage()
    {
        if (achievements.Count == 0) return 0f;
        return (float)GetCompletedAchievementCount() / GetTotalAchievementCount() * 100f;
    }

    // ���� ���� �� ������ ���ΰ�ħ
    public void RefreshAchievements()
    {
        // ���� ���� ���൵ �ʱ�ȭ
        foreach (var achievement in achievements)
        {
            achievement.progressCurrent = 0;
            achievement.isCompleted = false;
            achievement.isRewardClaimed = false;
        }

        // ���� ������ ������ �ε�
        LoadAchievementProgress();

        // UI ���� �˸�
        OnAchievementDataRefreshed?.Invoke();
    }
}