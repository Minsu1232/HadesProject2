using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System.IO;

// 다이얼로그 시스템 - 선형적 구조
public class DialogSystem : MonoBehaviour
{
    #region 열거형 및 데이터 구조

    // 화자 타입 열거형
    public enum SpeakerType
    {
        Default,
        Timekeeper,  // 타임키퍼
        Guardian,    // 수호자
        Alexander,   // 알렉산더
        Enemy,       // 적
        NPC,         // 일반 NPC
        System       // 시스템 메시지
    }

    // 대화 모드 열거형
    public enum DialogMode
    {
        StopGame,      // 게임 정지 (시간 멈춤, 플레이어 이동/회전 불가)
        AllowRotation, // 회전만 허용 (시간 정상, 이동 불가, 회전 가능)
        AllowAll,      // 모두 허용 (시간 정상, 이동/회전 가능)
        Cinematic      // 시네마틱 모드 (카메라 조작 및 특수 연출 가능)
    }

    [System.Serializable]
    public class SpeakerData
    {
        public string speakerName;
        public SpeakerType speakerType;
        [Tooltip("타입보다 우선적용됩니다. 비워두면 타입 기본색 사용")]
        public Color customColor = Color.clear; // 빈 색상은 타입 기본색 사용
    }

    [System.Serializable]
    public class DialogLine
    {
        public string speakerName;
        public string dialogText;
        public string portraitName;
        public string eventTrigger; // 이 대화 후 발생할 이벤트
    }

    [System.Serializable]
    public class DialogSequence
    {
        public string dialogID;
        public DialogLine[] lines;
        public DialogMode dialogMode = DialogMode.StopGame; // 기본값
    }

    [System.Serializable]
    public class DialogWrapper
    {
        public DialogSequence[] dialogSequences;
    }

    #endregion

    #region 싱글톤 및 전역 변수

    // 싱글톤 인스턴스
    public static DialogSystem Instance { get; private set; }

    // 이벤트 위임자
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

    #region 인스펙터 변수

    // UI 요소들
    [SerializeField] private GameObject dialogPanel;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogText;
    [SerializeField] private Button continueButton;

    // 대화 애니메이션 설정
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private AudioSource typingSoundEffect;

    // 모든 대화 시퀀스를 저장
    [SerializeField] private DialogSequence[] allDialogs;

    [Header("화자 설정")]
    [SerializeField] private List<SpeakerData> speakers = new List<SpeakerData>();
    [SerializeField] private bool useCustomColorsOverType = true; // 커스텀 색상 우선 사용 여부

    // 화자 타입별 기본 색상
    [Header("타입별 기본 색상")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color timekeeperColor = Color.white;
    [SerializeField] private Color guardianColor = new Color(1f, 0.84f, 0f); // 금색
    [SerializeField] private Color alexanderColor = new Color(0.9f, 0.2f, 0.2f); // 붉은색
    [SerializeField] private Color enemyColor = new Color(0.7f, 0.1f, 0.1f); // 진한 붉은색
    [SerializeField] private Color npcColor = new Color(0.5f, 0.8f, 1f); // 하늘색
    [SerializeField] private Color systemColor = new Color(0.7f, 0.7f, 0.7f); // 회색

    #endregion

    #region 초기화 및 생명주기 메서드

    private void Awake()
    {
        // 싱글톤 패턴 구현
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

        // 초기 상태 설정
        dialogPanel.SetActive(false);

        // 계속 버튼 이벤트 설정
        continueButton.onClick.AddListener(DisplayNextLine);
    }

    #endregion

    #region 화자 및 색상 관리

    private void InitializeSpeakerData()
    {
        speakerDataMap.Clear();

        // 기본 화자 데이터 추가
        AddDefaultSpeaker("타임키퍼", SpeakerType.Timekeeper);
        AddDefaultSpeaker("수호자", SpeakerType.Guardian);
        AddDefaultSpeaker("알렉산더", SpeakerType.Alexander);
        AddDefaultSpeaker("???", SpeakerType.Default);
        AddDefaultSpeaker("주인공", SpeakerType.Guardian);

        // 설정된 화자 데이터 추가
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

    // 화자 타입에 따른 색상 가져오기
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

    // 화자 이름으로 색상 가져오기
    private Color GetColorForSpeaker(string speakerName)
    {
        if (speakerDataMap.TryGetValue(speakerName, out SpeakerData data))
        {
            // 커스텀 색상이 설정되어 있고 우선 사용 옵션이 켜져 있으면 커스텀 색상 사용
            if (useCustomColorsOverType && data.customColor != Color.clear)
            {
                return data.customColor;
            }
            // 아니면 타입 기본 색상 사용
            return GetColorForSpeakerType(data.speakerType);
        }

        // 등록되지 않은 화자는 기본 색상 반환
        return defaultColor;
    }

    // 새 화자 추가/업데이트 (런타임에서 사용 가능)
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

    #region 다이얼로그 파일 로딩

    private void LoadDialogsFromStreamingAssets()
    {
        string filePath = Path.Combine(Application.streamingAssetsPath, "Dialogs/dialogs.json");

        // Windows, Mac, iOS 등 직접 파일 읽기 가능한 플랫폼
        if (File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            ParseDialogJson(jsonContent);
        }
        else
        {
            Debug.LogWarning($"dialogs.json 파일을 찾을 수 없습니다: {filePath}");
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
            Debug.LogWarning("JSON 파싱 실패 또는 데이터 없음.");
        }
    }

    // 외부에서 대화를 JSON으로 로드하는 함수
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

    #region 다이얼로그 표시 및 제어

    // 진행 상태에 따른 다이얼로그 표시
    public void StartDialogIfNotShown(string dialogID)
    {
        if (!GameProgressManager.Instance.IsDialogShown(dialogID))
        {
            StartDialog(dialogID);
            GameProgressManager.Instance.MarkDialogAsShown(dialogID);
        }
    }

    // 대화 시작
    public void StartDialog(string dialogID)
    {
        // 이미 대화 중이면 종료
        if (isDialogActive)
        {
            EndDialog();
        }
        // 현재 다이얼로그 ID 설정
        currentDialogID = dialogID;
        // 요청된 대화 시퀀스 확인
        if (!dialogDictionary.ContainsKey(dialogID))
        {
            Debug.LogWarning($"Dialog ID not found: {dialogID}");
            return;
        }

        // 현재 대화 설정
        DialogSequence sequence = dialogDictionary[dialogID];
        currentDialog.Clear();

        // 대화 모드 설정
        currentDialogMode = sequence.dialogMode;
        originalTimeScale = Time.timeScale;

        // 게임 제어 적용
        ApplyGameControl(false);

        // 모든 대화 라인을 큐에 추가
        foreach (DialogLine line in sequence.lines)
        {
            currentDialog.Enqueue(line);
        }

        // 대화 패널 활성화 및 첫 대화 표시
        dialogPanel.SetActive(true);
        isDialogActive = true;
        DisplayNextLine();
    }

    // 다음 대화 라인 표시
    public void DisplayNextLine()
    {
        // 텍스트 애니메이션 중이면 즉시 완료
        if (isDisplayingText)
        {
            StopAllCoroutines();
            dialogText.text = currentDialog.Peek().dialogText;
            isDisplayingText = false;
            continueButton.gameObject.SetActive(true);
            return;
        }

        // 더 이상 표시할 대화가 없으면 종료
        if (currentDialog.Count == 0)
        {
            EndDialog();
            return;
        }

        // 다음 대화 라인 가져오기
        DialogLine line = currentDialog.Dequeue();

        // UI 업데이트
        speakerNameText.text = line.speakerName;

        // 화자 이름에 색상 적용
        speakerNameText.color = GetColorForSpeaker(line.speakerName);

        // 텍스트 애니메이션 시작
        StartCoroutine(TypeText(line.dialogText));

        // 이벤트 트리거 처리
        if (!string.IsNullOrEmpty(line.eventTrigger))
        {
            HandleEventFromDialog(line.eventTrigger);
            OnDialogEvent?.Invoke(line.eventTrigger);
        }
    }

    // 텍스트 타이핑 효과
    private IEnumerator TypeText(string text)
    {
        isDisplayingText = true;
        dialogText.text = "";
        continueButton.gameObject.SetActive(false);

        foreach (char c in text.ToCharArray())
        {
            dialogText.text += c;

            // 타이핑 사운드 재생
            if (typingSoundEffect != null && c != ' ')
            {
                typingSoundEffect.Play();
            }

            yield return new WaitForSeconds(typingSpeed);
        }

        isDisplayingText = false;
        continueButton.gameObject.SetActive(true);
    }

    // 대화 종료
    public void EndDialog()
    {
        // 게임 제어 복원
        ApplyGameControl(true);

        dialogPanel.SetActive(false);
        isDialogActive = false;

        // 다이얼로그 완료 이벤트 발생
        if (currentDialogID != null)
        {
            string completeEvent = $"DialogComplete:{currentDialogID}";
            OnDialogEvent?.Invoke(completeEvent);
        }

        currentDialog.Clear();
        currentDialogID = null;
    }

    #endregion

    #region 게임 제어 및 상태 관리

    private void ApplyGameControl(bool restore)
    {
        // 플레이어 찾기
        PlayerMovement playerMovement = GameInitializer.Instance.gameObject.GetComponent<PlayerMovement>();

        if (restore)
        {
            // 대화 종료 - 모든 제어 복원 
            Time.timeScale = originalTimeScale;

            if (playerMovement != null)
            {
                playerMovement.SetMovementEnabled(true);
                playerMovement.SetRotationEnabled(true);
            }

            // 시네마틱 모드였다면 카메라 복원
            if (currentDialogMode == DialogMode.Cinematic && CinematicCameraController.Instance != null)
            {
                CinematicCameraController.Instance.ResetCamera();
            }
        }
        else
        {
            // 대화 시작 - 모드에 따라 제어
            switch (currentDialogMode)
            {
                case DialogMode.StopGame:
                    Time.timeScale = 0f; // 시간 멈춤

                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(false);
                    }
                    break;

                case DialogMode.AllowRotation:
                    // 시간은 정상 흐름
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(true);
                    }
                    break;

                case DialogMode.AllowAll:
                    // 모든 것 허용 (배경 대화)
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(true);
                        playerMovement.SetRotationEnabled(true);
                    }
                    break;

                case DialogMode.Cinematic:
                    // 시네마틱 모드 - 플레이어 움직임 제한
                    if (playerMovement != null)
                    {
                        playerMovement.SetMovementEnabled(false);
                        playerMovement.SetRotationEnabled(false);
                    }
                    break;
            }
        }
    }

    // 플래그 설정
    public void SetFlag(string flagName, bool value)
    {
        flags[flagName] = value;
    }

    // 플래그 확인
    public bool GetFlag(string flagName)
    {
        return flags.TryGetValue(flagName, out bool value) && value;
    }

    #endregion

    #region 이벤트 처리

    // 다이얼로그 이벤트 처리를 확장
    private void HandleEventFromDialog(string eventName)
    {
        // 플래그 설정 이벤트 처리
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
        // 챕터 해금 이벤트 처리
        else if (eventName.StartsWith("UnlockChapter:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                int chapter = int.Parse(parts[1]);
                GameProgressManager.Instance.SetCurrentChapter(chapter);
            }
        }
        // 무기 해금 이벤트 처리
        else if (eventName.StartsWith("UnlockWeapon:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                string weaponID = parts[1];
                GameProgressManager.Instance.UnlockWeapon(weaponID);
            }
        }
        // 파편 획득 이벤트 처리
        else if (eventName.StartsWith("AcquireFragment:"))
        {
            string[] parts = eventName.Split(':');
            if (parts.Length >= 2)
            {
                string fragmentID = parts[1];
                GameProgressManager.Instance.AcquireFragment(fragmentID);
            }
        }

        // 기존 이벤트 처리 호출
        OnDialogEvent?.Invoke(eventName);
    }

    #endregion
}

#region 관련 유틸리티 클래스

// 이벤트 처리 예시 클래스
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
                // 게임플레이 시작 처리
                Debug.Log("게임플레이 시작");
                break;

            case "StartAlexanderFight":
                // 보스전 시작 처리
                Debug.Log("보스전 시작");
                break;

            case "ShowMemory":
                // 기억 표시 처리
                Debug.Log("기억 표시");
                break;

            case "UnlockWeapon":
                // 무기 해금 처리
                Debug.Log("무기 해금");
                break;

            default:
                Debug.Log($"이벤트 처리: {eventName}");
                break;
        }
    }
}

// 다이얼로그 트리거 컴포넌트
public class DialogTrigger : MonoBehaviour
{
    [SerializeField] private string dialogID;
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    // 플레이어가 트리거에 진입했을 때
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TriggerDialog();
        }
    }

    // 수동으로 대화 트리거
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