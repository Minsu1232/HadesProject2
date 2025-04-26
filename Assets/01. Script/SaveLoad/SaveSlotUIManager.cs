using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// 모드 enum 추가
public enum SlotMode
{
    Load,   // 불러오기 모드
    NewGame // 새 게임 시작 모드
}

public class SaveSlotUIManager : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;              // 슬롯 버튼
        public GameObject slotPanel;           // 슬롯 패널 (양피지 이미지)
        public TextMeshProUGUI slotNumberText; // 슬롯 번호 텍스트
        public TextMeshProUGUI chapterText;    // 챕터 텍스트
        public TextMeshProUGUI saveTimeText;   // 저장 시간 텍스트
        public TextMeshProUGUI playTimeText;   // 플레이 시간 텍스트
        public GameObject emptySlotText;       // 비어있는 슬롯 텍스트
        public TextMeshProUGUI buttonText;     // 슬롯 버튼 텍스트
    }

    // 모드 관련 필드 추가
    private SlotMode currentMode = SlotMode.Load;

    [Header("슬롯 UI 요소")]
    [SerializeField] private SlotUI[] slotUIs = new SlotUI[3];

    [Header("액션 버튼")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;

    [Header("확인 패널")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI confirmMessage;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    // 세이브 화면 전체 패널
    [SerializeField] private GameObject saveSlotPanel;

    // 씬 전환 설정
    [Header("씬 전환 설정")]
    [SerializeField] private string villageSceneName = "Village";

    // 현재 선택된 슬롯 인덱스
    private int selectedSlotIndex = -1;
    private bool isConfirmingDelete = false;

    // Start is called before the first frame update
    void Start()
    {
        // SaveManager 확인
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager Instance가 존재하지 않습니다. SaveManager를 씬에 추가하세요.");
            return;
        }

        // 슬롯 UI 배열 확인
        bool hasNullSlot = false;
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null || slotUIs[i].slotButton == null)
            {
                Debug.LogError($"SlotUI[{i}] 또는 버튼이 설정되지 않았습니다. Inspector에서 확인하세요.");
                hasNullSlot = true;
            }
        }

        if (hasNullSlot)
            return;

        // 슬롯 UI 초기화
        InitializeSlotUI();

        // 액션 버튼 초기화
        loadButton.onClick.AddListener(LoadSelectedSlot);
        deleteButton.onClick.AddListener(ShowDeleteConfirmation);
        backButton.onClick.AddListener(ClosePanel);

        // 확인 패널 버튼 초기화
        confirmYesButton.onClick.AddListener(ConfirmAction);
        confirmNoButton.onClick.AddListener(CancelConfirmation);

        // 초기 UI 업데이트
        UpdateSlotUI();
        UpdateButtonStates();

        // 확인 패널 숨김
        confirmPanel.SetActive(false);
    }

    // 패널 표시
    public void ShowPanel()
    {
        saveSlotPanel.SetActive(true);
        UpdateSlotUI();
    }

    // 패널 닫기
    public void ClosePanel()
    {
        saveSlotPanel.SetActive(false);
        selectedSlotIndex = -1;
    }

    // 모드 설정 메서드
    public void SetMode(SlotMode mode)
    {
        currentMode = mode;

        // 모드에 따라 UI 조정
        switch (currentMode)
        {
            case SlotMode.Load:
                loadButton.gameObject.SetActive(true);
                break;

            case SlotMode.NewGame:
                loadButton.gameObject.SetActive(false);
                break;
        }

        // 버튼 상태 업데이트
        UpdateButtonStates();
    }

    // 슬롯 UI 초기화
    private void InitializeSlotUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            int slotIndex = i; // closure를 위한 변수 복사

            // 슬롯 버튼 클릭 시 선택 처리
            slotUIs[i].slotButton.onClick.AddListener(() => SelectSlot(slotIndex));
        }
    }
   
    // 슬롯 UI 업데이트
    public void UpdateSlotUI()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager Instance가 존재하지 않습니다.");
            return;
        }

        // 스팀 클라우드 사용 시 메타데이터 재로드
        if (SaveManager.Instance.IsUsingSteamCloud())
        {
            SaveManager.Instance.ReloadSlotMetadata();
        }

        // 모든 슬롯 메타데이터 가져오기
        List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
        Debug.Log($"로드된 슬롯 메타데이터 수: {slotInfos.Count}");

        // 각 슬롯의 정보 출력
        for (int i = 0; i < slotInfos.Count; i++)
        {
            SlotMetadataInfo info = slotInfos[i];
            Debug.Log($"슬롯 {i}: 데이터 있음={info.hasData}, 챕터={info.chapterProgress}, 저장시간={info.lastSaveTime}, 플레이시간={info.chapterProgress}");
        }

        // 각 슬롯 UI 업데이트
        for (int i = 0; i < slotUIs.Length && i < slotInfos.Count; i++)
        {
            SlotMetadataInfo info = slotInfos[i];
            SlotUI ui = slotUIs[i];
            Debug.Log($"현재 슬롯{i} :  {info.chapterProgress}");
            // 슬롯 번호 설정 (1부터 시작)
            ui.slotNumberText.text = $"슬롯 {i + 1}";

            // 데이터가 있는지 여부에 따라 UI 조정
            if (info.hasData)
            {
                // 비어있음 텍스트 숨김
                ui.emptySlotText.SetActive(false);

                // 슬롯 데이터 표시
                ui.chapterText.gameObject.SetActive(true);
                ui.saveTimeText.gameObject.SetActive(true);
                ui.playTimeText.gameObject.SetActive(true);

                // 버튼 텍스트를 '선택'으로 설정
                ui.buttonText.text = "선택";

                // 챕터 이름 설정
                
                ui.chapterText.text = $"챕터: {GetChapterName(info.chapterProgress)}";

                // 저장 시간 표시
                ui.saveTimeText.text = info.lastSaveTime.ToString();

                // 플레이 시간 표시
                ui.playTimeText.text = $"플레이 시간:{SaveManager.Instance.GetFormattedPlayTimeForSlot(i)}";
            }
            else
            {
                // 비어있음 텍스트 표시
                ui.emptySlotText.SetActive(true);

                // 데이터 필드 숨김
                ui.chapterText.gameObject.SetActive(false);
                ui.saveTimeText.gameObject.SetActive(false);
                ui.playTimeText.gameObject.SetActive(false);

                // 버튼 텍스트를 '새 게임'으로 설정
                ui.buttonText.text = "새 게임";
            }

            // 선택된 슬롯 하이라이트 효과
            bool isSelected = (i == selectedSlotIndex);
            HighlightSelectedSlot(ui, isSelected);
        }
    }

    // 챕터 ID를 기반으로 챕터 이름 반환
    private string GetChapterName(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 1: return "야수";
            case 2: return "용족";
            case 3: return "죽음";
            case 4: return "심장";
            default: return $"챕터 {chapterIndex}";
        }
    }

    // 선택된 슬롯 하이라이트 효과
    private void HighlightSelectedSlot(SlotUI ui, bool isSelected)
    {
        // 선택된 슬롯 표시
        if (isSelected)
        {
            ui.slotPanel.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        }
        else
        {
            ui.slotPanel.transform.localScale = Vector3.one;
        }
    }

    // 버튼 상태 업데이트
    private void UpdateButtonStates()
    {
        bool hasSelectedSlot = selectedSlotIndex >= 0;
        bool hasSelectedData = false;

        if (hasSelectedSlot && SaveManager.Instance != null)
        {
            // 선택된 슬롯에 데이터가 있는지 확인
            List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
            if (selectedSlotIndex < slotInfos.Count)
            {
                hasSelectedData = slotInfos[selectedSlotIndex].hasData;
            }
        }

        // 현재 모드에 따라 버튼 상태 조정
        switch (currentMode)
        {
            case SlotMode.Load:
                // 로드 모드에서는 데이터가 있는 슬롯만 로드 가능
                loadButton.interactable = hasSelectedSlot && hasSelectedData;
                deleteButton.interactable = hasSelectedSlot && hasSelectedData;
                break;

            case SlotMode.NewGame:
                // 새 게임 모드에서는 모든 슬롯 선택 가능
                deleteButton.interactable = hasSelectedSlot && hasSelectedData;
                break;
        }
    }

    // 슬롯 선택
    private void SelectSlot(int slotIndex)
    {
        // 같은 슬롯 더블 클릭 시 바로 실행
        if (selectedSlotIndex == slotIndex)
        {
            if (SaveManager.Instance == null) return;

            List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
            bool hasData = slotInfos[slotIndex].hasData;
            Debug.Log("첫시작 다이얼로그");
            // GameStartManager 연동
            if (GameStartManager.Instance != null)
            {
                if (currentMode == SlotMode.Load && hasData)
                {
                    Debug.Log("첫시작 다이얼로그OFF");
                    GameStartManager.Instance.ProcessSlotSelection(slotIndex, false);
                    
                }
                else if (currentMode == SlotMode.NewGame)
                {
                    if (hasData)
                    {
                        // 데이터가 있는 경우 확인 대화상자
                        isConfirmingDelete = false;
                        confirmMessage.text = "이 슬롯에 새 게임을 시작하면 기존 데이터가 삭제됩니다. 계속하시겠습니까?";
                        confirmPanel.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("첫시작 다이얼로그ON");
                        GameStartManager.Instance.ProcessSlotSelection(slotIndex, true); 
                    }
                }
            }
            else
            {
                // 기존 로직 유지 (GameStartManager가 없을 경우)
                if (currentMode == SlotMode.Load && hasData)
                {
                    LoadSelectedSlot();
                }
                else if (currentMode == SlotMode.NewGame)
                {
                    if (hasData)
                    {
                        isConfirmingDelete = false;
                        confirmMessage.text = "이 슬롯에 새 게임을 시작하면 기존 데이터가 삭제됩니다. 계속하시겠습니까?";
                        confirmPanel.SetActive(true);
                    }
                    else
                    {
                        StartNewGameOnSelectedSlot();
                    }
                }
            }
            return;
        }

        // 이전 선택 해제
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Length)
        {
            HighlightSelectedSlot(slotUIs[selectedSlotIndex], false);
        }

        // 새 슬롯 선택
        selectedSlotIndex = slotIndex;
        HighlightSelectedSlot(slotUIs[selectedSlotIndex], true);

        // 버튼 상태 업데이트
        UpdateButtonStates();
    }

    // 선택된 슬롯 로드
    private void LoadSelectedSlot()
    {
        if (selectedSlotIndex >= 0 && SaveManager.Instance != null)
        {
            // 현재 슬롯을 선택된 슬롯으로 변경 (실제 데이터 로드 활성화)
            SaveManager.Instance.SetActiveSlot(selectedSlotIndex, true);

            // 로딩 화면 표시 (LoadingScreen 사용)
            if (LoadingScreen.Instance != null)
            {
                LoadingScreen.Instance.ShowLoading(villageSceneName, OnGameLoaded);
            }
            else
            {
                // LoadingScreen이 없으면 직접 씬 전환
                SceneManager.LoadScene(villageSceneName);
                OnGameLoaded();
            }
        }
    }

    // 게임 로드 완료 시 호출
    private void OnGameLoaded()
    {
        // 게임 데이터 적용
        Debug.Log("게임 로드 완료");
    }

    // 삭제 확인 표시
    private void ShowDeleteConfirmation()
    {
        if (selectedSlotIndex >= 0)
        {
            isConfirmingDelete = true;
            confirmMessage.text = "선택한 슬롯의 모든 데이터를 삭제하시겠습니까?";
            confirmPanel.SetActive(true);
        }
    }

    // 선택된 슬롯에 새 게임 시작
    private void StartNewGameOnSelectedSlot()
    {
        if (SaveManager.Instance == null) return;

        // 현재 슬롯을 선택된 슬롯으로 변경 (데이터 로드 비활성화)
        SaveManager.Instance.SetActiveSlot(selectedSlotIndex, false);

        // 필요하면 슬롯 데이터 삭제
        SaveManager.Instance.DeleteCurrentSlot();

        // 로딩 화면 표시
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(villageSceneName, OnNewGameStarted);
        }
        else
        {
            // LoadingScreen이 없으면 직접 씬 전환
            SceneManager.LoadScene(villageSceneName);
            OnNewGameStarted();
        }
    }

    // 새 게임 시작 완료 시 호출
    private void OnNewGameStarted()
    {
        // 게임 데이터 초기 저장
        SaveInitialGameData();
        Debug.Log("새 게임 시작 완료");
    }

    // 초기 게임 데이터 저장
    private void SaveInitialGameData()
    {
        // 초기 데이터 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }
    }

    // 확인 액션 처리
    private void ConfirmAction()
    {
        confirmPanel.SetActive(false);

        if (isConfirmingDelete)
        {
            // 슬롯 삭제
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetActiveSlot(selectedSlotIndex, false);
                SaveManager.Instance.DeleteCurrentSlot();
            }

            // UI 업데이트
            UpdateSlotUI();
            UpdateButtonStates();
        }
        else
        {
            // 새 게임 시작
            StartNewGameOnSelectedSlot();
        }
    }

    // 확인 취소
    private void CancelConfirmation()
    {
        confirmPanel.SetActive(false);
    }
}