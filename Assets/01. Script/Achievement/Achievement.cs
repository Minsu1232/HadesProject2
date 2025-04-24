using System;
using System.Collections.Generic;
using UnityEngine;

// 업적 카테고리 enum
public enum AchievementCategory
{
    Combat,     // 전투 관련 (1000-1999)
    Survival,   // 생존 관련 (2000-2999)
    Progress,   // 진행도 관련 (3000-3999)
    Upgrade,    // 강화/기억 관련 (4000-4999)
    Discovery   // 발견 관련 (5000-5999)
}

// 업적 클래스
[System.Serializable]
public class Achievement
{
    public int id;                  // 업적 고유 ID
    public string name;             // 업적 이름
    public string description;      // 업적 설명/조건
    public AchievementCategory category; // 업적 카테고리
    public int progressCurrent;     // 현재 진행도
    public int progressRequired;    // 달성 필요 진행도   
    public int rewardItemId;        // 보상 아이템 ID
    public int rewardAmount;        // 보상 수량
    public bool isCompleted;        // 달성 여부
    public bool isRewardClaimed;    // 보상 수령 여부
    public bool isHidden;           // 숨겨진 업적 여부
    public List<RewardInfo> additionalRewards = new List<RewardInfo>();
    // 업적 진행도 퍼센트 계산
    public float GetProgressPercentage()
    {
        return Mathf.Clamp01((float)progressCurrent / progressRequired);
    }

    // 업적 진행도 업데이트
    public bool UpdateProgress(int newProgress)
    {
        // 이미 완료된 업적이면 업데이트하지 않음
        if (isCompleted) return false;

        bool wasUpdated = newProgress > progressCurrent;
        if (wasUpdated)
        {
            progressCurrent = newProgress;

            // 업적 달성 확인
            if (progressCurrent >= progressRequired)
            {
                isCompleted = true;
                return true; // 완료됨을 반환
            }
        }

        return false; // 완료되지 않았으면 항상 false 반환
    }

    // 진행도 증가
    public bool IncrementProgress(int amount = 1)
    {
        return UpdateProgress(progressCurrent + amount);
    }

    public string GetRewardDescription()
    {
        string description = "";

        // 주 보상 설명 생성
        if (rewardItemId > 0)
        {
            // 아이템 데이터 가져오기
            var item = ItemDataManager.Instance.GetItem(rewardItemId);
            if (item != null)
            {
                description = $"{item.itemName} {rewardAmount}개";
            }
            else
            {
                description = $"아이템 ID {rewardItemId} {rewardAmount}개";
            }
        }

        // 추가 보상이 있는 경우 설명 추가
        foreach (var reward in additionalRewards)
        {
            var item = ItemDataManager.Instance.GetItem(reward.itemId);
            if (item != null)
            {
                description += $" | {item.itemName} {reward.amount}개";
            }
            else
            {
                description += $" | 아이템 ID {reward.itemId} {reward.amount}개";
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
// 저장용 업적 진행도 클래스
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
