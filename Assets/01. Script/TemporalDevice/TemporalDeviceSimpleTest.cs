using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 시간 장치 간단 테스트 UI
public class TemporalDeviceSimpleTest : MonoBehaviour
{
    [SerializeField] private Button unlockDeviceButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button addCrystalsButton;
    [SerializeField] private TextMeshProUGUI crystalCountText;

    // 테스트할 장치 ID (기본값: 1 - "운명의 진자")
    [SerializeField] private int deviceIdToTest = 1;

    private TemporalDeviceManager deviceManager;
    private InventorySystem inventorySystem;

    private void Start()
    {
        // 매니저 인스턴스 참조
        deviceManager = TemporalDeviceManager.Instance;
        inventorySystem = InventorySystem.Instance;

        if (deviceManager == null || inventorySystem == null)
        {
            Debug.LogError("TemporalDeviceManager 또는 InventorySystem을 찾을 수 없습니다.");
            statusText.text = "오류: 필요한 매니저를 찾을 수 없습니다.";
            unlockDeviceButton.interactable = false;
            return;
        }

        // 버튼 이벤트 연결
        unlockDeviceButton.onClick.AddListener(UnlockTestDevice);

        if (addCrystalsButton != null)
        {
            addCrystalsButton.onClick.AddListener(AddTestCrystals);
        }

        // 초기 상태 업데이트
        UpdateUI();

        // 인벤토리 변경 이벤트 연결
        inventorySystem.OnInventoryChanged += UpdateUI;

        // 장치 해금 이벤트 연결
        deviceManager.OnDeviceUnlocked += OnDeviceUnlocked;
    }

    private void OnDestroy()
    {
        // 이벤트 연결 해제
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= UpdateUI;
        }

        if (deviceManager != null)
        {
            deviceManager.OnDeviceUnlocked -= OnDeviceUnlocked;
        }
    }

    // UI 업데이트
    private void UpdateUI()
    {
        // 크리스탈 수량 업데이트
        if (crystalCountText != null && inventorySystem != null)
        {
            int crystalCount = inventorySystem.GetItemQuantity(3001);
            crystalCountText.text = $"시간 조각: {crystalCount}개";
        }

        // 장치 상태 업데이트
        if (statusText != null && deviceManager != null)
        {
            TemporalDevice device = deviceManager.GetDevice(deviceIdToTest);
            if (device != null)
            {
                if (device.IsUnlocked)
                {
                    statusText.text = $"{device.DeviceName}이(가) 활성화되었습니다.";
                    statusText.color = Color.green;
                    unlockDeviceButton.interactable = false;
                }
                else
                {
                    statusText.text = $"{device.DeviceName} (비용: {device.TimeCrystalCost} 시간 조각)";
                    statusText.color = Color.white;

                    // 크리스탈이 충분한지 확인하여 버튼 활성화/비활성화
                    int crystalCount = inventorySystem.GetItemQuantity(3001);
                    unlockDeviceButton.interactable = crystalCount >= device.TimeCrystalCost;
                }
            }
            else
            {
                statusText.text = $"장치 ID {deviceIdToTest}을(를) 찾을 수 없습니다.";
                statusText.color = Color.red;
                unlockDeviceButton.interactable = false;
            }
        }
    }

    // 테스트 장치 해금
    private void UnlockTestDevice()
    {
        if (deviceManager != null)
        {
            bool success = deviceManager.TryUnlockDevice(deviceIdToTest);

            if (success)
            {
                Debug.Log($"장치 해금 성공: ID {deviceIdToTest}");
            }
            else
            {
                Debug.LogWarning($"장치 해금 실패: ID {deviceIdToTest}");

                // 실패 원인 파악 (로그용)
                TemporalDevice device = deviceManager.GetDevice(deviceIdToTest);
                if (device == null)
                {
                    Debug.LogError($"장치 ID {deviceIdToTest}을(를) 찾을 수 없습니다.");
                }
                else if (device.IsUnlocked)
                {
                    Debug.Log($"장치 '{device.DeviceName}'은(는) 이미 해금되어 있습니다.");
                }
                else
                {
                    int crystalCount = inventorySystem.GetItemQuantity(3001);
                    Debug.Log($"시간 조각 부족: 필요 {device.TimeCrystalCost}, 보유 {crystalCount}");
                }
            }

            // UI 업데이트
            UpdateUI();
        }
    }

    // 테스트용 시간 조각 추가
    private void AddTestCrystals()
    {
        if (inventorySystem != null)
        {
            inventorySystem.AddItem(3001, 20);
            Debug.Log("테스트용 시간 조각 20개 추가");
        }
    }

    // 장치 해금 이벤트 처리
    private void OnDeviceUnlocked(TemporalDevice device)
    {
        Debug.Log($"장치 해금 이벤트 수신: '{device.DeviceName}'");

        // ID가 테스트 중인 장치와 일치하면 UI 업데이트
        if (device.ID == deviceIdToTest)
        {
            UpdateUI();
        }
    }
}