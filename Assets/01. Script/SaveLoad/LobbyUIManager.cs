using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUIManager : MonoBehaviour
{
    [Header("���� �޴� ��ư")]
    [SerializeField] private Button startButton;    // �����ϱ� ��ư
    [SerializeField] private Button loadButton;     // �ҷ����� ��ư
    [SerializeField] private Button optionsButton;  // �ɼ� ��ư
    [SerializeField] private Button quitButton;     // ���� ��ư (�ʿ��)

    [Header("�г�")]
    [SerializeField] private GameObject saveSlotPanel; // ���̺� ���� UI �г�
    [SerializeField] private GameObject optionsPanel;  // �ɼ� �г�

    [Header("������Ʈ ����")]
    [SerializeField] private SaveSlotUIManager saveSlotUIManager;

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