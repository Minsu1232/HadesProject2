using DG.Tweening;
using UnityEngine;

public class GameInitializer : Singleton<GameInitializer>
{
    [SerializeField] private PlayerClassData playerClassData;
    [SerializeField] private Transform weaponMount;        // 기존 왼손 마운트
    [SerializeField] private Transform rightWeaponMount;   // 새로 추가할 오른손 마운트

    private WeaponService weaponService;
    [SerializeField] private PlayerClass playerClass;
    private ICharacterAttack characterAttack;
    private Animator animator;

    // 인벤토리와 파편 매니저 참조 추가
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private FragmentManager fragmentManager;

    // 플레이 타임 추적 관련 변수 추가
    private float sessionStartTime;
    private float totalPlayTime = 0f;

    // 자동 저장 설정
    [Header("자동 저장 설정")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5분마다 자동 저장

    public GameObject flameWall;
    public GameObject lightningEffect;

    [Header("무기이펙트 설정")]
    public GameObject chronoLeftEffect;
    public GameObject chronoRightEffect;
    public GameObject chargeBladeWavePrefab;
    public GameObject chronoSpecialEffectPrefab;

    protected override void Awake()
    {
        base.Awake();

        // 세션 시작 시간 기록
        sessionStartTime = Time.time;

        // 저장된 플레이 타임 로드
        if (SaveManager.Instance != null)
        {
            totalPlayTime = SaveManager.Instance.GetTotalPlayTime();
        }

        InitializeComponents();
        LoadGameData();
    }

    private void Start()
    {
        // 모든 초기화 이후 0.1 후 데이터 적용
        DOVirtual.DelayedCall(0.1f, ApplySavedDataToGame);

        // 자동 저장 시작 (활성화된 경우)
        if (enableAutoSave)
        {
            InvokeRepeating("AutoSave", autoSaveInterval, autoSaveInterval);
        } 
    }

    private void InitializeComponents()
    {
        try
        {
            animator = GetComponent<Animator>();
            characterAttack = GetComponent<ICharacterAttack>();

            if (animator == null || characterAttack == null)
            {
                throw new System.Exception("Required components are missing!");
            }

            InitializeWeaponService();
            InitializePlayerClass();

            // 인벤토리와 파편 매니저 참조 확인
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<InventorySystem>();
                if (inventorySystem == null)
                {
                    Debug.LogWarning("InventorySystem을 찾을 수 없습니다.");
                }
            }

            if (fragmentManager == null)
            {
                fragmentManager = FindObjectOfType<FragmentManager>();
                if (fragmentManager == null)
                {
                    Debug.LogWarning("FragmentManager를 찾을 수 없습니다.");
                }
            }

            Debug.Log("모든 컴포넌트 초기화 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"컴포넌트 초기화 실패: {e.Message}");
        }
    }

    private void InitializeWeaponService() 
    {
        weaponService = gameObject.AddComponent<WeaponService>();
        weaponService.OnWeaponChanged += OnWeaponChanged;
        weaponService.weaponMount = weaponMount; // 기존 왼손 마운트

        // 오른손 마운트 설정
        if (rightWeaponMount != null)
        {
            weaponService.rightWeaponMount = rightWeaponMount;
        }
        else
        {
            // 없으면 Unity Inspector에서 할당하라는 경고
            Debug.LogWarning("오른손 무기 마운트(rightWeaponMount)가 할당되지 않았습니다. Unity Inspector에서 할당해주세요.");
        }
    }

    private void InitializePlayerClass()
    {
        playerClass = new PlayerClass(playerClassData, characterAttack, transform, animator);
        Debug.Log($"플레이어 클래스 초기화 완료!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: {playerClassData.name}");
    }

    private void OnWeaponChanged(WeaponManager weapon)
    {
        characterAttack?.EquipWeapon(weapon);
        playerClass?.SelectWeapon(weapon);
    }

    private void LoadGameData()
    {
        try
        {
            // SaveManager를 통해 데이터 로드 (이미 현재 활성 슬롯을 고려함)
            if (SaveManager.Instance != null)
            {
                // SaveManager가 알아서 현재 선택된 슬롯에서 데이터를 로드
                // ApplySavedDataToGame()에서 처리되므로 여기서 별도 처리 불필요
                Debug.Log("세이브 매니저를 통해 데이터 로드 준비 완료");
            }
            else
            {
                Debug.LogWarning("SaveManager가 초기화되지 않았습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 데이터 로드 준비 실패: {e.Message}");
        }
    }

    private void ApplySavedDataToGame()
    {
        // Start 메서드에서 호출하여 모든 시스템이 초기화된 후 데이터 적용
        SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
    }

    // 자동 저장 메서드
    private void AutoSave()
    {
        Debug.Log("자동 저장 실행...");
        SaveGameData();
    }

    // 플레이 타임 업데이트 메서드
    private void UpdatePlayTime()
    {
        // 현재 세션 플레이 시간 계산
        float sessionTime = Time.time - sessionStartTime;

        // 저장된 전체 플레이 시간에 현재 세션 시간 추가
        totalPlayTime += sessionTime;

        // 다음 세션을 위해 시작 시간 재설정
        sessionStartTime = Time.time;

        // 플레이 타임 데이터 업데이트
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdatePlayTime((int)totalPlayTime);
        }
    }

    // 데이터 저장 메서드
    public void SaveGameData()
    {
        // 플레이 타임 업데이트
        UpdatePlayTime();

        if (playerClass != null)
        {
            SaveManager.Instance.UpdatePlayerStats(playerClass.GetStats());
        }

        if (inventorySystem != null)
        {
            SaveManager.Instance.UpdateInventory(inventorySystem);
        }

        if (fragmentManager != null)
        {
            SaveManager.Instance.UpdateEquippedFragments(fragmentManager);
        }

        SaveManager.Instance.SaveAllData();
    }

    // 게임 일시 정지시 플레이 타임 업데이트 및 저장
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 게임이 백그라운드로 갈 때 저장
            SaveGameData();
        }
        else
        {
            // 게임이 다시 포그라운드로 돌아올 때 세션 시작 시간 재설정
            sessionStartTime = Time.time;
        }
    }

    // 게임 종료 시 데이터 저장
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // 기존 접근자 메서드들
    public WeaponService GetWeaponService() => weaponService;
    public PlayerClass GetPlayerClass() => playerClass;

    // 총 플레이 타임 가져오기 (다른 클래스에서 필요할 경우)
    public float GetTotalPlayTime() => totalPlayTime;

    private void OnDestroy()
    {
        if (weaponService != null)
        {
            weaponService.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}