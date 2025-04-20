//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class DungeonReturnManager : MonoBehaviour
//{
//    [Header("참조")]
//    [SerializeField] private DungeonManager dungeonManager;
//    [SerializeField] private GameObject returnButtonPrefab;
//    [SerializeField] private Transform canvasTransform;

//    [Header("귀환 설정")]
//    [SerializeField] private float autoReturnDelay = 30f; // 자동 귀환 시간
//    [SerializeField] private float checkItemsRadius = 20f; // 아이템 검사 범위

//    private GameObject activeReturnButton;
//    private bool isDialogActive = false;
//    private bool isReturning = false;
//    private Coroutine autoReturnCoroutine;

//    private void Awake()
//    {
//        // 캔버스 참조 얻기 (없으면 찾기)
//        if (canvasTransform == null)
//        {
//            Canvas mainCanvas = FindObjectOfType<Canvas>();
//            if (mainCanvas != null)
//            {
//                canvasTransform = mainCanvas.transform;
//            }
//        }

//        // 던전 매니저 참조 얻기 (없으면 찾기)
//        if (dungeonManager == null)
//        {
//            dungeonManager = FindObjectOfType<DungeonManager>();
//        }
//    }
//    private void OnEnable()
//    {
//        // 다이얼로그 이벤트 구독
//        if (DialogSystem.Instance != null)
//        {            
//            DialogSystem.OnDialogStateChanged += HandleDialogStateChanged;
//        }
//    }

//    private void OnDisable()
//    {
//        if (DialogSystem.Instance != null)
//        {            
//            DialogSystem.OnDialogStateChanged -= HandleDialogStateChanged;
//        }
//    }

//    // 다이얼로그 상태 변경 핸들러
//    private void HandleDialogStateChanged(bool isActive)
//    {
//        if (isActive)
//        {
//            // 다이얼로그 활성화 시 버튼 숨기기
//            HideReturnButton();
//        }
//        else
//        {
//            // 다이얼로그 비활성화 시 버튼 다시 표시
//            ShowReturnButtonAgain();
//        }
//    }
//    // 다이얼로그 이벤트 처리


//    // 귀환 버튼 표시
//    public void ShowReturnButton()
//    {
//        if (returnButtonPrefab == null || activeReturnButton != null || isReturning)
//            return;

//        // 버튼 생성
//        activeReturnButton = Instantiate(returnButtonPrefab, canvasTransform);

//        // 버튼 클릭 이벤트 연결
//        Button button = activeReturnButton.GetComponentInChildren<Button>();
//        if (button != null)
//        {
//            button.onClick.AddListener(ReturnToVillage);
//        }       

//        // 자동 귀환 타이머 시작
//        StartAutoReturnTimer();
//    }

//    // 필드에 남은 아이템 확인


//    // 자동 귀환 타이머 시작
//    private void StartAutoReturnTimer()
//    {
//        // 이미 실행 중인 타이머 정지
//        if (autoReturnCoroutine != null)
//        {
//            StopCoroutine(autoReturnCoroutine);
//        }

//        // 새 타이머 시작
//        autoReturnCoroutine = StartCoroutine(AutoReturnCoroutine());
//    }

//    // 자동 귀환 코루틴
//    private IEnumerator AutoReturnCoroutine()
//    {
//        float remainingTime = autoReturnDelay;

//        // 30초, 15초, 5초 전에 알림
//        while (remainingTime > 0 && !isReturning)
//        {
//            // 특정 시간대에 알림 표시
//            if (remainingTime <= 30 && remainingTime > 29 ||
//                remainingTime <= 15 && remainingTime > 14 ||
//                remainingTime <= 5 && remainingTime > 4)
//            {
//                if (UIManager.Instance != null)
//                {
//                    UIManager.Instance.ShowNotification($"{Mathf.FloorToInt(remainingTime)}초 후 자동으로 마을로 귀환합니다", Color.white);
//                }
//            }

//            yield return new WaitForSeconds(1f);
//            remainingTime -= 1f;

//            // 정기적으로 아이템 확인
//            if (remainingTime % 10 == 0)
//            {
//                CheckRemainingItems();
//            }
//        }

//        // 타이머 종료 시 귀환
//        if (!isReturning)
//        {
//            ReturnToVillage();
//        }
//    }

//    // 마을로 귀환 처리
//    public void ReturnToVillage()
//    {
//        if (isReturning || dungeonManager == null) return;

//        isReturning = true;

//        // 버튼 상태 변경
//        if (activeReturnButton != null)
//        {
//            Button button = activeReturnButton.GetComponentInChildren<Button>();
//            if (button != null)
//            {
//                button.interactable = false;

//                TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
//                if (buttonText != null)
//                {
//                    buttonText.text = "귀환 중...";
//                }
//            }
//        }

//        // 던전 매니저의 마을 귀환 메서드 호출
//        StartCoroutine(ReturnToVillageProcess());
//    }

//    // 마을 귀환 프로세스
//    private IEnumerator ReturnToVillageProcess()
//    {
//        // 화면 페이드 효과
//        if (SceneTransitionManager.Instance != null)
//        {
//            SceneTransitionManager.Instance.FadeIn();
//        }

//        // 페이드 효과를 위한 짧은 대기
//        yield return new WaitForSeconds(0.5f);

//        // 버튼 제거
//        if (activeReturnButton != null)
//        {
//            Destroy(activeReturnButton);
//            activeReturnButton = null;
//        }

//        // 던전 매니저의 ReturnToVillage는 async Task이므로
//        // 직접 호출하고 코루틴을 종료합니다
//        dungeonManager.ReturnToVillage();

//        // 이 코루틴 이후의 코드는 실행되지 않습니다
//        // Scene 전환으로 인해 오브젝트가 파괴될 것이기 때문입니다
//    }

//    // 버튼 숨기기 (다이얼로그 시작 시 호출)
//    public void HideReturnButton()
//    {
//        if (activeReturnButton != null)
//        {
//            activeReturnButton.SetActive(false);
//        }

//        // 자동 귀환 타이머 일시 중지
//        if (autoReturnCoroutine != null)
//        {
//            StopCoroutine(autoReturnCoroutine);
//            autoReturnCoroutine = null;
//        }
//    }

//    // 버튼 다시 표시 (다이얼로그 종료 시 호출)
//    public void ShowReturnButtonAgain()
//    {
//        if (activeReturnButton != null)
//        {
//            activeReturnButton.SetActive(true);

//            // 자동 귀환 타이머 재시작
//            StartAutoReturnTimer();
//        }
//    }
//}