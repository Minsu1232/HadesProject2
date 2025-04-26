using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// ��� enum �߰�
public enum SlotMode
{
    Load,   // �ҷ����� ���
    NewGame // �� ���� ���� ���
}

public class SaveSlotUIManager : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Button slotButton;              // ���� ��ư
        public GameObject slotPanel;           // ���� �г� (������ �̹���)
        public TextMeshProUGUI slotNumberText; // ���� ��ȣ �ؽ�Ʈ
        public TextMeshProUGUI chapterText;    // é�� �ؽ�Ʈ
        public TextMeshProUGUI saveTimeText;   // ���� �ð� �ؽ�Ʈ
        public TextMeshProUGUI playTimeText;   // �÷��� �ð� �ؽ�Ʈ
        public GameObject emptySlotText;       // ����ִ� ���� �ؽ�Ʈ
        public TextMeshProUGUI buttonText;     // ���� ��ư �ؽ�Ʈ
    }

    // ��� ���� �ʵ� �߰�
    private SlotMode currentMode = SlotMode.Load;

    [Header("���� UI ���")]
    [SerializeField] private SlotUI[] slotUIs = new SlotUI[3];

    [Header("�׼� ��ư")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button backButton;

    [Header("Ȯ�� �г�")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TextMeshProUGUI confirmMessage;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    // ���̺� ȭ�� ��ü �г�
    [SerializeField] private GameObject saveSlotPanel;

    // �� ��ȯ ����
    [Header("�� ��ȯ ����")]
    [SerializeField] private string villageSceneName = "Village";

    // ���� ���õ� ���� �ε���
    private int selectedSlotIndex = -1;
    private bool isConfirmingDelete = false;

    // Start is called before the first frame update
    void Start()
    {
        // SaveManager Ȯ��
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager Instance�� �������� �ʽ��ϴ�. SaveManager�� ���� �߰��ϼ���.");
            return;
        }

        // ���� UI �迭 Ȯ��
        bool hasNullSlot = false;
        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null || slotUIs[i].slotButton == null)
            {
                Debug.LogError($"SlotUI[{i}] �Ǵ� ��ư�� �������� �ʾҽ��ϴ�. Inspector���� Ȯ���ϼ���.");
                hasNullSlot = true;
            }
        }

        if (hasNullSlot)
            return;

        // ���� UI �ʱ�ȭ
        InitializeSlotUI();

        // �׼� ��ư �ʱ�ȭ
        loadButton.onClick.AddListener(LoadSelectedSlot);
        deleteButton.onClick.AddListener(ShowDeleteConfirmation);
        backButton.onClick.AddListener(ClosePanel);

        // Ȯ�� �г� ��ư �ʱ�ȭ
        confirmYesButton.onClick.AddListener(ConfirmAction);
        confirmNoButton.onClick.AddListener(CancelConfirmation);

        // �ʱ� UI ������Ʈ
        UpdateSlotUI();
        UpdateButtonStates();

        // Ȯ�� �г� ����
        confirmPanel.SetActive(false);
    }

    // �г� ǥ��
    public void ShowPanel()
    {
        saveSlotPanel.SetActive(true);
        UpdateSlotUI();
    }

    // �г� �ݱ�
    public void ClosePanel()
    {
        saveSlotPanel.SetActive(false);
        selectedSlotIndex = -1;
    }

    // ��� ���� �޼���
    public void SetMode(SlotMode mode)
    {
        currentMode = mode;

        // ��忡 ���� UI ����
        switch (currentMode)
        {
            case SlotMode.Load:
                loadButton.gameObject.SetActive(true);
                break;

            case SlotMode.NewGame:
                loadButton.gameObject.SetActive(false);
                break;
        }

        // ��ư ���� ������Ʈ
        UpdateButtonStates();
    }

    // ���� UI �ʱ�ȭ
    private void InitializeSlotUI()
    {
        for (int i = 0; i < slotUIs.Length; i++)
        {
            int slotIndex = i; // closure�� ���� ���� ����

            // ���� ��ư Ŭ�� �� ���� ó��
            slotUIs[i].slotButton.onClick.AddListener(() => SelectSlot(slotIndex));
        }
    }
   
    // ���� UI ������Ʈ
    public void UpdateSlotUI()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager Instance�� �������� �ʽ��ϴ�.");
            return;
        }

        // ���� Ŭ���� ��� �� ��Ÿ������ ��ε�
        if (SaveManager.Instance.IsUsingSteamCloud())
        {
            SaveManager.Instance.ReloadSlotMetadata();
        }

        // ��� ���� ��Ÿ������ ��������
        List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
        Debug.Log($"�ε�� ���� ��Ÿ������ ��: {slotInfos.Count}");

        // �� ������ ���� ���
        for (int i = 0; i < slotInfos.Count; i++)
        {
            SlotMetadataInfo info = slotInfos[i];
            Debug.Log($"���� {i}: ������ ����={info.hasData}, é��={info.chapterProgress}, ����ð�={info.lastSaveTime}, �÷��̽ð�={info.chapterProgress}");
        }

        // �� ���� UI ������Ʈ
        for (int i = 0; i < slotUIs.Length && i < slotInfos.Count; i++)
        {
            SlotMetadataInfo info = slotInfos[i];
            SlotUI ui = slotUIs[i];
            Debug.Log($"���� ����{i} :  {info.chapterProgress}");
            // ���� ��ȣ ���� (1���� ����)
            ui.slotNumberText.text = $"���� {i + 1}";

            // �����Ͱ� �ִ��� ���ο� ���� UI ����
            if (info.hasData)
            {
                // ������� �ؽ�Ʈ ����
                ui.emptySlotText.SetActive(false);

                // ���� ������ ǥ��
                ui.chapterText.gameObject.SetActive(true);
                ui.saveTimeText.gameObject.SetActive(true);
                ui.playTimeText.gameObject.SetActive(true);

                // ��ư �ؽ�Ʈ�� '����'���� ����
                ui.buttonText.text = "����";

                // é�� �̸� ����
                
                ui.chapterText.text = $"é��: {GetChapterName(info.chapterProgress)}";

                // ���� �ð� ǥ��
                ui.saveTimeText.text = info.lastSaveTime.ToString();

                // �÷��� �ð� ǥ��
                ui.playTimeText.text = $"�÷��� �ð�:{SaveManager.Instance.GetFormattedPlayTimeForSlot(i)}";
            }
            else
            {
                // ������� �ؽ�Ʈ ǥ��
                ui.emptySlotText.SetActive(true);

                // ������ �ʵ� ����
                ui.chapterText.gameObject.SetActive(false);
                ui.saveTimeText.gameObject.SetActive(false);
                ui.playTimeText.gameObject.SetActive(false);

                // ��ư �ؽ�Ʈ�� '�� ����'���� ����
                ui.buttonText.text = "�� ����";
            }

            // ���õ� ���� ���̶���Ʈ ȿ��
            bool isSelected = (i == selectedSlotIndex);
            HighlightSelectedSlot(ui, isSelected);
        }
    }

    // é�� ID�� ������� é�� �̸� ��ȯ
    private string GetChapterName(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 1: return "�߼�";
            case 2: return "����";
            case 3: return "����";
            case 4: return "����";
            default: return $"é�� {chapterIndex}";
        }
    }

    // ���õ� ���� ���̶���Ʈ ȿ��
    private void HighlightSelectedSlot(SlotUI ui, bool isSelected)
    {
        // ���õ� ���� ǥ��
        if (isSelected)
        {
            ui.slotPanel.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        }
        else
        {
            ui.slotPanel.transform.localScale = Vector3.one;
        }
    }

    // ��ư ���� ������Ʈ
    private void UpdateButtonStates()
    {
        bool hasSelectedSlot = selectedSlotIndex >= 0;
        bool hasSelectedData = false;

        if (hasSelectedSlot && SaveManager.Instance != null)
        {
            // ���õ� ���Կ� �����Ͱ� �ִ��� Ȯ��
            List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
            if (selectedSlotIndex < slotInfos.Count)
            {
                hasSelectedData = slotInfos[selectedSlotIndex].hasData;
            }
        }

        // ���� ��忡 ���� ��ư ���� ����
        switch (currentMode)
        {
            case SlotMode.Load:
                // �ε� ��忡���� �����Ͱ� �ִ� ���Ը� �ε� ����
                loadButton.interactable = hasSelectedSlot && hasSelectedData;
                deleteButton.interactable = hasSelectedSlot && hasSelectedData;
                break;

            case SlotMode.NewGame:
                // �� ���� ��忡���� ��� ���� ���� ����
                deleteButton.interactable = hasSelectedSlot && hasSelectedData;
                break;
        }
    }

    // ���� ����
    private void SelectSlot(int slotIndex)
    {
        // ���� ���� ���� Ŭ�� �� �ٷ� ����
        if (selectedSlotIndex == slotIndex)
        {
            if (SaveManager.Instance == null) return;

            List<SlotMetadataInfo> slotInfos = SaveManager.Instance.GetAllSlotMetadata();
            bool hasData = slotInfos[slotIndex].hasData;
            Debug.Log("ù���� ���̾�α�");
            // GameStartManager ����
            if (GameStartManager.Instance != null)
            {
                if (currentMode == SlotMode.Load && hasData)
                {
                    Debug.Log("ù���� ���̾�α�OFF");
                    GameStartManager.Instance.ProcessSlotSelection(slotIndex, false);
                    
                }
                else if (currentMode == SlotMode.NewGame)
                {
                    if (hasData)
                    {
                        // �����Ͱ� �ִ� ��� Ȯ�� ��ȭ����
                        isConfirmingDelete = false;
                        confirmMessage.text = "�� ���Կ� �� ������ �����ϸ� ���� �����Ͱ� �����˴ϴ�. ����Ͻðڽ��ϱ�?";
                        confirmPanel.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("ù���� ���̾�α�ON");
                        GameStartManager.Instance.ProcessSlotSelection(slotIndex, true); 
                    }
                }
            }
            else
            {
                // ���� ���� ���� (GameStartManager�� ���� ���)
                if (currentMode == SlotMode.Load && hasData)
                {
                    LoadSelectedSlot();
                }
                else if (currentMode == SlotMode.NewGame)
                {
                    if (hasData)
                    {
                        isConfirmingDelete = false;
                        confirmMessage.text = "�� ���Կ� �� ������ �����ϸ� ���� �����Ͱ� �����˴ϴ�. ����Ͻðڽ��ϱ�?";
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

        // ���� ���� ����
        if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Length)
        {
            HighlightSelectedSlot(slotUIs[selectedSlotIndex], false);
        }

        // �� ���� ����
        selectedSlotIndex = slotIndex;
        HighlightSelectedSlot(slotUIs[selectedSlotIndex], true);

        // ��ư ���� ������Ʈ
        UpdateButtonStates();
    }

    // ���õ� ���� �ε�
    private void LoadSelectedSlot()
    {
        if (selectedSlotIndex >= 0 && SaveManager.Instance != null)
        {
            // ���� ������ ���õ� �������� ���� (���� ������ �ε� Ȱ��ȭ)
            SaveManager.Instance.SetActiveSlot(selectedSlotIndex, true);

            // �ε� ȭ�� ǥ�� (LoadingScreen ���)
            if (LoadingScreen.Instance != null)
            {
                LoadingScreen.Instance.ShowLoading(villageSceneName, OnGameLoaded);
            }
            else
            {
                // LoadingScreen�� ������ ���� �� ��ȯ
                SceneManager.LoadScene(villageSceneName);
                OnGameLoaded();
            }
        }
    }

    // ���� �ε� �Ϸ� �� ȣ��
    private void OnGameLoaded()
    {
        // ���� ������ ����
        Debug.Log("���� �ε� �Ϸ�");
    }

    // ���� Ȯ�� ǥ��
    private void ShowDeleteConfirmation()
    {
        if (selectedSlotIndex >= 0)
        {
            isConfirmingDelete = true;
            confirmMessage.text = "������ ������ ��� �����͸� �����Ͻðڽ��ϱ�?";
            confirmPanel.SetActive(true);
        }
    }

    // ���õ� ���Կ� �� ���� ����
    private void StartNewGameOnSelectedSlot()
    {
        if (SaveManager.Instance == null) return;

        // ���� ������ ���õ� �������� ���� (������ �ε� ��Ȱ��ȭ)
        SaveManager.Instance.SetActiveSlot(selectedSlotIndex, false);

        // �ʿ��ϸ� ���� ������ ����
        SaveManager.Instance.DeleteCurrentSlot();

        // �ε� ȭ�� ǥ��
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.ShowLoading(villageSceneName, OnNewGameStarted);
        }
        else
        {
            // LoadingScreen�� ������ ���� �� ��ȯ
            SceneManager.LoadScene(villageSceneName);
            OnNewGameStarted();
        }
    }

    // �� ���� ���� �Ϸ� �� ȣ��
    private void OnNewGameStarted()
    {
        // ���� ������ �ʱ� ����
        SaveInitialGameData();
        Debug.Log("�� ���� ���� �Ϸ�");
    }

    // �ʱ� ���� ������ ����
    private void SaveInitialGameData()
    {
        // �ʱ� ������ ����
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }
    }

    // Ȯ�� �׼� ó��
    private void ConfirmAction()
    {
        confirmPanel.SetActive(false);

        if (isConfirmingDelete)
        {
            // ���� ����
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SetActiveSlot(selectedSlotIndex, false);
                SaveManager.Instance.DeleteCurrentSlot();
            }

            // UI ������Ʈ
            UpdateSlotUI();
            UpdateButtonStates();
        }
        else
        {
            // �� ���� ����
            StartNewGameOnSelectedSlot();
        }
    }

    // Ȯ�� ���
    private void CancelConfirmation()
    {
        confirmPanel.SetActive(false);
    }
}