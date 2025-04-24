using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // �̱��� �ν��Ͻ�
    public static UIManager Instance { get; private set; }


    [Header("�˸� �ý���")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationParent; // Canvas ������ �˸��� ǥ�õ� ��ġ
    [SerializeField] private float notificationDuration = 3.0f; // �˸� ǥ�� �ð�
    [SerializeField] private float fadeTime = 0.5f; // �˸� ���̵� ��/�ƿ� �ð�
    // ���� �ִ� UI �˾����� �����ϴ� ����
    private Stack<GameObject> activeUIStack = new Stack<GameObject>();
    private Queue<NotificationInfo> notificationQueue = new Queue<NotificationInfo>();
    private bool isShowingNotification = false;

    private string lastMessageContent = string.Empty;
    // �˸� ���� Ŭ����
    #region �˸� ���
    private class NotificationInfo
    {
        public string message;
        public Color color = Color.white;
        public float duration;
        public bool hasIcon;
        public Sprite icon;
    }
    // �˸� �����ֱ�
    public void ShowNotification(string message)
    {
        ShowNotification(message, Color.white, notificationDuration, false, null);
        Debug.Log($"��������!!!!!!!!{message}");
    }

    // �˸� �����ֱ� (���� ����)
    public void ShowNotification(string message, Color color)
    {
        ShowNotification(message, color, notificationDuration, false, null);
        Debug.Log($"��������!!!!!!!!222{message}");
    }

    // �˸� �����ֱ� (������ ����)
    public void ShowNotification(string message, Sprite icon)
    {
        ShowNotification(message, Color.white, notificationDuration, true, icon);
    }

    // �˸� �����ֱ� (��� �ɼ�)
    public void ShowNotification(string message, Color color, float duration, bool hasIcon, Sprite icon)
    {
        // ���� �޽����� �Ȱ��� �������� Ȯ�� (�ߺ� ����)
        if (message == lastMessageContent && notificationQueue.Count > 0)
        {
            Debug.Log($"�ߺ� �˸� ����: {message}");
            return; // �ߺ� �˸� ����
        }

        // ������ �޽��� ���� ������Ʈ
        lastMessageContent = message;

        NotificationInfo info = new NotificationInfo
        {
            message = message,
            color = color,
            duration = duration,
            hasIcon = hasIcon,
            icon = icon
        };

        // ť�� �˸� �߰�
        notificationQueue.Enqueue(info);

        // ���� ǥ�� ���� �˸��� ������ �˸� ǥ�� ����
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    // �˸� ť ó�� �ڷ�ƾ
    private IEnumerator ProcessNotificationQueue()
    {
        isShowingNotification = true;

        while (notificationQueue.Count > 0)
        {
            NotificationInfo info = notificationQueue.Dequeue();
            yield return StartCoroutine(ShowNotificationCoroutine(info));
        }

        isShowingNotification = false;
    }

    // ���� �˸� ǥ�� �ڷ�ƾ
    private IEnumerator ShowNotificationCoroutine(NotificationInfo info)
    {
        // �˸� �������� ������ ����
        if (notificationPrefab == null || notificationParent == null)
        {
            Debug.LogError("�˸� ������ �Ǵ� �θ� Transform�� �������� �ʾҽ��ϴ�.");
            yield break;
        }

        // �˸� ������Ʈ ����
        GameObject notificationObj = Instantiate(notificationPrefab, notificationParent);
        RectTransform rectTransform = notificationObj.GetComponent<RectTransform>();

        // �˸� ������Ʈ ã��
        TextMeshProUGUI messageText = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
        Image iconImage = notificationObj.transform.Find("Icon")?.GetComponent<Image>();
        CanvasGroup canvasGroup = notificationObj.GetComponent<CanvasGroup>();

        if (messageText == null)
        {
            Debug.LogError("�˸� �����տ� TextMeshProUGUI ������Ʈ�� �����ϴ�.");
            Destroy(notificationObj);
            yield break;
        }

        // �˸� ���� ����
        messageText.text = info.message;
        messageText.color = info.color;

        // ������ ����
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(info.hasIcon);
            if (info.hasIcon && info.icon != null)
            {
                iconImage.sprite = info.icon;
            }
        }

        // ���̵� ��
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                canvasGroup.alpha = elapsedTime / fadeTime;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        // ǥ�� ����
        yield return new WaitForSeconds(info.duration);

        // ���̵� �ƿ�
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeTime)
            {
                canvasGroup.alpha = 1f - (elapsedTime / fadeTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        // �˸� ������Ʈ ����
        Destroy(notificationObj);
    }
    #endregion
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

    private void Update()
    {
        // ESC Ű �Է� ����
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUIStack.Count > 0)
            {
                // ���� UI�� ������ �ֻ��� UI �ݱ�
                CloseTopUI();
            }
            else
            {
                // ���� UI�� ������ ���� �г� ���
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.ToggleSettingsPanel();
                }
            }
        }
    }

    // UI�� Ȱ��ȭ ���ÿ� ���
    public void RegisterActiveUI(GameObject uiPanel)
    {
        // �̹� ���ÿ� �ִ��� Ȯ���ϰ� �ߺ� ��� ����
        if (activeUIStack.Contains(uiPanel))
        {
            // �̹� ���ÿ� �ִٸ� �ֻ����� �������ϱ� ���� ���� �׸� ����
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // �ٽ� ���ÿ� �ֱ� (����)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }
        }

        // ���ÿ� �߰�
        activeUIStack.Push(uiPanel);

        Debug.Log($"UI ���: {uiPanel.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
    }

    // UI�� Ȱ��ȭ ���ÿ��� ����
    public void UnregisterActiveUI(GameObject uiPanel)
    {
        // �ش� UI�� ���ÿ� �ִ��� Ȯ���ϰ� ����
        if (activeUIStack.Contains(uiPanel))
        {
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // �ٽ� ���ÿ� �ֱ� (����)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }

            Debug.Log($"UI ����: {uiPanel.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
        }
    }

    // UI â ����
    public void OpenUI(GameObject uiPanel)
    {
        // ������ �̹� ���� �������� Ȯ��
        if (uiPanel.activeSelf)
            return;

        uiPanel.SetActive(true);
        RegisterActiveUI(uiPanel);
    }

    // �ֻ��� UI �ݱ�
    public void CloseTopUI()
    {
        if (activeUIStack.Count > 0)
        {
            GameObject topUI = activeUIStack.Pop();
            topUI.SetActive(false);
            Debug.Log($"�ֻ��� UI �ݱ�: {topUI.name}, ���� Ȱ�� UI ��: {activeUIStack.Count}");
        }
    }

    // Ư�� UI �ݱ�
    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            UnregisterActiveUI(uiPanel);
        }
    }

    // ��� UI �ݱ�
    public void CloseAllUI()
    {
        while (activeUIStack.Count > 0)
        {
            GameObject ui = activeUIStack.Pop();
            ui.SetActive(false);
        }
        Debug.Log("��� UI �ݱ� �Ϸ�");
    }

    // ���� Ȱ��ȭ�� UI ��� ��ȸ (������)
    public List<GameObject> GetActiveUIs()
    {
        return new List<GameObject>(activeUIStack);
    }
}