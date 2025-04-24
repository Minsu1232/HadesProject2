using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SettingsManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static SettingsManager Instance { get; private set; }

    [Header("UI ����")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_InputField languageInputField;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button saveGameButton; // ���� ���� ��ư
    [SerializeField] private Button quitGameButton; // ���� ���� ��ư

    // ���� ����
    private GameSettingsData currentSettings;

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // �ʱ�ȭ
        Initialize();
    }

    private void Initialize()
    {
        // ���� ������ �ε�
        LoadSettings();

        // UI �̺�Ʈ ����
        SetupUI();
    }

    private void LoadSettings()
    {
        // SaveManager���� ���� �ε�
        if (SaveManager.Instance != null)
        {
            currentSettings = SaveManager.Instance.GetSettingsData();

            // ������ null�̸� �⺻�� ����
            if (currentSettings == null)
            {
                currentSettings = new GameSettingsData();
                SaveSettings();
            }
        }
        else
        {
            // SaveManager�� ���� ��� �⺻ ���� ����
            currentSettings = new GameSettingsData();
            Debug.LogWarning("SaveManager�� ã�� �� ���� �⺻ ������ ����մϴ�.");
        }

        // ���� ����
        ApplySettings();
    }

    private void SetupUI()
    {
        // UI ��ҵ��� �Ҵ�Ǿ����� Ȯ��
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
            // ���� ��� �ڵ� ǥ��
            languageInputField.text = currentSettings.language;
            languageInputField.onEndEdit.AddListener(OnLanguageInputChanged);
        }

        if (applyButton != null)
        {
            applyButton.onClick.AddListener(OnApplyButtonClicked);
        }

        // ���� ���� ��ư ����
        if (saveGameButton != null)
        {
            saveGameButton.onClick.AddListener(SaveGame);
        }

        // ���� ���� ��ư ����
        if (quitGameButton != null)
        {
            quitGameButton.onClick.AddListener(QuitGame);
        }
    }

    private void SaveSettings()
    {
        if (SaveManager.Instance != null)
        {
            // SaveManager�� ���� ������ ����
            var settingsData = SaveManager.Instance.GetSettingsData();
            settingsData.musicVolume = currentSettings.musicVolume;
            settingsData.fullscreen = currentSettings.fullscreen;
            settingsData.language = currentSettings.language;

            // ���� ����
            SaveManager.Instance.SaveSettingsData();
        }
    }

    private void ApplySettings()
    {
        // ����� ���� ����
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(currentSettings.musicVolume);
        }

        // ȭ�� ��� ���� ����
        Screen.fullScreen = currentSettings.fullscreen;

        // UI ������Ʈ
        UpdateUI();
    }

    private void UpdateUI()
    {
        // ���� ���������� UI ������Ʈ
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

    #region UI �̺�Ʈ �ڵ鷯

    // ���� ���� �����̴� ���� �̺�Ʈ
    private void OnMusicVolumeChanged(float value)
    {
        currentSettings.musicVolume = value;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    // ��üȭ�� ��� ���� �̺�Ʈ
    private void OnFullscreenChanged(bool isFullscreen)
    {
        currentSettings.fullscreen = isFullscreen;
    }

    // ��� �Է� ���� �̺�Ʈ
    private void OnLanguageInputChanged(string languageCode)
    {
        currentSettings.language = languageCode;
    }

    // ���� ��ư Ŭ�� �̺�Ʈ
    private void OnApplyButtonClicked()
    {
        ApplySettings();
        SaveSettings();

        // ���� �г� �ݱ� (������)
        // HideSettingsPanel();
    }

    // ���� ���� ��ư Ŭ�� �̺�Ʈ
    private void SaveGame()
    {
        // ���� ����
        SaveSettings();

        // ���� ��ü ������ ����
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
            Debug.Log("���� �����Ͱ� ����Ǿ����ϴ�.");

            // ���� �Ϸ� �޽����� ǥ���� �� �ֽ��ϴ� (������)
            ShowSaveCompleteMessage();
        }
        else
        {
            Debug.LogError("SaveManager�� ã�� �� ���� ������ ������ �� �����ϴ�.");
        }
    }

    // ���� ���� ��ư Ŭ�� �̺�Ʈ
    private void QuitGame()
    {
        // ���� �� ������ ���� (������)
        SaveGame();

        Debug.Log("������ �����մϴ�.");

        // ���� ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ���� �Ϸ� �޽��� ǥ�� (�ʿ信 ���� ����)
    private void ShowSaveCompleteMessage()
    {
        if(UIManager.Instance != null)
        UIManager.Instance.ShowNotification("����Ϸ�");
       
    }

    #endregion

    #region ���� �޼���

    // ���� �г� ǥ��
    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    // ���� �г� �����
    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    // ���� �г� ���
    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    // ���� ���� ��������
    public GameSettingsData GetCurrentSettings()
    {
        return currentSettings;
    }

    #endregion

    private void OnApplicationQuit()
    {
        // ���� ���� �� ���� �ڵ� ����
        SaveSettings();
    }
}