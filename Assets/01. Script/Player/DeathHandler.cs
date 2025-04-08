// DeathHandler.cs - ������ ���� Ŭ����
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public static class DeathHandler
{
    // ��� Ƚ���� ���� ���̾�α� ID
    private static readonly string[] DeathDialogIDs = {
        "first_death_tutorial",  // 1ȸ ���
        "second_death",          // 2ȸ ���
        "third_death",           // 3ȸ ���
        "many_deaths"            // 4ȸ �̻� ���
    };

    // �÷��̾� ��� �� ȣ��
    public static void HandlePlayerDeath()
    {
        // ��� Ƚ�� ����
        GameProgressManager.Instance.IncrementDeathCount();

        // ���� ��ȯ �غ�
        PlayerPrefs.SetFloat("VillageSpawnX", -6.6f);
        PlayerPrefs.SetFloat("VillageSpawnY", 0.1f);
        PlayerPrefs.SetFloat("VillageSpawnZ", -20f);
        PlayerPrefs.SetInt("ReturnFromDungeon", 1);

        // ���� ��� Ƚ�� ����
        int deathCount = GameProgressManager.Instance.GetDeathCount();

        // �ε� ȭ������ ���� ��ȯ
        if (LoadingScreen.Instance != null)
        {
            // onComplete �ݹ鿡 ���̾�α� ǥ�� �Լ� ����
            LoadingScreen.Instance.ShowLoading("Village", () => {
                // �ε� �Ϸ� �� ������
                if (GameInitializer.Instance != null)
                {
                    GameInitializer.Instance.StartCoroutine(DelayedShowDeathDialog(deathCount));
                }
            });
        }
        else
        {
            // �ε� ȭ�� ���� ���� ��ȯ
            SceneManager.LoadScene("Village");
        }
    }

    // ������ �� ���̾�α� ǥ��
    private static IEnumerator DelayedShowDeathDialog(int deathCount)
    {
        // �� ��ȯ �� �߰� ��� �ð�
        yield return new WaitForSeconds(0.5f);

        // ��� Ƚ���� ���� ���̾�α� ����
        int dialogIndex = Mathf.Min(deathCount - 1, DeathDialogIDs.Length - 1);

        if (dialogIndex >= 0 && DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(DeathDialogIDs[dialogIndex]);
        }
    }

}