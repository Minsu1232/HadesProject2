using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public static class DeathHandler
{
    // 플레이어 사망 시 호출
    public static void HandlePlayerDeath()
    {
        // 사망 횟수 증가
        GameProgressManager.Instance.IncrementDeathCount();

        AchievementManager.Instance.UpdateAchievement(2001,SaveManager.Instance.GetPlayerData().deathCount);
        AchievementManager.Instance.UpdateAchievement(5003, SaveManager.Instance.GetPlayerData().deathCount);
        // 마을 귀환 준비 (위치와 플래그 설정)
        PlayerPrefs.SetFloat("VillageSpawnX", -6.6f);
        PlayerPrefs.SetFloat("VillageSpawnY", 0.1f);
        PlayerPrefs.SetFloat("VillageSpawnZ", -20f);
        PlayerPrefs.SetInt("ReturnFromDungeon", 1);

        // 사망으로 인한 귀환임을 표시하는 플래그 추가
        PlayerPrefs.SetInt("ReturnByDeath", 1);

        // 잠시 딜레이 후 마을로 전환
        DOVirtual.DelayedCall(0.5f, () => {
            if (LoadingScreen.Instance != null)
            {
                LoadingScreen.Instance.ShowLoading("Village", null);
            }
            else
            {
                SceneManager.LoadScene("Village");
            }
        });
    }
}