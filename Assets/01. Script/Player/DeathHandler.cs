// DeathHandler.cs - 간단한 정적 클래스
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;

public static class DeathHandler
{
    // 사망 횟수에 따른 다이얼로그 ID
    private static readonly string[] DeathDialogIDs = {
        "first_death_tutorial",  // 1회 사망
        "second_death",          // 2회 사망
        "third_death",           // 3회 사망
        "many_deaths"            // 4회 이상 사망
    };

    // 플레이어 사망 시 호출
    public static void HandlePlayerDeath()
    {
        // 사망 횟수 증가
        GameProgressManager.Instance.IncrementDeathCount();

        // 마을 귀환 준비
        PlayerPrefs.SetFloat("VillageSpawnX", -6.6f);
        PlayerPrefs.SetFloat("VillageSpawnY", 0.1f);
        PlayerPrefs.SetFloat("VillageSpawnZ", -20f);
        PlayerPrefs.SetInt("ReturnFromDungeon", 1);

        // 현재 사망 횟수 저장
        int deathCount = GameProgressManager.Instance.GetDeathCount();

        // 로딩 화면으로 마을 전환
        if (LoadingScreen.Instance != null)
        {
            // onComplete 콜백에 다이얼로그 표시 함수 전달
            LoadingScreen.Instance.ShowLoading("Village", () => {
                // 로딩 완료 후 딜레이
                if (GameInitializer.Instance != null)
                {
                    GameInitializer.Instance.StartCoroutine(DelayedShowDeathDialog(deathCount));
                }
            });
        }
        else
        {
            // 로딩 화면 없이 직접 전환
            SceneManager.LoadScene("Village");
        }
    }

    // 딜레이 후 다이얼로그 표시
    private static IEnumerator DelayedShowDeathDialog(int deathCount)
    {
        // 씬 전환 후 추가 대기 시간
        yield return new WaitForSeconds(0.5f);

        // 사망 횟수에 따라 다이얼로그 선택
        int dialogIndex = Mathf.Min(deathCount - 1, DeathDialogIDs.Length - 1);

        if (dialogIndex >= 0 && DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog(DeathDialogIDs[dialogIndex]);
        }
    }

}