using System;
using System.Collections.Generic;
using UnityEngine;

// ���� ī�װ� enum
public enum AchievementCategory
{
    Combat,     // ���� ���� (1000-1999)
    Survival,   // ���� ���� (2000-2999)
    Progress,   // ���൵ ���� (3000-3999)
    Upgrade,    // ��ȭ/��� ���� (4000-4999)
    Discovery   // �߰� ���� (5000-5999)
}

// ���� Ŭ����
[System.Serializable]
public class Achievement
{
    public int id;                  // ���� ���� ID
    public string name;             // ���� �̸�
    public string description;      // ���� ����/����
    public AchievementCategory category; // ���� ī�װ�
    public int progressCurrent;     // ���� ���൵
    public int progressRequired;    // �޼� �ʿ� ���൵   
    public int rewardItemId;        // ���� ������ ID
    public int rewardAmount;        // ���� ����
    public bool isCompleted;        // �޼� ����
    public bool isRewardClaimed;    // ���� ���� ����
    public bool isHidden;           // ������ ���� ����
    public List<RewardInfo> additionalRewards = new List<RewardInfo>();
    // ���� ���൵ �ۼ�Ʈ ���
    public float GetProgressPercentage()
    {
        return Mathf.Clamp01((float)progressCurrent / progressRequired);
    }

    // ���� ���൵ ������Ʈ
    public bool UpdateProgress(int newProgress)
    {
        // �̹� �Ϸ�� �����̸� ������Ʈ���� ����
        if (isCompleted) return false;

        bool wasUpdated = newProgress > progressCurrent;
        if (wasUpdated)
        {
            progressCurrent = newProgress;

            // ���� �޼� Ȯ��
            if (progressCurrent >= progressRequired)
            {
                isCompleted = true;
                return true; // �Ϸ���� ��ȯ
            }
        }

        return false; // �Ϸ���� �ʾ����� �׻� false ��ȯ
    }

    // ���൵ ����
    public bool IncrementProgress(int amount = 1)
    {
        return UpdateProgress(progressCurrent + amount);
    }

    public string GetRewardDescription()
    {
        string description = "";

        // �� ���� ���� ����
        if (rewardItemId > 0)
        {
            // ������ ������ ��������
            var item = ItemDataManager.Instance.GetItem(rewardItemId);
            if (item != null)
            {
                description = $"{item.itemName} {rewardAmount}��";
            }
            else
            {
                description = $"������ ID {rewardItemId} {rewardAmount}��";
            }
        }

        // �߰� ������ �ִ� ��� ���� �߰�
        foreach (var reward in additionalRewards)
        {
            var item = ItemDataManager.Instance.GetItem(reward.itemId);
            if (item != null)
            {
                description += $" | {item.itemName} {reward.amount}��";
            }
            else
            {
                description += $" | ������ ID {reward.itemId} {reward.amount}��";
            }
        }

        return description;
    }
}
[System.Serializable]
public class RewardInfo
{
    public int itemId;
    public int amount;
    public string description;

    public RewardInfo(int itemId, int amount, string description)
    {
        this.itemId = itemId;
        this.amount = amount;
        this.description = description;
    }
}
// ����� ���� ���൵ Ŭ����
[System.Serializable]
public class AchievementProgress
{
    public int progress;
    public bool isCompleted;
    public bool isRewardClaimed;

    public AchievementProgress(int progress, bool isCompleted, bool isRewardClaimed)
    {
        this.progress = progress;
        this.isCompleted = isCompleted;
        this.isRewardClaimed = isRewardClaimed;
    }
}
