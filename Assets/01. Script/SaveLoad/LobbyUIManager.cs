using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class LobbyUIManager : MonoBehaviour
{
    [Header("���� �޴� ��ư")]
    [SerializeField] private Button startButton;    // �����ϱ� ��ư
    [SerializeField] private Button loadButton;     // �ҷ����� ��ư
    [SerializeField] private Button optionsButton;  // �ɼ� ��ư
    [SerializeField] private Button quitButton;     // ���� ��ư (�ʿ��)

    [Header("��ư ȣ�� ȿ�� ����")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;  // ȣ�� �� ��ư ũ�� ����
    [SerializeField] private float hoverAnimationSpeed = 5f;     // ȣ�� �ִϸ��̼� �ӵ�

    [Header("�г�")]
    [SerializeField] private GameObject saveSlotPanel; // ���̺� ���� UI �г�
    [SerializeField] private GameObject optionsPanel;  // �ɼ� �г�

    [Header("������Ʈ ����")]
    [SerializeField] private SaveSlotUIManager saveSlotUIManager;

    // ���� ũ�⸦ ������ Dictionary
    private System.Collections.Generic.Dictionary<Button, Vector3> originalScales = new System.Collections.Generic.Dictionary<Button, Vector3>();
    // ���� ȣ�� ���� ��ư
    private Button currentHoveredButton = null;

    private void Start()
    {
        // ��ư �̺�Ʈ ����
        startButton.onClick.AddListener(ShowSaveSlotPanelForNewGame);
        loadButton.onClick.AddListener(ShowSaveSlotPanelForLoad);
        optionsButton.onClick.AddListener(ShowOptionsPanel);

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // �г� �ʱ� ���� ����
        saveSlotPanel.SetActive(false);
        optionsPanel.SetActive(false);

        // ��ư ȣ�� �̺�Ʈ ����
        SetupButtonHoverEffects();
    }

    private void SetupButtonHoverEffects()
    {
        // ��� ��ư�� ȣ�� ȿ�� �߰�
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

        // ���� ũ�� ����
        originalScales[button] = button.transform.localScale;

        // �̺�Ʈ Ʈ���� ������Ʈ �������� �Ǵ� �߰�
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // ������ ���� �̺�Ʈ ����
        EventTrigger.Entry entryEvent = new EventTrigger.Entry();
        entryEvent.eventID = EventTriggerType.PointerEnter;
        entryEvent.callback.AddListener((data) => { OnPointerEnter(button); });
        trigger.triggers.Add(entryEvent);

        // ������ ���� �̺�Ʈ ����
        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { OnPointerExit(button); });
        trigger.triggers.Add(exitEvent);
    }

    private void OnPointerEnter(Button button)
    {
        // ���� ȣ�� ���� ��ư ����
        currentHoveredButton = button;
    }

    private void OnPointerExit(Button button)
    {
        // ȣ�� ȿ�� ����
        if (currentHoveredButton == button)
        {
            currentHoveredButton = null;
        }
    }

    private void Update()
    {
        // ESC Ű ó��
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ���� �ִ� �г� �ݱ�
            if (saveSlotPanel.activeSelf)
            {
                saveSlotPanel.SetActive(false);
            }
            else if (optionsPanel.activeSelf)
            {
                optionsPanel.SetActive(false);
            }
        }

        // ȣ�� �ִϸ��̼� ������Ʈ
        UpdateHoverEffects();
    }

    private void UpdateHoverEffects()
    {
        // ��� ��ư Ȯ��
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

        // ȣ�� ���� ��ư�̸� ũ�� Ȯ��
        if (button == currentHoveredButton)
        {
            targetScale = originalScales[button] * hoverScaleMultiplier;
        }
        else
        {
            targetScale = originalScales[button];
        }

        // �ε巯�� �ִϸ��̼����� ũ�� ����
        button.transform.localScale = Vector3.Lerp(
            button.transform.localScale,
            targetScale,
            Time.deltaTime * hoverAnimationSpeed
        );
    }

    // �� ���ӿ� ���̺� ���� �г� ǥ��
    private void ShowSaveSlotPanelForNewGame()
    {
        // ȿ���� ���
        //AudioManager.Instance?.PlaySFX("button_click");

        // ���� UI �Ŵ����� �� ���� ��� ����
        saveSlotUIManager.SetMode(SlotMode.NewGame);

        // �г� ǥ��
        ShowSaveSlotPanel();
    }

    // �ҷ������ ���̺� ���� �г� ǥ��
    private void ShowSaveSlotPanelForLoad()
    {
        // ȿ���� ���
        //AudioManager.Instance?.PlaySFX("button_click");

        // ���� UI �Ŵ����� �ε� ��� ����
        saveSlotUIManager.SetMode(SlotMode.Load);

        // �г� ǥ��
        ShowSaveSlotPanel();
    }

    // ���̺� ���� �г� ǥ��
    private void ShowSaveSlotPanel()
    {
        // �г� Ȱ��ȭ
        saveSlotPanel.SetActive(true);
        optionsPanel.SetActive(false);

        // GameManager�� UI ���� ���� �˸� (GameManager�� �ִ� ���)
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj != null)
        {
            GameManager gameManager = gameManagerObj.GetComponent<GameManager>();
            if (gameManager != null)
            {
                gameManager.OnUIStateChanged(saveSlotPanel, true);
            }
        }

        // ���� UI ������Ʈ
        saveSlotUIManager.UpdateSlotUI();
    }

    // �ɼ� �г� ǥ��
    private void ShowOptionsPanel()
    {
        // ȿ���� ���
        //AudioManager.Instance?.PlaySFX("button_click");

        // �г� ǥ��
        optionsPanel.SetActive(true);
        saveSlotPanel.SetActive(false);

        // GameManager�� UI ���� ���� �˸� (GameManager�� �ִ� ���)
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

    // �г� �ݱ�
    public void ClosePanel(GameObject panel)
    {
        // ȿ���� ���
        //AudioManager.Instance?.PlaySFX("button_click");

        // �г� ��Ȱ��ȭ
        panel.SetActive(false);

        // GameManager�� UI ���� ���� �˸� (GameManager�� �ִ� ���)
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

    // ���� ����
    private void QuitGame()
    {
        // ȿ���� ���
        //AudioManager.Instance?.PlaySFX("button_click");

        // ���� ���� �� ������ ����
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }

        // ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}