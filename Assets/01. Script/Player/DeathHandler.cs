using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public static class DeathHandler
{
    // �÷��̾� ��� �� ȣ��
    public static void HandlePlayerDeath()
    {
        // ��� Ƚ�� ����
        GameProgressManager.Instance.IncrementDeathCount();

        AchievementManager.Instance.UpdateAchievement(2001,SaveManager.Instance.GetPlayerData().deathCount);
        AchievementManager.Instance.UpdateAchievement(5003, SaveManager.Instance.GetPlayerData().deathCount);
        // ���� ��ȯ �غ� (��ġ�� �÷��� ����)
        PlayerPrefs.SetFloat("VillageSpawnX", -6.6f);
        PlayerPrefs.SetFloat("VillageSpawnY", 0.1f);
        PlayerPrefs.SetFloat("VillageSpawnZ", -20f);
        PlayerPrefs.SetInt("ReturnFromDungeon", 1);

        // ������� ���� ��ȯ���� ǥ���ϴ� �÷��� �߰�
        PlayerPrefs.SetInt("ReturnByDeath", 1);

        // ��� ������ �� ������ ��ȯ
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