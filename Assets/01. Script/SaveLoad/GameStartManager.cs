using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    [Header("씬 설정")]
    [SerializeField] private string firstChapterScene = "Chapter1_Start";
    [SerializeField] private string villageScene = "Village";

    public static GameStartManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 새 게임 시작
    public void StartNewGame()
    {
        // 게임 진행 상태 초기화
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
        }

        // 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }

        // 첫 챕터 로드
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(villageScene, OnNewGameStarted);
        }
        else
        {
            SceneManager.LoadScene(villageScene);
            OnNewGameStarted();
        }
    }

    private void OnNewGameStarted()
    {
        Debug.Log("새 게임 시작됨");

        // 인트로 다이얼로그
        Invoke("ShowIntroDialog", 0.5f);
    }

    private void ShowIntroDialog()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog("game_intro");
        }
    }

    // 저장된 게임 로드
    public void LoadSavedGame()
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(villageScene, OnSavedGameLoaded);
        }
        else
        {
            SceneManager.LoadScene(villageScene);
            OnSavedGameLoaded();
        }
    }

    private void OnSavedGameLoaded()
    {
        Debug.Log("저장된 게임 로드됨");
    }

    // SaveSlotUIManager에서 호출
    public void ProcessSlotSelection(int slotIndex, bool isNewGame)
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetActiveSlot(slotIndex, !isNewGame);
        }

        if (isNewGame)
        {
            StartNewGame();
        }
        else
        {
            LoadSavedGame();
        }
    }
}