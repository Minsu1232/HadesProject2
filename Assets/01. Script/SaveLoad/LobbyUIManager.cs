using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUIManager : MonoBehaviour
{
    [Header("메인 메뉴 버튼")]
    [SerializeField] private Button startButton;    // 시작하기 버튼
    [SerializeField] private Button loadButton;     // 불러오기 버튼
    [SerializeField] private Button optionsButton;  // 옵션 버튼
    [SerializeField] private Button quitButton;     // 종료 버튼 (필요시)

    [Header("패널")]
    [SerializeField] private GameObject saveSlotPanel; // 세이브 슬롯 UI 패널
    [SerializeField] private GameObject optionsPanel;  // 옵션 패널

    [Header("컴포넌트 참조")]
    [SerializeField] private SaveSlotUIManager saveSlotUIManager;

    private void Start()
    {
        // 버튼 이벤트 연결
        startButton.onClick.AddListener(ShowSaveSlotPanelForNewGame);
        loadButton.onClick.AddListener(ShowSaveSlotPanelForLoad);
        optionsButton.onClick.AddListener(ShowOptionsPanel);

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // 패널 초기 상태 설정
        saveSlotPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private void Update()
    {
        // ESC 키 처리
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 열려 있는 패널 닫기
            if (saveSlotPanel.activeSelf)
            {
                saveSlotPanel.SetActive(false);
            }
            else if (optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
            }
        }
    }

    // 새 게임용 세이브 슬롯 패널 표시
    private void ShowSaveSlotPanelForNewGame()
    {
        // 효과음 재생
        //AudioManager.Instance?.PlaySFX("button_click");

        // 슬롯 UI 매니저에 새 게임 모드 설정
        saveSlotUIManager.SetMode(SlotMode.NewGame);

        // 패널 표시
        ShowSaveSlotPanel();
    }

    // 불러오기용 세이브 슬롯 패널 표시
    private void ShowSaveSlotPanelForLoad()
    {
        // 효과음 재생
        //AudioManager.Instance?.PlaySFX("button_click");

        // 슬롯 UI 매니저에 로드 모드 설정
        saveSlotUIManager.SetMode(SlotMode.Load);

        // 패널 표시
        ShowSaveSlotPanel();
    }

    // 세이브 슬롯 패널 표시
    private void ShowSaveSlotPanel()
    {
        // 패널 활성화
        saveSlotPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // GameManager에 UI 상태 변경 알림 (GameManager가 있는 경우)
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj != null)
        {
            GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnUIStateChanged(saveSlotPanel, true);
            }
        }

        // 슬롯 UI 업데이트
        saveSlotUIManager.UpdateSlotUI();
    }

    // 옵션 패널 표시
    private void ShowOptionsPanel()
    {
        // 효과음 재생
        //AudioManager.Instance?.PlaySFX("button_click");

        // 패널 표시
        optionsPanel.SetActive(true);
        saveSlotPanel.SetActive(false);

        // GameManager에 UI 상태 변경 알림 (GameManager가 있는 경우)
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj != null)
        {
            GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnUIStateChanged(optionsPanel, true);
            }
        }
    }

    // 패널 닫기
    public void ClosePanel(GameObject panel)
    {
        // 효과음 재생
        //AudioManager.Instance?.PlaySFX("button_click");

        // 패널 비활성화
        panel.SetActive(false);

        // GameManager에 UI 상태 변경 알림 (GameManager가 있는 경우)
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj != null)
        {
            GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnUIStateChanged(panel, false);
            }
        }
    }

    // 게임 종료
    private void QuitGame()
    {
        // 효과음 재생
        //AudioManager.Instance?.PlaySFX("button_click");

        // 게임 종료 전 데이터 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }

        // 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}