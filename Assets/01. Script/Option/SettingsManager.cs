using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SettingsManager Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_InputField languageInputField;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button saveGameButton; // 게임 저장 버튼
    [SerializeField] private Button quitGameButton; // 게임 종료 버튼

    // 현재 설정
    private GameSettingsData currentSettings;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 초기화
        Initialize();
    }

    private void Initialize()
    {
        // 설정 데이터 로드
        LoadSettings();

        // UI 이벤트 연결
        SetupUI();
    }

    private void LoadSettings()
    {
        // SaveManager에서 설정 로드
        if (SaveManager.Instance != null)
        {
            currentSettings = SaveManager.Instance.GetSettingsData();

            // 설정이 null이면 기본값 생성
            if (currentSettings == null)
            {
                currentSettings = new GameSettingsData();
                SaveSettings();
            }
        }
        else
        {
            // SaveManager가 없는 경우 기본 설정 생성
            currentSettings = new GameSettingsData();
            Debug.LogWarning("SaveManager를 찾을 수 없어 기본 설정을 사용합니다.");
        }

        // 설정 적용
        ApplySettings();
    }

    private void SetupUI()
    {
        // UI 요소들이 할당되었는지 확인
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = currentSettings.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = currentSettings.fullscreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        if (languageInputField != null)
        {
            // 현재 언어 코드 표시
            languageInputField.text = currentSettings.language;
            languageInputField.onEndEdit.AddListener(OnLanguageInputChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyButtonClicked);
        }

        // 게임 저장 버튼 설정
        if (saveGameButton != null)
        {
            saveGameButton.onClick.AddListener(SaveGame);
        }

        // 게임 종료 버튼 설정
        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
        }
    }

    private void SaveSettings()
    {
        if (SaveManager.Instance != null)
        {
            // SaveManager에 설정 데이터 설정
            var settingsData = SaveManager.Instance.GetSettingsData();
            settingsData.musicVolume = currentSettings.musicVolume;
            settingsData.fullscreen = currentSettings.fullscreen;
            settingsData.language = currentSettings.language;

            // 설정 저장
            SaveManager.Instance.SaveSettingsData();
        }
    }

    private void ApplySettings()
    {
        // 오디오 설정 적용
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
        }

        // 화면 모드 설정 적용
        Screen.fullScreen = currentSettings.fullscreen;

        // UI 업데이트
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 현재 설정값으로 UI 업데이트
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = currentSettings.musicVolume;
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = currentSettings.fullscreen;
        }

        if (languageInputField != null)
        {
            languageInputField.text = currentSettings.language;
        }
    }

    #region UI 이벤트 핸들러

    // 음악 볼륨 슬라이더 변경 이벤트
    private void OnMusicVolumeChanged(float value)
    {
        currentSettings.musicVolume = value;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    // 전체화면 토글 변경 이벤트
    private void OnFullscreenChanged(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
    }

    // 언어 입력 변경 이벤트
    private void OnLanguageInputChanged(string languageCode)
    {
        currentSettings.language = languageCode;
    }

    // 적용 버튼 클릭 이벤트
    private void OnApplyButtonClicked()
    {
        ApplySettings();
        SaveSettings();

        // 설정 패널 닫기 (선택적)
        // HideSettingsPanel();
    }

    // 게임 저장 버튼 클릭 이벤트
    private void SaveGame()
    {
        // 설정 저장
        SaveSettings();

        // 게임 전체 데이터 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
            Debug.Log("게임 데이터가 저장되었습니다.");

            // 저장 완료 메시지를 표시할 수 있습니다 (선택적)
            ShowSaveCompleteMessage();
        }
        else
        {
            Debug.LogError("SaveManager를 찾을 수 없어 게임을 저장할 수 없습니다.");
        }
    }

    // 게임 종료 버튼 클릭 이벤트
    private void QuitGame()
    {
        // 종료 전 데이터 저장 (선택적)
        SaveGame();

        Debug.Log("게임을 종료합니다.");

        // 게임 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // 저장 완료 메시지 표시 (필요에 따라 구현)
    private void ShowSaveCompleteMessage()
    {
        if(UIManager.Instance != null)
        UIManager.Instance.ShowNotification("저장완료");
       
    }

    #endregion

    #region 공개 메서드

    // 설정 패널 표시
    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // 설정 패널 숨기기
    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // 설정 패널 토글
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    // 현재 설정 가져오기
    public GameSettingsData GetCurrentSettings()
    {
        return currentSettings;
    }

    #endregion

    private void OnApplicationQuit()
    {
        // 게임 종료 시 설정 자동 저장
        SaveSettings();
    }
}