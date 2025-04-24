using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static UIManager Instance { get; private set; }


    [Header("알림 시스템")]
    [SerializeField] private GameObject notificationPrefab;
    [SerializeField] private Transform notificationParent; // Canvas 내부의 알림이 표시될 위치
    [SerializeField] private float notificationDuration = 3.0f; // 알림 표시 시간
    [SerializeField] private float fadeTime = 0.5f; // 알림 페이드 인/아웃 시간
    // 열려 있는 UI 팝업들을 추적하는 스택
    private Stack<GameObject> activeUIStack = new Stack<GameObject>();
    private Queue<NotificationInfo> notificationQueue = new Queue<NotificationInfo>();
    private bool isShowingNotification = false;

    private string lastMessageContent = string.Empty;
    // 알림 정보 클래스
    #region 알림 기능
    private class NotificationInfo
    {
        public string message;
        public Color color = Color.white;
        public float duration;
        public bool hasIcon;
        public Sprite icon;
    }
    // 알림 보여주기
    public void ShowNotification(string message)
    {
        ShowNotification(message, Color.white, notificationDuration, false, null);
        Debug.Log($"공지사항!!!!!!!!{message}");
    }

    // 알림 보여주기 (색상 지정)
    public void ShowNotification(string message, Color color)
    {
        ShowNotification(message, color, notificationDuration, false, null);
        Debug.Log($"공지사항!!!!!!!!222{message}");
    }

    // 알림 보여주기 (아이콘 포함)
    public void ShowNotification(string message, Sprite icon)
    {
        ShowNotification(message, Color.white, notificationDuration, true, icon);
    }

    // 알림 보여주기 (모든 옵션)
    public void ShowNotification(string message, Color color, float duration, bool hasIcon, Sprite icon)
    {
        // 직전 메시지와 똑같은 내용인지 확인 (중복 방지)
        if (message == lastMessageContent && notificationQueue.Count > 0)
        {
            Debug.Log($"중복 알림 무시: {message}");
            return; // 중복 알림 무시
        }

        // 마지막 메시지 내용 업데이트
        lastMessageContent = message;

        NotificationInfo info = new NotificationInfo
        {
            message = message,
            color = color,
            duration = duration,
            hasIcon = hasIcon,
            icon = icon
        };

        // 큐에 알림 추가
        notificationQueue.Enqueue(info);

        // 현재 표시 중인 알림이 없으면 알림 표시 시작
        if (!isShowingNotification)
        {
            StartCoroutine(ProcessNotificationQueue());
        }
    }

    // 알림 큐 처리 코루틴
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

    // 개별 알림 표시 코루틴
    private IEnumerator ShowNotificationCoroutine(NotificationInfo info)
    {
        // 알림 프리팹이 없으면 종료
        if (notificationPrefab == null || notificationParent == null)
        {
            Debug.LogError("알림 프리팹 또는 부모 Transform이 설정되지 않았습니다.");
            yield break;
        }

        // 알림 오브젝트 생성
        GameObject notificationObj = Instantiate(notificationPrefab, notificationParent);
        RectTransform rectTransform = notificationObj.GetComponent<RectTransform>();

        // 알림 컴포넌트 찾기
        TextMeshProUGUI messageText = notificationObj.GetComponentInChildren<TextMeshProUGUI>();
        Image iconImage = notificationObj.transform.Find("Icon")?.GetComponent<Image>();
        CanvasGroup canvasGroup = notificationObj.GetComponent<CanvasGroup>();

        if (messageText == null)
        {
            Debug.LogError("알림 프리팹에 TextMeshProUGUI 컴포넌트가 없습니다.");
            Destroy(notificationObj);
            yield break;
        }

        // 알림 내용 설정
        messageText.text = info.message;
        messageText.color = info.color;

        // 아이콘 설정
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(info.hasIcon);
            if (info.hasIcon && info.icon != null)
            {
                iconImage.sprite = info.icon;
            }
        }

        // 페이드 인
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

        // 표시 유지
        yield return new WaitForSeconds(info.duration);

        // 페이드 아웃
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

        // 알림 오브젝트 제거
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
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUIStack.Count > 0)
            {
                // 열린 UI가 있으면 최상위 UI 닫기
                CloseTopUI();
            }
            else
            {
                // 열린 UI가 없으면 설정 패널 토글
                if (SettingsManager.Instance != null)
                {
                    SettingsManager.Instance.ToggleSettingsPanel();
                }
            }
        }
    }

    // UI를 활성화 스택에 등록
    public void RegisterActiveUI(GameObject uiPanel)
    {
        // 이미 스택에 있는지 확인하고 중복 등록 방지
        if (activeUIStack.Contains(uiPanel))
        {
            // 이미 스택에 있다면 최상위로 재정렬하기 위해 기존 항목 제거
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // 다시 스택에 넣기 (역순)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }
        }

        // 스택에 추가
        activeUIStack.Push(uiPanel);

        Debug.Log($"UI 등록: {uiPanel.name}, 현재 활성 UI 수: {activeUIStack.Count}");
    }

    // UI를 활성화 스택에서 제거
    public void UnregisterActiveUI(GameObject uiPanel)
    {
        // 해당 UI가 스택에 있는지 확인하고 제거
        if (activeUIStack.Contains(uiPanel))
        {
            List<GameObject> tempList = new List<GameObject>();
            while (activeUIStack.Count > 0)
            {
                GameObject ui = activeUIStack.Pop();
                if (ui != uiPanel)
                    tempList.Add(ui);
            }

            // 다시 스택에 넣기 (역순)
            for (int i = tempList.Count - 1; i >= 0; i--)
            {
                activeUIStack.Push(tempList[i]);
            }

            Debug.Log($"UI 제거: {uiPanel.name}, 현재 활성 UI 수: {activeUIStack.Count}");
        }
    }

    // UI 창 열기
    public void OpenUI(GameObject uiPanel)
    {
        // 기존에 이미 열린 상태인지 확인
        if (uiPanel.activeSelf)
            return;

        uiPanel.SetActive(true);
        RegisterActiveUI(uiPanel);
    }

    // 최상위 UI 닫기
    public void CloseTopUI()
    {
        if (activeUIStack.Count > 0)
        {
            GameObject topUI = activeUIStack.Pop();
            topUI.SetActive(false);
            Debug.Log($"최상위 UI 닫기: {topUI.name}, 남은 활성 UI 수: {activeUIStack.Count}");
        }
    }

    // 특정 UI 닫기
    public void CloseUI(GameObject uiPanel)
    {
        if (uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
            UnregisterActiveUI(uiPanel);
        }
    }

    // 모든 UI 닫기
    public void CloseAllUI()
    {
        while (activeUIStack.Count > 0)
        {
            GameObject ui = activeUIStack.Pop();
            ui.SetActive(false);
        }
        Debug.Log("모든 UI 닫기 완료");
    }

    // 현재 활성화된 UI 목록 조회 (디버깅용)
    public List<GameObject> GetActiveUIs()
    {
        return new List<GameObject>(activeUIStack);
    }
}