// ItemPickupNotification.cs - ������ ȹ�� �˸� �ý���
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupNotification : MonoBehaviour
{
    [Header("�˸� ����")]
    [SerializeField] private GameObject notificationPrefab;  // �˸� UI ������
    [SerializeField] private Transform notificationParent;   // �˸����� ������ �θ� Transform
    [SerializeField] private int maxNotifications = 3;       // �ִ� ���� ǥ�� �˸� ��
    [SerializeField] private float stackingTime = 0.5f;      // ���� ������ ����ŷ �ð� (��)

    [Header("�ִϸ��̼� ����")]
    [SerializeField] private float displayDuration = 3.0f;    // ǥ�� ���� �ð�
    [SerializeField] private float slideDuration = 0.3f;      // �����̵� �ִϸ��̼� �ð�
    [SerializeField] private float fadeOutDuration = 0.5f;    // ���̵� �ƿ� �ð�

    [Header("��ġ ����")]
    [SerializeField] private Vector2 startPosition = new Vector2(200, 0);  // ���� ��ġ (ȭ�� �ٱ�)
    [SerializeField] private Vector2 endPosition = new Vector2(0, 0);      // ���� ��ġ (ȭ�� ��)

    // ��͵��� ����
    private readonly Color[] rarityColors = new Color[5]
    {
        new Color(1f, 1f, 1f), // Common (ȸ��)
        new Color(0.3f, 0.7f, 0.3f), // Uncommon (���)
        new Color(0.3f, 0.3f, 0.9f), // Rare (�Ķ���)
        new Color(0.7f, 0.3f, 0.9f), // Epic (�����)
        new Color(1.0f, 0.8f, 0.0f)  // Legendary (�ݻ�)
    };

    // Ȱ��ȭ�� �˸� ������ ���� ť
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();

    // ���� ��� ���� ���� ������ ����
    private float currentVerticalOffset = 0f;

    // ������ ����ŷ�� ���� ���� (������ ID -> �˸� ����)
    private Dictionary<int, ItemNotificationInfo> pendingNotifications = new Dictionary<int, ItemNotificationInfo>();

    // ���� �ֱ� �˸� ó�� �ð�
    private float lastNotificationTime = 0f;

    // �̱��� �ν��Ͻ�
    public static ItemPickupNotification Instance { get; private set; }

    // �˸� ������ �����ϱ� ���� Ŭ����
    private class ItemNotificationInfo
    {
        public Item item;
        public int quantity;
        public float firstPickupTime;

        public ItemNotificationInfo(Item item, int quantity)
        {
            this.item = item;
            this.quantity = quantity;
            this.firstPickupTime = Time.time;
        }
    }

    private void Awake()
    {
        // �θ� Transform�� �������� ���� ��� ���� ��ü�� Transform ���
        if (notificationParent == null)
        {
            notificationParent = transform;
        }
    }

    private void Start()
    {
        // ������ ȹ�� �̺�Ʈ�� �ޱ� ���� ItemPickupObject Ŭ���� �� ȹ�� �Ϸ� ������ ���� �ʿ�
        // �Ϲ������� ItemPickupObject�� PickupItem �޼��� �ȿ��� ȣ��
    }

    // ������ ȹ�� �˸��� ǥ���ϴ� ���� �޼���
    public void ShowNotification(Item item, int quantity)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("�˸� �������� �������� �ʾҽ��ϴ�!");
            return;
        }

        // ���� �ð� ����
        float currentTime = Time.time;

        // ����ŷ �������� Ȯ�� (���� ������ �������� �ֱٿ� ȹ��Ǿ�����)
        if (TryStackNotification(item, quantity, currentTime))
        {
            // ���������� ����ŷ�Ǿ����� �� �˸� �������� ����
            return;
        }

        // ����ŷ ���� ���� (Ư�� �ð����� ������ ȹ�� ������)
        StartCoroutine(ProcessNotification(item, quantity));

        // ������ �˸� �ð� ������Ʈ
        lastNotificationTime = currentTime;
    }

    // ���� ������ ����ŷ �õ�
    private bool TryStackNotification(Item item, int quantity, float currentTime)
    {
        // �̹� Ȱ��ȭ�� �˸� �߿��� ���� �������� ã��
        foreach (GameObject notification in activeNotifications)
        {
            NotificationData data = notification.GetComponent<NotificationData>();
            if (data != null && data.itemId == item.itemID)
            {
                // ���� ������ �˸��� �̹� ǥ�õǰ� �ִ� ���, ���� ������Ʈ
                data.quantity += quantity;

                // �ؽ�Ʈ ������Ʈ
                TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
                if (notificationText != null)
                {
                    notificationText.text = $"{item.itemName} x{data.quantity} ȹ��";
                }

                // ������ �ִϸ��̼����� ���� ���� ǥ��
                StartCoroutine(PulseAnimation(notification));

                return true;
            }
        }

        return false;
    }

    // �˸� ó�� �ڷ�ƾ (���� �ð� ���� ���� ������ ȹ�� ������)
    private IEnumerator ProcessNotification(Item item, int quantity)
    {
        int itemId = item.itemID;

        // ���� ó�� ���� ���������� Ȯ��
        if (pendingNotifications.ContainsKey(itemId))
        {
            // �̹� ���� �������� ��� ���̸� ������ ����
            pendingNotifications[itemId].quantity += quantity;
            yield break;
        }

        // �� ������ ���� �߰�
        pendingNotifications.Add(itemId, new ItemNotificationInfo(item, quantity));

        // ����ŷ �ð� ���� ��� (���� �������� �� ���� �� ����)
        yield return new WaitForSeconds(stackingTime);

        // ���� ������ ���� �˸� ����
        if (pendingNotifications.TryGetValue(itemId, out ItemNotificationInfo info))
        {
            CreateNotification(info.item, info.quantity);
            pendingNotifications.Remove(itemId);
        }
    }

    // ���� �˸� UI ����
    private void CreateNotification(Item item, int quantity)
    {
        // �� �˸� ����
        GameObject notification = Instantiate(notificationPrefab, notificationParent);

        // �˸� ������ ������Ʈ �߰�
        NotificationData data = notification.AddComponent<NotificationData>();
        data.itemId = item.itemID;
        data.quantity = quantity;

        // �˸� UI ����
        SetupNotificationUI(notification, item, quantity);

        // �˸� ��ġ ��� (�̹� �����ϴ� �˸����� ��ġ�� ���)
        CalculateNotificationPosition(notification);

        // �˸� �ִϸ��̼� ����
        StartCoroutine(AnimateNotification(notification));

        // Ȱ�� �˸� ����
        activeNotifications.Enqueue(notification);

        // �ִ� ���� �ʰ� �� ������ �˸� ����
        if (activeNotifications.Count > maxNotifications)
        {
            GameObject oldNotification = activeNotifications.Dequeue();
            Destroy(oldNotification);
        }
    }

    // ���� ���� �� �˸� �޽� �ִϸ��̼�
    private IEnumerator PulseAnimation(GameObject notification)
    {
        RectTransform rect = notification.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // Ŀ���� �ִϸ��̼�
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ���� ũ��� ���ƿ��� �ִϸ��̼�
        elapsed = 0f;
        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localScale = originalScale;
    }

    // �˸� UI ��� ����
    private void SetupNotificationUI(GameObject notification, Item item, int quantity)
    {
        // �ؽ�Ʈ ������Ʈ ã��
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (notificationText == null) return;

        // ������ �̸� + ���� ����
        string quantityText = quantity > 1 ? $" x{quantity}" : "";
        notificationText.text = $"{item.itemName}{quantityText} ȹ��";

        // ��͵��� ���� ���� ����
        int rarityIndex = (int)item.rarity;
        if (rarityIndex >= 0 && rarityIndex < rarityColors.Length)
        {
            notificationText.color = rarityColors[rarityIndex];
        }

    }

    // �˸� �����͸� �����ϱ� ���� ������Ʈ
    private class NotificationData : MonoBehaviour
    {
        public int itemId;
        public int quantity;
    }

    // �� �˸��� ��ġ ���
    private void CalculateNotificationPosition(GameObject notification)
    {
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        float notificationHeight = rectTransform.rect.height + 10f; // ���� �߰�

        // ���� ���� �����¿� ����Ͽ� ���� �� ���� ��ġ ���
        Vector2 newStartPos = new Vector2(startPosition.x, startPosition.y - currentVerticalOffset);
        Vector2 newEndPos = new Vector2(endPosition.x, endPosition.y - currentVerticalOffset);

        // �� �˸��� ��ġ ����
        rectTransform.anchoredPosition = newStartPos;

        // �� �˸��� ���� ���� �� ���� ��ġ ���� (�ִϸ��̼ǿ��� ���)
        notification.AddComponent<NotificationPositionData>().Initialize(newStartPos, newEndPos);

        // ���� �˸��� ���� ���� ������ ������Ʈ
        currentVerticalOffset += notificationHeight;
    }

    // �˸� �ִϸ��̼� ó��
    private IEnumerator AnimateNotification(GameObject notification)
    {
        // ������Ʈ ��������
        RectTransform rectTransform = notification.GetComponent<RectTransform>();

        // TextMeshProUGUI ������Ʈ ã�� (���� �����)
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();     

        // ����� ��ġ ���� ��������
        NotificationPositionData posData = notification.GetComponent<NotificationPositionData>();
        Vector2 notifStartPos = posData.startPosition;
        Vector2 notifEndPos = posData.endPosition;

        // �����̵� �� �ִϸ��̼�
        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            if (notification == null) yield break; // �˸��� �ı��ƴٸ� �ڷ�ƾ ����

            float t = elapsedTime / slideDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(notifStartPos, notifEndPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �˸��� �ı��ƴٸ� �ڷ�ƾ ����
        if (notification == null) yield break;

        // ���� ��ġ ����
        rectTransform.anchoredPosition = notifEndPos;

        // ǥ�� ����
        yield return new WaitForSeconds(displayDuration);

        // �˸��� �ı��ƴٸ� �ڷ�ƾ ����
        if (notification == null) yield break;

        // ���̵� �ƿ�
        elapsedTime = 0;
        bool isDestroyed = false;

        while (elapsedTime < fadeOutDuration && !isDestroyed)
        {
            // �ı� ���� Ȯ��
            if (notification == null)
            {
                yield break; // �˸��� �ı��ƴٸ� �ڷ�ƾ ����
            }

            float t = elapsedTime / fadeOutDuration;
           

            // �����ϰ� �ؽ�Ʈ ���İ� ����
            if (notificationText != null && notificationText.gameObject != null)
            {
                try
                {
                    Color textColor = notificationText.color;
                    notificationText.color = new Color(textColor.r, textColor.g, textColor.b);
                }
                catch (MissingReferenceException)
                {
                    // �����ϰ� ��� ����
                }
            }
    
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // �˸��� �ı��ƴٸ� �ڷ�ƾ ����
        if (notification == null) yield break;

        // �˸� ���� �� ����
        RemoveNotification(notification, rectTransform);
    }

    // �˸��� �����ϰ� �����ϴ� ������ �޼���
    private void RemoveNotification(GameObject notification, RectTransform rectTransform)
    {
        try
        {
            // Ȱ�� �˸� ��Ͽ��� ����
            if (activeNotifications.Contains(notification))
            {
                activeNotifications = new Queue<GameObject>(
                    new List<GameObject>(activeNotifications).FindAll(n => n != notification)
                );
            }

            // �˸� ���̸�ŭ currentVerticalOffset ���� (null üũ �߰�)
            if (rectTransform != null)
            {
                float height = rectTransform.rect.height + 10f;
                currentVerticalOffset -= height;
                if (currentVerticalOffset < 0) currentVerticalOffset = 0;
            }

            // �ٸ� �˸����� ��ġ ������
            AdjustNotificationsPosition();

            // �����ϰ� �ı�
            if (notification != null)
            {
                Destroy(notification);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�˸� ���� �� ����: {e.Message}");
        }
    }

    // Ȱ��ȭ�� ��� �˸��� ��ġ ����
    private void AdjustNotificationsPosition()
    {
        try
        {
            // ��ȿ�� �˸��� ���͸�
            List<GameObject> validNotifications = new List<GameObject>();
            foreach (GameObject notification in activeNotifications)
            {
                if (notification != null)
                {
                    validNotifications.Add(notification);
                }
            }

            // Ȱ�� �˸� ��� ������Ʈ
            if (validNotifications.Count != activeNotifications.Count)
            {
                activeNotifications = new Queue<GameObject>(validNotifications);
            }

            // ���� ���� ������ �缳��
            currentVerticalOffset = 0;

            // �� �˸��� ���̸� ������� ��ġ ����
            foreach (GameObject notification in validNotifications)
            {
                try
                {
                    RectTransform rectTransform = notification.GetComponent<RectTransform>();
                    NotificationPositionData posData = notification.GetComponent<NotificationPositionData>();

                    if (posData == null || rectTransform == null) continue;

                    // ���� ���
                    float height = rectTransform.rect.height + 10f; // ���� �߰�

                    // �� ���� ��ġ ���
                    Vector2 newEndPos = new Vector2(endPosition.x, endPosition.y - currentVerticalOffset);

                    // ��ġ ������Ʈ
                    rectTransform.anchoredPosition = newEndPos;

                    // ��ġ ������ ������Ʈ
                    posData.endPosition = newEndPos;

                    // ���� �˸��� ���� ������ ����
                    currentVerticalOffset += height;
                }
                catch (MissingReferenceException)
                {
                    // �̹� �ı��� ��ü��� �ǳʶٱ�
                    Debug.LogWarning("AdjustNotificationsPosition: �̹� �ı��� �˸� ��ü�� �߰��߽��ϴ�.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"�˸� ��ġ ���� �� ����: {e.Message}");
        }
    }

    // �˸��� ��ġ �����͸� �����ϱ� ���� ������Ʈ
    private class NotificationPositionData : MonoBehaviour
    {
        public Vector2 startPosition;
        public Vector2 endPosition;

        public void Initialize(Vector2 start, Vector2 end)
        {
            startPosition = start;
            endPosition = end;
        }
    }

    // �׽�Ʈ�� �޼��� (�����Ϳ��� �׽�Ʈ�� �� ���)
}