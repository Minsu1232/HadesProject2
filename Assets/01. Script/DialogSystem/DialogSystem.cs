using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.IO;

// ���̾�α� �ý��� - ������ ����
public class DialogSystem : MonoBehaviour
{
    #region ������ �� ������ ����

    // ȭ�� Ÿ�� ������
    public enum SpeakerType
    {
        Default,
        Timekeeper,  // Ÿ��Ű��
        Guardian,    // ��ȣ��
        Alexander,   // �˷����
        Enemy,       // ��
        NPC,         // �Ϲ� NPC
        System       // �ý��� �޽���
    }

    // ��ȭ ��� ������
    public enum DialogMode
    {
        StopGame,      // ���� ���� (�ð� ����, �÷��̾� �̵�/ȸ�� �Ұ�)
        AllowRotation, // ȸ���� ��� (�ð� ����, �̵� �Ұ�, ȸ�� ����)
        AllowAll,      // ��� ��� (�ð� ����, �̵�/ȸ�� ����)
        Cinematic      // �ó׸�ƽ ��� (ī�޶� ���� �� Ư�� ���� ����)
    }

    [System.Serializable]
    public class SpeakerData
    {
        public string speakerName;
        public SpeakerType speakerType;
        [Tooltip("Ÿ�Ժ��� �켱����˴ϴ�. ����θ� Ÿ�� �⺻�� ���")]
        public Color customColor = Color.clear; // �� ������ Ÿ�� �⺻�� ���
    }

    [System.Serializable]
    public class DialogLine
    {
        public string speakerName;
        public string dialogText;
        public string portraitName;
        public string eventTrigger; // �� ��ȭ �� �߻��� �̺�Ʈ
    }

    [System.Serializable]
    public class DialogSequence
    {
        public string dialogID;
        public DialogLine[] lines;
        public DialogMode dialogMode = DialogMode.StopGame; // �⺻��
    }

    [System.Serializable]
    public class DialogWrapper
    {
        public DialogSequence[] dialogSequences;
    }

    #endregion

    #region �̱��� �� ���� ����

    // �̱��� �ν��Ͻ�
    public static DialogSystem Instance { get; private set; }

    // �̺�Ʈ ������
    public delegate void DialogEvent(string eventName);
    public static event DialogEvent OnDialogEvent;

    private DialogMode currentDialogMode;
    private float originalTimeScale;
    private Queue<DialogLine> currentDialog = new Queue<DialogLine>();
    private bool isDisplayingText = false;
    private bool isDialogActive = false;
    private string currentDialogID;
    private Dictionary<string, bool> flags = new Dictionary<string, bool>();
    private Dictionary<string, DialogSequence> dialogDictionary = new Dictionary<string, DialogSequence>();
    private Dictionary<string, SpeakerData> speakerDataMap = new Dictionary<string, SpeakerData>();

    #endregion

    #region �ν����� ����

    // UI ��ҵ�
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button continueButton;

    // ��ȭ �ִϸ��̼� ����
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioSource typingSoundEffect;

    // ��� ��ȭ �������� ����
    [SerializeField] private DialogSequence[] allDialogs;

    [Header("ȭ�� ����")]
    [SerializeField] private List<SpeakerData> speakers = new List<SpeakerData>();
    [SerializeField] private bool useCustomColorsOverType = true; // Ŀ���� ���� �켱 ��� ����

    // ȭ�� Ÿ�Ժ� �⺻ ����
    [Header("Ÿ�Ժ� �⺻ ����")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color timekeeperColor = Color.white;
    [SerializeField] private Color guardianColor = new Color(1f, 0.84f, 0f); // �ݻ�
    [SerializeField] private Color alexanderColor = new Color(0.9f, 0.2f, 0.2f); // ������
    [SerializeField] private Color enemyColor = new Color(0.7f, 0.1f, 0.1f); // ���� ������
    [SerializeField] private Color npcColor = new Color(0.5f, 0.8f, 1f); // �ϴû�
    [SerializeField] private Color systemColor = new Color(0.7f, 0.7f, 0.7f); // ȸ��

    #endregion

    #region �ʱ�ȭ �� �����ֱ� �޼���

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDialogsFromStreamingAssets();
            InitializeSpeakerData();
        }
        else
        {
            Destroy(gameObject);
        }

        // �ʱ� ���� ����
        dialogPanel.SetActive(false);

        // ��� ��ư �̺�Ʈ ����
        continueButton.onClick.AddListener(DisplayNextLine);
    }

    #endregion

    #region ȭ�� �� ���� ����

    private void InitializeSpeakerData()
    {
        speakerDataMap.Clear();

        // �⺻ ȭ�� ������ �߰�
        AddDefaultSpeaker("Ÿ��Ű��", SpeakerType.Timekeeper);
        AddDefaultSpeaker("��ȣ��", SpeakerType.Guardian);
        AddDefaultSpeaker("�˷����", SpeakerType.Alexander);
        AddDefaultSpeaker("???", SpeakerType.Default);
        AddDefaultSpeaker("���ΰ�", SpeakerType.Guardian);

        // ������ ȭ�� ������ �߰�
        foreach (var data in speakers)
        {
            speakerDataMap[data.speakerName] = data;
        }
    }

    private void AddDefaultSpeaker(string name, SpeakerType type)
    {
        if (!speakerDataMap.ContainsKey(name))
        {
            SpeakerData data = new SpeakerData
            {
                speakerName = name,
                speakerType = type,
                customColor = Color.clear
            };
            speakerDataMap[name] = data;
        }
    }

    // ȭ�� Ÿ�Կ� ���� ���� ��������
    private Color GetColorForSpeakerType(SpeakerType type)
    {
        switch (type)
        {
            case SpeakerType.Timekeeper: return timekeeperColor;
            case SpeakerType.Guardian: return guardianColor;
            case SpeakerType.Alexander: return alexanderColor;
            case SpeakerType.Enemy: return enemyColor;
            case SpeakerType.NPC: return npcColor;
            case SpeakerType.System: return systemColor;
            default: return defaultColor;
        }
    }

    // ȭ�� �̸����� ���� ��������
    private Color GetColorForSpeaker(string speakerName)
    {
        if (speakerDataMap.TryGetValue(speakerName, out SpeakerData data))
        {
            // Ŀ���� ������ �����Ǿ� �ְ� �켱 ��� �ɼ��� ���� ������ Ŀ���� ���� ���
            if (useCustomColorsOverType && data.customColor != Color.clear)
            {
                return data.customColor;
            }
            // �ƴϸ� Ÿ�� �⺻ ���� ���
            return GetColorForSpeakerType(data.speakerType);
        }

        // ��ϵ��� ���� ȭ�ڴ� �⺻ ���� ��ȯ
        return defaultColor;
    }

    // �� ȭ�� �߰�/������Ʈ (��Ÿ�ӿ��� ��� ����)
    public void RegisterSpeaker(string name, SpeakerType type, Color customColor = default)
    {
        SpeakerData data = new SpeakerData
        {
            speakerName = name,
            speakerType = type,
            customColor = customColor
        };

        speakerDataMap[name] = data;
    }

    #endregion

    #region ���̾�α� ���� �ε�

    private void LoadDialogsFromStreamingAssets()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Dialogs/dialogs.json");

        // Windows, Mac, iOS �� ���� ���� �б� ������ �÷���
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            ParseDialogJson(jsonContent);
        }
        else
        {
            Debug.LogWarning($"dialogs.json ������ ã�� �� �����ϴ�: {filePath}");
        }
    }

    private void ParseDialogJson(string jsonContent)
    {
        DialogWrapper wrapper = JsonUtility.FromJson<DialogWrapper>(jsonContent);

        if (wrapper != null && wrapper.dialogSequences != null)
        {
            foreach (DialogSequence sequence in wrapper.dialogSequences)
            {
                dialogDictionary[sequence.dialogID] = sequence;
            }
        }
        else
        {
            Debug.LogWarning("JSON �Ľ� ���� �Ǵ� ������ ����.");
        }
    }

    // �ܺο��� ��ȭ�� JSON���� �ε��ϴ� �Լ�
    public void LoadDialogsFromJson(TextAsset jsonFile)
    {
        if (jsonFile != null)
        {
            DialogSequence[] loadedDialogs = JsonUtility.FromJson<DialogSequence[]>(jsonFile.text);

            foreach (DialogSequence sequence in loadedDialogs)
            {
                dialogDictionary[sequence.dialogID] = sequence;
            }
        }
    }

    #endregion

    #region ���̾�α� ǥ�� �� ����

    // ���� ���¿� ���� ���̾�α� ǥ��
    public void StartDialogIfNotShown(string dialogID)
    {
        if (!GameProgressManager.Instance.IsDialogShown(dialogID))
        {
            StartDialog(dialogID);
            GameProgressManager.Instance.MarkDialogAsShown(dialogID);
        }
    }

    // ��ȭ ����
    public void StartDialog(string dialogID)
    {
        // �̹� ��ȭ ���̸� ����
        if (isDialogActive)
        {
            EndDialog();
        }
        // ���� ���̾�α� ID ����
        currentDialogID = dialogID;
        // ��û�� ��ȭ ������ Ȯ��
        if (!dialogDictionary.ContainsKey(dialogID))
        {
            Debug.LogWarning($"Dialog ID not found: {dialogID}");
            return;
        }

        // ���� ��ȭ ����
        DialogSequence sequence = dialogDictionary[dialogID];
        currentDialog.Clear();

        // ��ȭ ��� ����
        currentDialogMode = sequence.dialogMode;
        originalTimeScale = Time.timeScale;

        // ���� ���� ����
        ApplyGameControl(false);

        // ��� ��ȭ ������ ť�� �߰�
        foreach (DialogLine line in sequence.lines)
        {
            currentDialog.Enqueue(line);
        }

        // ��ȭ �г� Ȱ��ȭ �� ù ��ȭ ǥ��
        dialogPanel.SetActive(true);
        isDialogActive = true;
        DisplayNextLine();
    }

    // ���� ��ȭ ���� ǥ��
    public void DisplayNextLine()
    {
        // �ؽ�Ʈ �ִϸ��̼� ���̸� ��� �Ϸ�
        if (isDisplayingText)
        {
            StopAllCoroutines();
            dialogText.text = currentDialog.Peek().dialogText;
            isDisplayingText = false;
            continueButton.gameObject.SetActive(true);
            return;
        }

        // �� �̻� ǥ���� ��ȭ�� ������ ����
        if (currentDialog.Count == 0)
        {
            EndDialog();
            return;
        }

        // ���� ��ȭ ���� ��������
        DialogLine line = currentDialog.Dequeue();

        // UI ������Ʈ
        speakerNameText.text = line.speakerName;

        // ȭ�� �̸��� ���� ����
        speakerNameText.color = GetColorForSpeaker(line.speakerName);

        // �ؽ�Ʈ �ִϸ��̼� ����
        StartCoroutine(TypeText(line.dialogText));

        // �̺�Ʈ Ʈ���� ó��
        if (!string.IsNullOrEmpty(line.eventTrigger))
        {
            HandleEventFromDialog(line.eventTrigger);
            OnDialogEvent?.Invoke(line.eventTrigger);
        }
    }

    // �ؽ�Ʈ Ÿ���� ȿ��
    private IEnumerator TypeText(string text)
    {
        isDisplayingText = true;
        dialogText.text = "";
        continueButton.gameObject.SetActive(false);

        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c;

            // Ÿ���� ���� ���
            if (typingSoundEffect != null && c != ' ')
            {
                typingSoundEffect.Play();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isDisplayingText = false;
        continueButton.gameObject.SetActive(true);
    }

    // ��ȭ ����
    public void EndDialog()
    {
        // ���� ���� ����
        ApplyGameControl(true);

        dialogPanel.SetActive(false);
        isDialogActive = false;

        // ���̾�α� �Ϸ� �̺�Ʈ �߻�
        if (currentDialogID != null)
        {
            string completeEvent = $"DialogComplete:{currentDialogID}";
            OnDialogEvent?.Invoke(completeEvent);
        }

        currentDialog.Clear();
        currentDialogID = null;
    }

    #endregion

    #region ���� ���� �� ���� ����

    private void ApplyGameControl(bool restore)
    {
        // �÷��̾� ã��
        PlayerMovement playerMovement = GameInitializer.Instance.gameObject.GetComponent<PlayerMovement>();

        if (restore)
        {
            // ��ȭ ���� - ��� ���� ���� 
            Time.timeScale = originalTimeScale;

            if (playerMovement != null)
            {
                playerMovement.SetMovementEnabled(true);
                playerMovement.SetRotationEnabled(true);
            }

            // �ó׸�ƽ ��忴�ٸ� ī�޶� ����
            if (currentDialogMode == DialogMode.Cinematic && CinematicCameraController.Instance != null)
            {
                CinematicCameraController.Instance.ResetCamera();
            }
        }
        else
        {
            // ��ȭ ���� - ��忡 ���� ����
            switch (currentDialogMode)
            {
                case DialogMode.StopGame:
                    Time.timeScale = 0f; // �ð� ����

                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(false);
                    }
                    break;

                case DialogMode.AllowRotation:
                    // �ð��� ���� �帧
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(true);
                    }
                    break;

                case DialogMode.AllowAll:
                    // ��� �� ��� (��� ��ȭ)
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(true);
                        playerMovement.SetRotationEnabled(true);
                    }
                    break;

                case DialogMode.Cinematic:
                    // �ó׸�ƽ ��� - �÷��̾� ������ ����
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(false);
                    }
                    break;
            }
        }
    }

    // �÷��� ����
    public void SetFlag(string flagName, bool value)
    {
        flags[flagName] = value;
    }

    // �÷��� Ȯ��
    public bool GetFlag(string flagName)
    {
        return flags.TryGetValue(flagName, out bool value) && value;
    }

    #endregion

    #region �̺�Ʈ ó��

    // ���̾�α� �̺�Ʈ ó���� Ȯ��
    private void HandleEventFromDialog(string eventName)
    {
        // �÷��� ���� �̺�Ʈ ó��
        if (eventName.StartsWith("SetFlag:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 3)
            {
                string flagName = parts[1];
                bool value = bool.Parse(parts[2]);
                GameProgressManager.Instance.SetFlag(flagName, value);
            }
        }
        // é�� �ر� �̺�Ʈ ó��
        else if (eventName.StartsWith("UnlockChapter:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                int chapter = int.Parse(parts[1]);
                GameProgressManager.Instance.SetCurrentChapter(chapter);
            }
        }
        // ���� �ر� �̺�Ʈ ó��
        else if (eventName.StartsWith("UnlockWeapon:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                string weaponID = parts[1];
                GameProgressManager.Instance.UnlockWeapon(weaponID);
            }
        }
        // ���� ȹ�� �̺�Ʈ ó��
        else if (eventName.StartsWith("AcquireFragment:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                string fragmentID = parts[1];
                GameProgressManager.Instance.AcquireFragment(fragmentID);
            }
        }

        // ���� �̺�Ʈ ó�� ȣ��
        OnDialogEvent?.Invoke(eventName);
    }

    #endregion
}

#region ���� ��ƿ��Ƽ Ŭ����

// �̺�Ʈ ó�� ���� Ŭ����
public class DialogEventHandler : MonoBehaviour
{
    private void OnEnable()
    {
        DialogSystem.OnDialogEvent += HandleDialogEvent;
    }

    private void OnDisable()
    {
        DialogSystem.OnDialogEvent -= HandleDialogEvent;
    }

    private void HandleDialogEvent(string eventName)
    {
        switch (eventName)
        {
            case "StartGameplay":
                // �����÷��� ���� ó��
                Debug.Log("�����÷��� ����");
                break;

            case "StartAlexanderFight":
                // ������ ���� ó��
                Debug.Log("������ ����");
                break;

            case "ShowMemory":
                // ��� ǥ�� ó��
                Debug.Log("��� ǥ��");
                break;

            case "UnlockWeapon":
                // ���� �ر� ó��
                Debug.Log("���� �ر�");
                break;

            default:
                Debug.Log($"�̺�Ʈ ó��: {eventName}");
                break;
        }
    }
}

// ���̾�α� Ʈ���� ������Ʈ
public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private string dialogID;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    // �÷��̾ Ʈ���ſ� �������� ��
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialog();
        }
    }

    // �������� ��ȭ Ʈ����
    public void TriggerDialog()
    {
        if (!triggerOnce || !hasTriggered)
        {
            DialogSystem.Instance.StartDialog(dialogID);
            hasTriggered = true;
        }
    }
}

#endregion