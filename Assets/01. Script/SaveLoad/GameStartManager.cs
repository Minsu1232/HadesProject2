using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    [Header("�� ����")]
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

    // �� ���� ����
    public void StartNewGame()
    {
        // ���� ���� ���� �ʱ�ȭ
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
        }

        // ����
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }

        // ù é�� �ε�
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
        Debug.Log("�� ���� ���۵�");

        // ��Ʈ�� ���̾�α�
        Invoke("ShowIntroDialog", 0.5f);
    }

    private void ShowIntroDialog()
    {
        if (DialogSystem.Instance != null)
        {
            DialogSystem.Instance.StartDialog("game_intro");
        }
    }

    // ����� ���� �ε�
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
        Debug.Log("����� ���� �ε��");
    }

    // SaveSlotUIManager���� ȣ��
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