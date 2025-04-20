using DG.Tweening;
using UnityEngine;

public class GameInitializer : Singleton<GameInitializer>
{
    [SerializeField] private PlayerClassData playerClassData;
    [SerializeField] private Transform weaponMount;        // ���� �޼� ����Ʈ
    [SerializeField] private Transform rightWeaponMount;   // ���� �߰��� ������ ����Ʈ

    private WeaponService weaponService;
    [SerializeField] private PlayerClass playerClass;
    private ICharacterAttack characterAttack;
    private Animator animator;

    // �κ��丮�� ���� �Ŵ��� ���� �߰�
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private FragmentManager fragmentManager;

    // �÷��� Ÿ�� ���� ���� ���� �߰�
    private float sessionStartTime;
    private float totalPlayTime = 0f;

    // �ڵ� ���� ����
    [Header("�ڵ� ���� ����")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 300f; // 5�и��� �ڵ� ����

    public GameObject flameWall;
    public GameObject lightningEffect;

    [Header("��������Ʈ ����")]
    public GameObject chronoLeftEffect;
    public GameObject chronoRightEffect;
    public GameObject chargeBladeWavePrefab;
    public GameObject chronoSpecialEffectPrefab;

    protected override void Awake()
    {
        base.Awake();

        // ���� ���� �ð� ���
        sessionStartTime = Time.time;

        // ����� �÷��� Ÿ�� �ε�
        if (SaveManager.Instance != null)
        {
            totalPlayTime = SaveManager.Instance.GetTotalPlayTime();
        }

        InitializeComponents();
        LoadGameData();
    }

    private void Start()
    {
        // ��� �ʱ�ȭ ���� 0.1 �� ������ ����
        DOVirtual.DelayedCall(0.1f, ApplySavedDataToGame);

        // �ڵ� ���� ���� (Ȱ��ȭ�� ���)
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
        weaponService.weaponMount = weaponMount; // ���� �޼� ����Ʈ

        // ������ ����Ʈ ����
        if (rightWeaponMount != null)
        {
            weaponService.rightWeaponMount = rightWeaponMount;
        }
        else
        {
            // ������ Unity Inspector���� �Ҵ��϶�� ���
            Debug.LogWarning("������ ���� ����Ʈ(rightWeaponMount)�� �Ҵ���� �ʾҽ��ϴ�. Unity Inspector���� �Ҵ����ּ���.");
        }
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
            // SaveManager�� ���� ������ �ε� (�̹� ���� Ȱ�� ������ �����)
            if (SaveManager.Instance != null)
            {
                // SaveManager�� �˾Ƽ� ���� ���õ� ���Կ��� �����͸� �ε�
                // ApplySavedDataToGame()���� ó���ǹǷ� ���⼭ ���� ó�� ���ʿ�
                Debug.Log("���̺� �Ŵ����� ���� ������ �ε� �غ� �Ϸ�");
            }
            else
            {
                Debug.LogWarning("SaveManager�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"���� ������ �ε� �غ� ����: {e.Message}");
        }
    }

    private void ApplySavedDataToGame()
    {
        // Start �޼��忡�� ȣ���Ͽ� ��� �ý����� �ʱ�ȭ�� �� ������ ����
        SaveManager.Instance.ApplyGameData(playerClass, inventorySystem, fragmentManager);
    }

    // �ڵ� ���� �޼���
    private void AutoSave()
    {
        Debug.Log("�ڵ� ���� ����...");
        SaveGameData();
    }

    // �÷��� Ÿ�� ������Ʈ �޼���
    private void UpdatePlayTime()
    {
        // ���� ���� �÷��� �ð� ���
        float sessionTime = Time.time - sessionStartTime;

        // ����� ��ü �÷��� �ð��� ���� ���� �ð� �߰�
        totalPlayTime += sessionTime;

        // ���� ������ ���� ���� �ð� �缳��
        sessionStartTime = Time.time;

        // �÷��� Ÿ�� ������ ������Ʈ
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.UpdatePlayTime((int)totalPlayTime);
        }
    }

    // ������ ���� �޼���
    public void SaveGameData()
    {
        // �÷��� Ÿ�� ������Ʈ
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

    // ���� �Ͻ� ������ �÷��� Ÿ�� ������Ʈ �� ����
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // ������ ��׶���� �� �� ����
            SaveGameData();
        }
        else
        {
            // ������ �ٽ� ���׶���� ���ƿ� �� ���� ���� �ð� �缳��
            sessionStartTime = Time.time;
        }
    }

    // ���� ���� �� ������ ����
    private void OnApplicationQuit()
    {
        SaveGameData();
    }

    // ���� ������ �޼����
    public WeaponService GetWeaponService() => weaponService;
    public PlayerClass GetPlayerClass() => playerClass;

    // �� �÷��� Ÿ�� �������� (�ٸ� Ŭ�������� �ʿ��� ���)
    public float GetTotalPlayTime() => totalPlayTime;

    private void OnDestroy()
    {
        if (weaponService != null)
        {
            weaponService.OnWeaponChanged -= OnWeaponChanged;
        }
    }
}