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

    // �κ��丮�� ���� �Ŵ��� ���� �߰�
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
        // ��� �ʱ�ȭ ���� 0.1 �� ������ ����
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

            // �κ��丮�� ���� �Ŵ��� ���� Ȯ��
            if (inventorySystem == null)
            {
                inventorySystem = FindObjectOfType<InventorySystem>();
                if (inventorySystem == null)
                {
                    Debug.LogWarning("InventorySystem�� ã�� �� �����ϴ�.");
                }
            }

            if (fragmentManager == null)
            {
                fragmentManager = FindObjectOfType<FragmentManager>();
                if (fragmentManager == null)
                {
                    Debug.LogWarning("FragmentManager�� ã�� �� �����ϴ�.");
                }
            }

            Debug.Log("��� ������Ʈ �ʱ�ȭ �Ϸ�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"������Ʈ �ʱ�ȭ ����: {e.Message}");
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
        Debug.Log($"�÷��̾� Ŭ���� �ʱ�ȭ �Ϸ�!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!: {playerClassData.name}");
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
            // ���� ��İ��� ȣȯ�� ����
            string filePath = $"{Application.persistentDataPath}/SaveFiles/playerData.json";
            DataManager.Instance.LoadPlayerDataFromJson(filePath, playerClassData);

            Debug.Log("�÷��̾� ������ �ε� �Ϸ�");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���� ������ �ε� ����: {e.Message}");
        }
    }

    private void ApplySavedDataToGame()
    {
        // Start �޼��忡�� ȣ���Ͽ� ��� �ý����� �ʱ�ȭ�� �� ������ ����
        SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
    }

    // ������ ���� �޼���
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

    // ���� ���� �� ������ ����
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // ���� ������ �޼����
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