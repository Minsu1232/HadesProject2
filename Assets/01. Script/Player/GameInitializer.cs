using DG.Tweening;
using UnityEngine;

public class GameInitializer : Singleton<GameInitializer>
{
    [SerializeField] private PlayerClassData playerClassData;
    [SerializeField] private Transform weaponMount;

    private WeaponService weaponService;
    [SerializeField] private PlayerClass playerClass;
    private ICharacterAttack characterAttack;
    private Animator animator;

    // 인벤토리와 파편 매니저 참조 추가
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private FragmentManager fragmentManager;


    public GameObject flameWall;
    public GameObject lightningEffect;

    protected override void Awake()
    {
        base.Awake();
        InitializeComponents();
        LoadGameData();
    }

    private void Start()
    {
        // 모든 초기화 이후 0.1 후 데이터 적용
        DOVirtual.DelayedCall(0.1f, ApplySavedDataToGame);
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
        weaponService.weaponMount = weaponMount;
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
            // 기존 방식과의 호환성 유지
            string filePath = $"{Application.persistentDataPath}/SaveFiles/playerData.json";
            DataManager.Instance.LoadPlayerDataFromJson(filePath, playerClassData);

            Debug.Log("플레이어 데이터 로드 완료");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"게임 데이터 로드 실패: {e.Message}");
        }
    }

    private void ApplySavedDataToGame()
    {
        // Start 메서드에서 호출하여 모든 시스템이 초기화된 후 데이터 적용
        SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
    }

    // 데이터 저장 메서드
    public void SaveGameData()
    {
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

    // 게임 종료 시 데이터 저장
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // 기존 접근자 메서드들
    public WeaponService GetWeaponService() => weaponService;
    public PlayerClass GetPlayerClass() => playerClass;

    private void OnDestroy()
    {
        if (weaponService != null)
        {
            weaponService.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}