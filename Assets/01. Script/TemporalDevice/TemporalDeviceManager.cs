using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;

// 효과 타입 열거형
public enum EffectType
{
    NONE,
    RARE_ABILITY_CHANCE, // 첫 번째 장치 효과만 구현
    ABILITY_SELECTION_COUNT,
    EXTRA_REWARD_CHANCE
}

// 시간 장치 클래스
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

    // 3D 오브젝트 참조
    [HideInInspector]
    public GameObject DeviceObject;
}

// 시간 장치 관리자
public class TemporalDeviceManager : MonoBehaviour
{
    public static TemporalDeviceManager Instance { get; private set; }

    [SerializeField] private List<TemporalDevice> allDevices = new List<TemporalDevice>();
    [SerializeField] private Transform devicesParent; // 장치들의 부모 Transform

    private Dictionary<int, TemporalDevice> deviceDictionary = new Dictionary<int, TemporalDevice>();
    private InventorySystem inventorySystem;
    private SaveManager saveManager;

    // 시간 조각 아이템의 ID (인벤토리에서 사용되는)
    [SerializeField] private int timeCrystalItemID = 3001;

    // 장치 해금 이벤트
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
        // 인벤토리 시스템과 세이브 매니저 참조 가져오기
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
        // 3D 오브젝트 가져오기
       

        // 모든 해금된 장치 효과 적용
        ApplyAllUnlockedDeviceEffects();



        SetupDeviceObjects();

       

    }

    private void InitializeDevices()
    {
        // CSV 파일에서 장치 데이터 로드
        LoadDevicesFromCSV();

        // 저장된 해금 상태 불러오기
        LoadUnlockStatus();

        // Dictionary 초기화
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
                continue; // 헤더 라인 스킵
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
            Debug.Log($"장치 해금 상태 로드: {savedData.Count}개의 항목");
            foreach (var pair in savedData)
            {
                Debug.Log($"장치 ID {pair.Key}: {(pair.Value ? "해금됨" : "미해금")}");
            }

        }
    }

    // 3D 장치 오브젝트 설정
    private void SetupDeviceObjects()
    {
        if (devicesParent == null)
        {
            Debug.LogError("Devices parent transform not set!");
            return;
        }

        // 장치 오브젝트 찾기
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

    // 장치 시각 효과 업데이트
    private void UpdateDeviceVisuals(TemporalDevice device)
    {
        if (device.DeviceObject == null) return;

        // 활성화 상태에 따라 시각 효과 변경
        DeviceVisualController visualController = device.DeviceObject.GetComponent<DeviceVisualController>();
        if (visualController != null)
        {
            visualController.SetActiveState(device.IsUnlocked);
        }
        else
        {
            // 기본 활성화/비활성화 처리
            Renderer[] renderers = device.DeviceObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material mat = renderer.material;
                if (device.IsUnlocked)
                {
                    // 활성화 시 원래 색상으로
                    mat.color = Color.white;

                    // 발광 효과 (에미시브 텍스처가 있는 경우)
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", Color.white);
                    }
                }
                else
                {
                    // 비활성화 시 회색조
                    mat.color = Color.gray;

                    // 발광 효과 끄기
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        // 활성화 파티클 시스템 제어
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

    // 장치 해금 시도
    public bool TryUnlockDevice(int deviceID)
    {
        if (!deviceDictionary.TryGetValue(deviceID, out var device))
            return false;

        if (device.IsUnlocked)
            return false;

        // 시간 조각 비용 확인
        if (HasEnoughTimeCrystals(device.TimeCrystalCost))
        {
            UseTimeCrystals(device.TimeCrystalCost);
            device.IsUnlocked = true;

            // 시각 효과 업데이트
            UpdateDeviceVisuals(device);

            // 저장
            SaveDeviceStatus();

            // 해금 이벤트 발생
            OnDeviceUnlocked?.Invoke(device);

            // 효과 적용
            ApplyDeviceEffect(device);

            return true;
        }

        return false;
    }

    // 인벤토리에서 시간 조각 개수 확인
    private bool HasEnoughTimeCrystals(int amount)
    {
        if (inventorySystem == null) return false;

        int currentAmount = inventorySystem.GetItemQuantity(timeCrystalItemID);
        return currentAmount >= amount;
    }

    // 시간 조각 사용
    private void UseTimeCrystals(int amount)
    {
        if (inventorySystem == null) return;

        inventorySystem.RemoveItem(timeCrystalItemID, amount);
    }

    // 장치 효과 적용
    private void ApplyDeviceEffect(TemporalDevice device)
    {
        if (!device.IsUnlocked) return;

        // 여기서 효과 유형에 따라 적절한 시스템에 효과 적용
        switch (device.EffectType)
        {
            case EffectType.RARE_ABILITY_CHANCE:
                // 로그라이크 능력 선택 시 레어 등급 등장 확률 증가
                Debug.Log($"적용: 레어 능력 등장 확률 {device.EffectValue * 100}% 증가");

                // DungeonAbilityManager 연동 - 직접 호출 추가
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.SetRareAbilityChanceMultiplier(1f + device.EffectValue);
                    Debug.Log($"DungeonAbilityManager에 레어 능력 확률 수정자 적용됨: {1f + device.EffectValue}");
                }
                else
                {
                    Debug.LogWarning("DungeonAbilityManager를 찾을 수 없어 장치 효과를 적용할 수 없습니다.");
                }
                break;
            case EffectType.ABILITY_SELECTION_COUNT:
                // 새로운 효과 타입 처리
                int selectionCount = (int)device.EffectValue;
                Debug.Log($"적용: 능력 선택지 수를 {selectionCount}개로 설정");
                if (DungeonAbilityManager.Instance != null)
                {
                    DungeonAbilityManager.Instance.SetAbilitiesPerSelection(selectionCount);
                }
                break;
            case EffectType.EXTRA_REWARD_CHANCE:
                Debug.Log($"적용: 방 클리어 시 추가 보상 등장 확률 {device.EffectValue * 100}% 증가");
                break;

        }
    }

    // 모든 해금된 장치 효과 적용
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

    // 장치 해금 상태 저장
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

    // 모든 장치 정보 가져오기
    public List<TemporalDevice> GetAllDevices()
    {
        return allDevices;
    }

    // 특정 장치 정보 가져오기
    public TemporalDevice GetDevice(int deviceID)
    {
        if (deviceDictionary.TryGetValue(deviceID, out var device))
        {
            return device;
        }
        return null;
    }

    // 현재 해금된 장치 수 가져오기
    public int GetUnlockedDeviceCount()
    {
        return allDevices.Count(d => d.IsUnlocked);
    }
}