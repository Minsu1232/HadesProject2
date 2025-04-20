using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LobbyUIManager : MonoBehaviour
{
    [Header("메인 메뉴 버튼")]
    [SerializeField] private Button startButton;    // 시작하기 버튼
    [SerializeField] private Button loadButton;     // 불러오기 버튼
    [SerializeField] private Button optionsButton;  // 옵션 버튼
    [SerializeField] private Button quitButton;     // 종료 버튼 (필요시)

    [Header("버튼 호버 효과 설정")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;  // 호버 시 버튼 크기 배율
    [SerializeField] private float hoverAnimationSpeed = 5f;     // 호버 애니메이션 속도

    [Header("패널")]
    [SerializeField] private GameObject saveSlotPanel; // 세이브 슬롯 UI 패널
    [SerializeField] private GameObject optionsPanel;  // 옵션 패널

    [Header("컴포넌트 참조")]
    [SerializeField] private SaveSlotUIManager saveSlotUIManager;

    // 원래 크기를 저장할 Dictionary
    private System.Collections.Generic.Dictionary<Button, Vector3> originalScales = new System.Collections.Generic.Dictionary<Button, Vector3>();
    // 현재 호버 중인 버튼
    private Button currentHoveredButton = null;

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

        // 버튼 호버 이벤트 설정
        SetupButtonHoverEffects();
    }

    private void SetupButtonHoverEffects()
    {
        // 모든 버튼에 호버 효과 추가
        SetupButtonHoverEffect(startButton);
        SetupButtonHoverEffect(loadButton);
        SetupButtonHoverEffect(optionsButton);

        if (quitButton != null)
        {
            SetupButtonHoverEffect(quitButton);
        }
    }

    private void SetupButtonHoverEffect(Button button)
    {
        if (button == null) return;

        // 원래 크기 저장
        originalScales[button] = button.transform.localScale;

        // 이벤트 트리거 컴포넌트 가져오기 또는 추가
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 포인터 진입 이벤트 설정
        EventTrigger.Entry entryEvent = new EventTrigger.Entry();
        entryEvent.eventID = EventTriggerType.PointerEnter;
        entryEvent.callback.AddListener((data) => { OnPointerEnter(button); });
        trigger.triggers.Add(entryEvent);

        // 포인터 퇴출 이벤트 설정
        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { OnPointerExit(button); });
        trigger.triggers.Add(exitEvent);
    }

    private void OnPointerEnter(Button button)
    {
        // 현재 호버 중인 버튼 설정
        currentHoveredButton = button;
    }

    private void OnPointerExit(Button button)
    {
        // 호버 효과 제거
        if (currentHoveredButton == button)
        {
            currentHoveredButton = null;
        }
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

        // 호버 애니메이션 업데이트
        UpdateHoverEffects();
    }

    private void UpdateHoverEffects()
    {
        // 모든 버튼 확인
        UpdateButtonScale(startButton);
        UpdateButtonScale(loadButton);
        UpdateButtonScale(optionsButton);

        if (quitButton != null)
        {
            UpdateButtonScale(quitButton);
        }
    }

    private void UpdateButtonScale(Button button)
    {
        if (button == null || !originalScales.ContainsKey(button)) return;

        Vector3 targetScale;

        // 호버 중인 버튼이면 크기 확대
        if (button == currentHoveredButton)
        {
            targetScale = originalScales[button] * hoverScaleMultiplier;
        }
        else
        {
            targetScale = originalScales[button];
        }

        // 부드러운 애니메이션으로 크기 변경
        button.transform.localScale = Vector3.Lerp(
            button.transform.localScale,
            targetScale,
            Time.deltaTime * hoverAnimationSpeed
        );
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