using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 씬 이름 상수
    private const string LOBBY_SCENE = "Lobby";
    private const string VILLAGE_SCENE = "Village";

    [SerializeField] private List<GameObject> toggleableUIElements;
    private List<GameObject> activeUIElements = new List<GameObject>();

    // 게임 플레이 시간 추적
    private float sessionStartTime;
    private float totalPlayTime = 0f;

    // 스크린샷 캡처 해상도
    [SerializeField] private int screenshotWidth = 256;
    [SerializeField] private int screenshotHeight = 192;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 세션 시작 시간 기록
            sessionStartTime = Time.time;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activeUIElements.Count > 0)
            {
                // 역순으로 활성화된 UI 비활성화
                GameObject lastActiveUI = activeUIElements[activeUIElements.Count - 1];
                lastActiveUI.SetActive(false);
                activeUIElements.Remove(lastActiveUI);
            }
            else
            {
                // 활성화된 UI가 없을 때 ESC를 누르면 게임 일시정지 메뉴 표시 등
                //ShowPauseMenu();
            }
        }
    }

    // 각 UI 요소의 활성화 상태가 변경될 때 호출
    public void OnUIStateChanged(GameObject uiElement, bool isActive)
    {
        if (isActive)
        {
            if (!activeUIElements.Contains(uiElement))
                activeUIElements.Add(uiElement);
        }
        else
        {
            activeUIElements.Remove(uiElement);
        }
    }

    // 새 게임 시작
    public void StartNewGame()
    {
        // 로딩 화면 표시
        if (LoadingScreen.Instance != null)
        {
            // 새 게임은 Village 씬으로 시작
            LoadingScreen.Instance.ShowLoading(VILLAGE_SCENE, () => {
                // 필요한 초기화 로직
                Debug.Log("새 게임 시작 완료");

                // 스크린샷 캡처 (필요시)
                CaptureScreenshot();

                // 게임 데이터 초기 저장
                SaveGameData();
            });
        }
        else
        {
            // 로딩 화면이 없는 경우 직접 씬 로드
            SceneManager.LoadScene(VILLAGE_SCENE);
        }
    }

    // 게임 로드
    public void LoadGameScene()
    {
        // 로딩 화면 표시
        if (LoadingScreen.Instance != null)
        {
            // 게임 불러오기는 Village 씬으로 이동
            LoadingScreen.Instance.ShowLoading(VILLAGE_SCENE, () => {
                // 필요한 초기화 로직
                Debug.Log("게임 로드 완료");

                // 게임 데이터 적용
                ApplyGameData();
            });
        }
        else
        {
            // 로딩 화면이 없는 경우 직접 씬 로드
            SceneManager.LoadScene(VILLAGE_SCENE);
        }
    }

    // 현재 화면 스크린샷 캡처
    public void CaptureScreenshot()
    {
        StartCoroutine(CaptureScreenshotCoroutine());
    }

    private IEnumerator CaptureScreenshotCoroutine()
    {
        // 한 프레임 대기하여 UI가 완전히 렌더링되도록 함
        yield return new WaitForEndOfFrame();

        // 현재 슬롯 번호 가져오기
        int currentSlot = SaveManager.Instance.GetCurrentSlot();

        // 스크린샷 경로 생성
        string screenshotDir = System.IO.Path.Combine(
            Application.persistentDataPath,
            "SaveFiles",
            $"Slot{currentSlot}"
        );

        string screenshotPath = System.IO.Path.Combine(screenshotDir, "screenshot.png");

        // 디렉토리가 없으면 생성
        if (!System.IO.Directory.Exists(screenshotDir))
        {
            System.IO.Directory.CreateDirectory(screenshotDir);
        }

        // 렌더 텍스처 생성 및 캡처
        RenderTexture rt = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        RenderTexture prevRT = RenderTexture.active;
        Camera.main.targetTexture = rt;
        RenderTexture.active = rt;
        Camera.main.Render();

        Texture2D screenShot = new Texture2D(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        screenShot.Apply();

        // 원래 렌더 타겟 복원
        Camera.main.targetTexture = null;
        RenderTexture.active = prevRT;
        Destroy(rt);

        // 스크린샷 저장
        byte[] bytes = screenShot.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenshotPath, bytes);

        Debug.Log($"스크린샷 저장 완료: {screenshotPath}");
    }

    // 게임 데이터 저장
    public void SaveGameData()
    {
        // 플레이 타임 업데이트
        UpdatePlayTime();

        // 실제 게임 데이터 저장
        SaveManager.Instance.SaveAllData();
    }

    // 플레이 타임 업데이트
    private void UpdatePlayTime()
    {
        // 현재 세션 플레이 시간 계산
        float sessionTime = Time.time - sessionStartTime;

        // 저장된 전체 플레이 시간에 현재 세션 시간 추가
        totalPlayTime += sessionTime;

        // 다음 세션을 위해 시작 시간 재설정
        sessionStartTime = Time.time;

        // 플레이 타임 데이터 업데이트 (SaveManager에 메서드 추가 필요)
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdatePlayTime((int)totalPlayTime);
        }
    }

    // 게임 데이터 적용
    private void ApplyGameData()
    {
        // PlayerClass, InventorySystem, FragmentManager 컴포넌트 찾기
        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        InventorySystem inventorySystem = FindObjectOfType<InventorySystem>();
        FragmentManager fragmentManager = FindObjectOfType<FragmentManager>();

        // 데이터 적용
        if (SaveManager.Instance != null && playerClass != null)
        {
            SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
        }
    }

    // 어플리케이션 종료 시
    private void OnApplicationQuit()
    {
        // 게임 종료 시 현재 플레이 타임 저장
        UpdatePlayTime();

        // 모든 데이터 저장
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveAllData();
        }
    }

    // 게임 일시 정지시 (씬 전환 등)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 게임이 백그라운드로 갈 때 데이터 저장
            UpdatePlayTime();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveAllData();
            }
        }
        else
        {
            // 게임이 다시 포그라운드로 돌아올 때 세션 시작 시간 재설정
            sessionStartTime = Time.time;
        }
    }
}