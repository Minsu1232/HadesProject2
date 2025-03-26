// ItemPickupNotification.cs - 아이템 획득 알림 시스템
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ItemPickupNotification : MonoBehaviour
{
    [Header("알림 설정")]
    [SerializeField] private GameObject notificationPrefab;  // 알림 UI 프리팹
    [SerializeField] private Transform notificationParent;   // 알림들이 생성될 부모 Transform
    [SerializeField] private int maxNotifications = 3;       // 최대 동시 표시 알림 수
    [SerializeField] private float stackingTime = 0.5f;      // 같은 아이템 스택킹 시간 (초)

    [Header("애니메이션 설정")]
    [SerializeField] private float displayDuration = 3.0f;    // 표시 지속 시간
    [SerializeField] private float slideDuration = 0.3f;      // 슬라이드 애니메이션 시간
    [SerializeField] private float fadeOutDuration = 0.5f;    // 페이드 아웃 시간

    [Header("위치 설정")]
    [SerializeField] private Vector2 startPosition = new Vector2(200, 0);  // 시작 위치 (화면 바깥)
    [SerializeField] private Vector2 endPosition = new Vector2(0, 0);      // 최종 위치 (화면 안)

    // 희귀도별 색상
    private readonly Color[] rarityColors = new Color[5]
    {
        new Color(1f, 1f, 1f), // Common (회색)
        new Color(0.3f, 0.7f, 0.3f), // Uncommon (녹색)
        new Color(0.3f, 0.3f, 0.9f), // Rare (파란색)
        new Color(0.7f, 0.3f, 0.9f), // Epic (보라색)
        new Color(1.0f, 0.8f, 0.0f)  // Legendary (금색)
    };

    // 활성화된 알림 관리를 위한 큐
    private Queue<GameObject> activeNotifications = new Queue<GameObject>();

    // 현재 사용 중인 수직 공간을 추적
    private float currentVerticalOffset = 0f;

    // 아이템 스택킹을 위한 사전 (아이템 ID -> 알림 정보)
    private Dictionary<int, ItemNotificationInfo> pendingNotifications = new Dictionary<int, ItemNotificationInfo>();

    // 가장 최근 알림 처리 시간
    private float lastNotificationTime = 0f;

    // 싱글톤 인스턴스
    public static ItemPickupNotification Instance { get; private set; }

    // 알림 정보를 저장하기 위한 클래스
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
        // 부모 Transform이 지정되지 않은 경우 현재 객체의 Transform 사용
        if (notificationParent == null)
        {
            notificationParent = transform;
        }
    }

    private void Start()
    {
        // 아이템 획득 이벤트를 받기 위해 ItemPickupObject 클래스 내 획득 완료 시점에 연결 필요
        // 일반적으로 ItemPickupObject의 PickupItem 메서드 안에서 호출
    }

    // 아이템 획득 알림을 표시하는 공개 메서드
    public void ShowNotification(Item item, int quantity)
    {
        if (notificationPrefab == null)
        {
            Debug.LogError("알림 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 현재 시간 저장
        float currentTime = Time.time;

        // 스택킹 가능한지 확인 (같은 종류의 아이템이 최근에 획득되었는지)
        if (TryStackNotification(item, quantity, currentTime))
        {
            // 성공적으로 스택킹되었으면 새 알림 생성하지 않음
            return;
        }

        // 스택킹 로직 실행 (특정 시간동안 아이템 획득 모으기)
        StartCoroutine(ProcessNotification(item, quantity));

        // 마지막 알림 시간 업데이트
        lastNotificationTime = currentTime;
    }

    // 같은 아이템 스택킹 시도
    private bool TryStackNotification(Item item, int quantity, float currentTime)
    {
        // 이미 활성화된 알림 중에서 같은 아이템을 찾음
        foreach (GameObject notification in activeNotifications)
        {
            NotificationData data = notification.GetComponent<NotificationData>();
            if (data != null && data.itemId == item.itemID)
            {
                // 같은 아이템 알림이 이미 표시되고 있는 경우, 수량 업데이트
                data.quantity += quantity;

                // 텍스트 업데이트
                TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
                if (notificationText != null)
                {
                    notificationText.text = $"{item.itemName} x{data.quantity} 획득";
                }

                // 스케일 애니메이션으로 수량 증가 표시
                StartCoroutine(PulseAnimation(notification));

                return true;
            }
        }

        return false;
    }

    // 알림 처리 코루틴 (일정 시간 동안 같은 아이템 획득 모으기)
    private IEnumerator ProcessNotification(Item item, int quantity)
    {
        int itemId = item.itemID;

        // 아직 처리 중인 아이템인지 확인
        if (pendingNotifications.ContainsKey(itemId))
        {
            // 이미 같은 아이템이 대기 중이면 수량만 증가
            pendingNotifications[itemId].quantity += quantity;
            yield break;
        }

        // 새 아이템 정보 추가
        pendingNotifications.Add(itemId, new ItemNotificationInfo(item, quantity));

        // 스택킹 시간 동안 대기 (같은 아이템이 더 들어올 수 있음)
        yield return new WaitForSeconds(stackingTime);

        // 모은 정보로 실제 알림 생성
        if (pendingNotifications.TryGetValue(itemId, out ItemNotificationInfo info))
        {
            CreateNotification(info.item, info.quantity);
            pendingNotifications.Remove(itemId);
        }
    }

    // 실제 알림 UI 생성
    private void CreateNotification(Item item, int quantity)
    {
        // 새 알림 생성
        GameObject notification = Instantiate(notificationPrefab, notificationParent);

        // 알림 데이터 컴포넌트 추가
        NotificationData data = notification.AddComponent<NotificationData>();
        data.itemId = item.itemID;
        data.quantity = quantity;

        // 알림 UI 설정
        SetupNotificationUI(notification, item, quantity);

        // 알림 위치 계산 (이미 존재하는 알림들의 위치를 고려)
        CalculateNotificationPosition(notification);

        // 알림 애니메이션 시작
        StartCoroutine(AnimateNotification(notification));

        // 활성 알림 관리
        activeNotifications.Enqueue(notification);

        // 최대 개수 초과 시 오래된 알림 제거
        if (activeNotifications.Count > maxNotifications)
        {
            GameObject oldNotification = activeNotifications.Dequeue();
            Destroy(oldNotification);
        }
    }

    // 수량 증가 시 알림 펄스 애니메이션
    private IEnumerator PulseAnimation(GameObject notification)
    {
        RectTransform rect = notification.GetComponent<RectTransform>();
        Vector3 originalScale = rect.localScale;
        Vector3 targetScale = originalScale * 1.2f;

        // 커지는 애니메이션
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 크기로 돌아오는 애니메이션
        elapsed = 0f;
        while (elapsed < duration)
        {
            rect.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.localScale = originalScale;
    }

    // 알림 UI 요소 설정
    private void SetupNotificationUI(GameObject notification, Item item, int quantity)
    {
        // 텍스트 컴포넌트 찾기
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
        if (notificationText == null) return;

        // 아이템 이름 + 수량 설정
        string quantityText = quantity > 1 ? $" x{quantity}" : "";
        notificationText.text = $"{item.itemName}{quantityText} 획득";

        // 희귀도에 따른 색상 설정
        int rarityIndex = (int)item.rarity;
        if (rarityIndex >= 0 && rarityIndex < rarityColors.Length)
        {
            notificationText.color = rarityColors[rarityIndex];
        }

    }

    // 알림 데이터를 저장하기 위한 컴포넌트
    private class NotificationData : MonoBehaviour
    {
        public int itemId;
        public int quantity;
    }

    // 새 알림의 위치 계산
    private void CalculateNotificationPosition(GameObject notification)
    {
        RectTransform rectTransform = notification.GetComponent<RectTransform>();
        float notificationHeight = rectTransform.rect.height + 10f; // 여백 추가

        // 현재 수직 오프셋에 기반하여 시작 및 종료 위치 계산
        Vector2 newStartPos = new Vector2(startPosition.x, startPosition.y - currentVerticalOffset);
        Vector2 newEndPos = new Vector2(endPosition.x, endPosition.y - currentVerticalOffset);

        // 새 알림의 위치 설정
        rectTransform.anchoredPosition = newStartPos;

        // 이 알림을 위한 시작 및 종료 위치 저장 (애니메이션에서 사용)
        notification.AddComponent<NotificationPositionData>().Initialize(newStartPos, newEndPos);

        // 다음 알림을 위한 수직 오프셋 업데이트
        currentVerticalOffset += notificationHeight;
    }

    // 알림 애니메이션 처리
    private IEnumerator AnimateNotification(GameObject notification)
    {
        // 컴포넌트 가져오기
        RectTransform rectTransform = notification.GetComponent<RectTransform>();

        // TextMeshProUGUI 컴포넌트 찾기 (색상 변경용)
        TextMeshProUGUI notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();     

        // 저장된 위치 정보 가져오기
        NotificationPositionData posData = notification.GetComponent<NotificationPositionData>();
        Vector2 notifStartPos = posData.startPosition;
        Vector2 notifEndPos = posData.endPosition;

        // 슬라이드 인 애니메이션
        float elapsedTime = 0;
        while (elapsedTime < slideDuration)
        {
            if (notification == null) yield break; // 알림이 파괴됐다면 코루틴 종료

            float t = elapsedTime / slideDuration;
            rectTransform.anchoredPosition = Vector2.Lerp(notifStartPos, notifEndPos, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 알림이 파괴됐다면 코루틴 종료
        if (notification == null) yield break;

        // 최종 위치 설정
        rectTransform.anchoredPosition = notifEndPos;

        // 표시 유지
        yield return new WaitForSeconds(displayDuration);

        // 알림이 파괴됐다면 코루틴 종료
        if (notification == null) yield break;

        // 페이드 아웃
        elapsedTime = 0;
        bool isDestroyed = false;

        while (elapsedTime < fadeOutDuration && !isDestroyed)
        {
            // 파괴 여부 확인
            if (notification == null)
            {
                yield break; // 알림이 파괴됐다면 코루틴 종료
            }

            float t = elapsedTime / fadeOutDuration;
           

            // 안전하게 텍스트 알파값 조정
            if (notificationText != null && notificationText.gameObject != null)
            {
                try
                {
                    Color textColor = notificationText.color;
                    notificationText.color = new Color(textColor.r, textColor.g, textColor.b);
                }
                catch (MissingReferenceException)
                {
                    // 무시하고 계속 진행
                }
            }
    
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 알림이 파괴됐다면 코루틴 종료
        if (notification == null) yield break;

        // 알림 제거 및 정리
        RemoveNotification(notification, rectTransform);
    }

    // 알림을 안전하게 제거하는 별도의 메서드
    private void RemoveNotification(GameObject notification, RectTransform rectTransform)
    {
        try
        {
            // 활성 알림 목록에서 제거
            if (activeNotifications.Contains(notification))
            {
                activeNotifications = new Queue<GameObject>(
                    new List<GameObject>(activeNotifications).FindAll(n => n != notification)
                );
            }

            // 알림 높이만큼 currentVerticalOffset 감소 (null 체크 추가)
            if (rectTransform != null)
            {
                float height = rectTransform.rect.height + 10f;
                currentVerticalOffset -= height;
                if (currentVerticalOffset < 0) currentVerticalOffset = 0;
            }

            // 다른 알림들의 위치 재조정
            AdjustNotificationsPosition();

            // 안전하게 파괴
            if (notification != null)
            {
                Destroy(notification);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"알림 제거 중 오류: {e.Message}");
        }
    }

    // 활성화된 모든 알림의 위치 조정
    private void AdjustNotificationsPosition()
    {
        try
        {
            // 유효한 알림만 필터링
            List<GameObject> validNotifications = new List<GameObject>();
            foreach (GameObject notification in activeNotifications)
            {
                if (notification != null)
                {
                    validNotifications.Add(notification);
                }
            }

            // 활성 알림 목록 업데이트
            if (validNotifications.Count != activeNotifications.Count)
            {
                activeNotifications = new Queue<GameObject>(validNotifications);
            }

            // 현재 수직 오프셋 재설정
            currentVerticalOffset = 0;

            // 각 알림의 높이를 기반으로 위치 조정
            foreach (GameObject notification in validNotifications)
            {
                try
                {
                    RectTransform rectTransform = notification.GetComponent<RectTransform>();
                    NotificationPositionData posData = notification.GetComponent<NotificationPositionData>();

                    if (posData == null || rectTransform == null) continue;

                    // 높이 계산
                    float height = rectTransform.rect.height + 10f; // 여백 추가

                    // 새 종료 위치 계산
                    Vector2 newEndPos = new Vector2(endPosition.x, endPosition.y - currentVerticalOffset);

                    // 위치 업데이트
                    rectTransform.anchoredPosition = newEndPos;

                    // 위치 데이터 업데이트
                    posData.endPosition = newEndPos;

                    // 다음 알림을 위한 오프셋 증가
                    currentVerticalOffset += height;
                }
                catch (MissingReferenceException)
                {
                    // 이미 파괴된 객체라면 건너뛰기
                    Debug.LogWarning("AdjustNotificationsPosition: 이미 파괴된 알림 객체를 발견했습니다.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"알림 위치 조정 중 오류: {e.Message}");
        }
    }

    // 알림의 위치 데이터를 저장하기 위한 컴포넌트
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

    // 테스트용 메서드 (에디터에서 테스트할 때 사용)
}