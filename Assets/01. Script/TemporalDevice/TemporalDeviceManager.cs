using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

// ȿ�� Ÿ�� ������
public enum EffectType
{
    NONE,
    RARE_ABILITY_CHANCE, // ù ��° ��ġ ȿ���� ����
    ABILITY_SELECTION_COUNT,
    EXTRA_REWARD_CHANCE
}

// �ð� ��ġ Ŭ����
[System.Serializable]
public class TemporalDevice
{
    public int ID;
    public string DeviceName;
    public string Description;
    public int TimeCrystalCost;
    public bool IsUnlocked;
    public string IconKey;
    public EffectType EffectType;
    public float EffectValue;
    public int UnlockRequirement;

    // 3D ������Ʈ ����
    [HideInInspector]
    public GameObject DeviceObject;
}

// �ð� ��ġ ������
public class TemporalDeviceManager : MonoBehaviour
{
    public static TemporalDeviceManager Instance { get; private set; }

    [SerializeField] private List<TemporalDevice> allDevices = new List<TemporalDevice>();
    [SerializeField] private Transform devicesParent; // ��ġ���� �θ� Transform

    private Dictionary<int, TemporalDevice> deviceDictionary = new Dictionary<int, TemporalDevice>();
    private InventorySystem inventorySystem;
    private SaveManager saveManager;

    // �ð� ���� �������� ID (�κ��丮���� ���Ǵ�)
    [SerializeField] private int timeCrystalItemID = 3001;

    // ��ġ �ر� �̺�Ʈ
    public event Action<TemporalDevice> OnDeviceUnlocked;

    public event Action OnLoadedDebice;

    [SerializeField] private SimpleTemporalDeviceUI ui;
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

    private void Start()
    {
        // �κ��丮 �ý��۰� ���̺� �Ŵ��� ���� ��������
        inventorySystem = InventorySystem.Instance;
        saveManager = SaveManager.Instance;

        if (inventorySystem == null)
        {
            Debug.LogError("InventorySystem not found!");
        }

        if (saveManager == null)
        {
            Debug.LogError("SaveManager not found!");
        }
       
        InitializeDevices();
        // 3D ������Ʈ ��������
       

        // ��� �رݵ� ��ġ ȿ�� ����
        ApplyAllUnlockedDeviceEffects();



        SetupDeviceObjects();

       

    }

    private void InitializeDevices()
    {
        // CSV ���Ͽ��� ��ġ ������ �ε�
        LoadDevicesFromCSV();

        // ����� �ر� ���� �ҷ�����
        LoadUnlockStatus();

        // Dictionary �ʱ�ȭ
        foreach (var device in allDevices)
        {
            deviceDictionary[device.ID] = device;
        }
    }

    private void LoadDevicesFromCSV()
    {
        string csvPath = Path.Combine(Application.streamingAssetsPath, "TemporalDevices.csv");

        if (!File.Exists(csvPath))
        {
            Debug.LogError($"TemporalDevices.csv not found at path: {csvPath}");
            return;
        }

        string[] lines = File.ReadAllLines(csvPath);
        bool isFirstLine = true;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (isFirstLine)
            {
                isFirstLine = false;
                continue; // ��� ���� ��ŵ
            }

            string[] values = line.Split(',');
            if (values.Length < 9) continue;

            TemporalDevice device = new TemporalDevice
            {
                ID = int.Parse(values[0]),
                DeviceName = values[1],
                Description = values[2],
                TimeCrystalCost = int.Parse(values[3]),
                IsUnlocked = bool.Parse(values[4]),
                IconKey = values[5],
                EffectType = ParseEffectType(values[6]),
                EffectValue = float.Parse(values[7]),
                UnlockRequirement = int.Parse(values[8])
            };

            allDevices.Add(device);
        }
        ui.InitializeDeviceButtons();
        Debug.Log($"Loaded {allDevices.Count} temporal devices from CSV");
    }

    private EffectType ParseEffectType(string effectTypeStr)
    {
        if (Enum.TryParse(effectTypeStr, out EffectType result))
        {
            return result;
        }
        return EffectType.NONE;
    }

    private void LoadUnlockStatus() 
    {
        if (saveManager == null) return;

        var savedData = saveManager.GetDeviceUnlockStatus();
        if (savedData != null)
        {
            foreach (var device in allDevices)
            {
                if (savedData.TryGetValue(device.ID, out bool isUnlocked))
                {
                    device.IsUnlocked = isUnlocked;
                }
            }
            Debug.Log($"��ġ �ر� ���� �ε�: {savedData.Count}���� �׸�");
            foreach (var pair in savedData)
            {
                Debug.Log($"��ġ ID {pair.Key}: {(pair.Value ? "�رݵ�" : "���ر�")}");
            }

        }
    }

    // 3D ��ġ ������Ʈ ����
    private void SetupDeviceObjects()
    {
        if (devicesParent == null)
        {
            Debug.LogError("Devices parent transform not set!");
            return;
        }

        // ��ġ ������Ʈ ã��
        foreach (var device in allDevices)
        {
            Transform deviceTransform = devicesParent.Find("Device_" + device.ID);
            if (deviceTransform != null)
            {
                device.DeviceObject = deviceTransform.gameObject;
                UpdateDeviceVisuals(device);
            }
            else
            {
                Debug.LogWarning($"Device object for ID {device.ID} not found!");
            }
        }
    }

    // ��ġ �ð� ȿ�� ������Ʈ
    private void UpdateDeviceVisuals(TemporalDevice device)
    {
        if (device.DeviceObject == null) return;

        // Ȱ��ȭ ���¿� ���� �ð� ȿ�� ����
        DeviceVisualController visualController = device.DeviceObject.GetComponent<DeviceVisualController>();
        if (visualController != null)
        {
            visualController.SetActiveState(device.IsUnlocked);
        }
        else
        {
            // �⺻ Ȱ��ȭ/��Ȱ��ȭ ó��
            Renderer[] renderers = device.DeviceObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material mat = renderer.material;
                if (device.IsUnlocked)
                {
                    // Ȱ��ȭ �� ���� ��������
                    mat.color = Color.white;

                    // �߱� ȿ�� (���̽ú� �ؽ�ó�� �ִ� ���)
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.white);
                    }
                }
                else
                {
                    // ��Ȱ��ȭ �� ȸ����
                    mat.color = Color.gray;

                    // �߱� ȿ�� ����
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        // Ȱ��ȭ ��ƼŬ �ý��� ����
        ParticleSystem[] particles = device.DeviceObject.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            if (device.IsUnlocked)
            {
                if (!ps.isPlaying) ps.Play();
            }
            else
            {
                if (ps.isPlaying) ps.Stop();
            }
        }
    }

    // ��ġ �ر� �õ�
    public bool TryUnlockDevice(int deviceID)
    {
        if (!deviceDictionary.TryGetValue(deviceID, out var device))
            return false;

        if (device.IsUnlocked)
            return false;

        // �ð� ���� ��� Ȯ��
        if (HasEnoughTimeCrystals(device.TimeCrystalCost))
        {
            UseTimeCrystals(device.TimeCrystalCost);
            device.IsUnlocked = true;

            // �ð� ȿ�� ������Ʈ
            UpdateDeviceVisuals(device);

            // ����
            SaveDeviceStatus();

            // �ر� �̺�Ʈ �߻�
            OnDeviceUnlocked?.Invoke(device);

            // ȿ�� ����
            ApplyDeviceEffect(device);

            return true;
        }

        return false;
    }

    // �κ��丮���� �ð� ���� ���� Ȯ��
    private bool HasEnoughTimeCrystals(int amount)
    {
        if (inventorySystem == null) return false;

        int currentAmount = inventorySystem.GetItemQuantity(timeCrystalItemID);
        return currentAmount >= amount;
    }

    // �ð� ���� ���
    private void UseTimeCrystals(int amount)
    {
        if (inventorySystem == null) return;

        inventorySystem.RemoveItem(timeCrystalItemID, amount);
    }

    // ��ġ ȿ�� ����
    private void ApplyDeviceEffect(TemporalDevice device)
    {
        if (!device.IsUnlocked) return;

        // ���⼭ ȿ�� ������ ���� ������ �ý��ۿ� ȿ�� ����
        switch (device.EffectType)
        {
            case EffectType.RARE_ABILITY_CHANCE:
                // �α׶���ũ �ɷ� ���� �� ���� ��� ���� Ȯ�� ����
                Debug.Log($"����: ���� �ɷ� ���� Ȯ�� {device.EffectValue * 100}% ����");

                // DungeonAbilityManager ���� - ���� ȣ�� �߰�
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.SetRareAbilityChanceMultiplier(1f + device.EffectValue);
                    Debug.Log($"DungeonAbilityManager�� ���� �ɷ� Ȯ�� ������ �����: {1f + device.EffectValue}");
                }
                else
                {
                    Debug.LogWarning("DungeonAbilityManager�� ã�� �� ���� ��ġ ȿ���� ������ �� �����ϴ�.");
                }
                break;
            case EffectType.ABILITY_SELECTION_COUNT:
                // ���ο� ȿ�� Ÿ�� ó��
                int selectionCount = (int)device.EffectValue;
                Debug.Log($"����: �ɷ� ������ ���� {selectionCount}���� ����");
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.SetAbilitiesPerSelection(selectionCount);
                }
                break;
            case EffectType.EXTRA_REWARD_CHANCE:
                Debug.Log($"����: �� Ŭ���� �� �߰� ���� ���� Ȯ�� {device.EffectValue * 100}% ����");
                break;

        }
    }

    // ��� �رݵ� ��ġ ȿ�� ����
    public void ApplyAllUnlockedDeviceEffects()
    {
        foreach (var device in allDevices)
        {
            if (device.IsUnlocked)
            {
                ApplyDeviceEffect(device);
            }
        }
    }

    // ��ġ �ر� ���� ����
    private void SaveDeviceStatus()
    {
        if (saveManager == null) return;

        Dictionary<int, bool> deviceStatus = new Dictionary<int, bool>();
        foreach (var device in allDevices)
        {
            deviceStatus[device.ID] = device.IsUnlocked;
        }

        saveManager.SaveDeviceUnlockStatus(deviceStatus);
    }

    // ��� ��ġ ���� ��������
    public List<TemporalDevice> GetAllDevices()
    {
        return allDevices;
    }

    // Ư�� ��ġ ���� ��������
    public TemporalDevice GetDevice(int deviceID)
    {
        if (deviceDictionary.TryGetValue(deviceID, out var device))
        {
            return device;
        }
        return null;
    }

    // ���� �رݵ� ��ġ �� ��������
    public int GetUnlockedDeviceCount()
    {
        return allDevices.Count(d => d.IsUnlocked);
    }
}