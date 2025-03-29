using UnityEngine;
using UnityEngine.UI;
using TMPro;

// �ð� ��ġ ���� �׽�Ʈ UI
public class TemporalDeviceSimpleTest : MonoBehaviour
{
    [SerializeField] private Button unlockDeviceButton;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button addCrystalsButton;
    [SerializeField] private TextMeshProUGUI crystalCountText;

    // �׽�Ʈ�� ��ġ ID (�⺻��: 1 - "����� ����")
    [SerializeField] private int deviceIdToTest = 1;

    private TemporalDeviceManager deviceManager;
    private InventorySystem inventorySystem;

    private void Start()
    {
        // �Ŵ��� �ν��Ͻ� ����
        deviceManager = TemporalDeviceManager.Instance;
        inventorySystem = InventorySystem.Instance;

        if (deviceManager == null || inventorySystem == null)
        {
            Debug.LogError("TemporalDeviceManager �Ǵ� InventorySystem�� ã�� �� �����ϴ�.");
            statusText.text = "����: �ʿ��� �Ŵ����� ã�� �� �����ϴ�.";
            unlockDeviceButton.interactable = false;
            return;
        }

        // ��ư �̺�Ʈ ����
        unlockDeviceButton.onClick.AddListener(UnlockTestDevice);

        if (addCrystalsButton != null)
        {
            addCrystalsButton.onClick.AddListener(AddTestCrystals);
        }

        // �ʱ� ���� ������Ʈ
        UpdateUI();

        // �κ��丮 ���� �̺�Ʈ ����
        inventorySystem.OnInventoryChanged += UpdateUI;

        // ��ġ �ر� �̺�Ʈ ����
        deviceManager.OnDeviceUnlocked += OnDeviceUnlocked;
    }

    private void OnDestroy()
    {
        // �̺�Ʈ ���� ����
        if (inventorySystem != null)
        {
            inventorySystem.OnInventoryChanged -= UpdateUI;
        }

        if (deviceManager != null)
        {
            deviceManager.OnDeviceUnlocked -= OnDeviceUnlocked;
        }
    }

    // UI ������Ʈ
    private void UpdateUI()
    {
        // ũ����Ż ���� ������Ʈ
        if (crystalCountText != null && inventorySystem != null)
        {
            int crystalCount = inventorySystem.GetItemQuantity(3001);
            crystalCountText.text = $"�ð� ����: {crystalCount}��";
        }

        // ��ġ ���� ������Ʈ
        if (statusText != null && deviceManager != null)
        {
            TemporalDevice device = deviceManager.GetDevice(deviceIdToTest);
            if (device != null)
            {
                if (device.IsUnlocked)
                {
                    statusText.text = $"{device.DeviceName}��(��) Ȱ��ȭ�Ǿ����ϴ�.";
                    statusText.color = Color.green;
                    unlockDeviceButton.interactable = false;
                }
                else
                {
                    statusText.text = $"{device.DeviceName} (���: {device.TimeCrystalCost} �ð� ����)";
                    statusText.color = Color.white;

                    // ũ����Ż�� ������� Ȯ���Ͽ� ��ư Ȱ��ȭ/��Ȱ��ȭ
                    int crystalCount = inventorySystem.GetItemQuantity(3001);
                    unlockDeviceButton.interactable = crystalCount >= device.TimeCrystalCost;
                }
            }
            else
            {
                statusText.text = $"��ġ ID {deviceIdToTest}��(��) ã�� �� �����ϴ�.";
                statusText.color = Color.red;
                unlockDeviceButton.interactable = false;
            }
        }
    }

    // �׽�Ʈ ��ġ �ر�
    private void UnlockTestDevice()
    {
        if (deviceManager != null)
        {
            bool success = deviceManager.TryUnlockDevice(deviceIdToTest);

            if (success)
            {
                Debug.Log($"��ġ �ر� ����: ID {deviceIdToTest}");
            }
            else
            {
                Debug.LogWarning($"��ġ �ر� ����: ID {deviceIdToTest}");

                // ���� ���� �ľ� (�α׿�)
                TemporalDevice device = deviceManager.GetDevice(deviceIdToTest);
                if (device == null)
                {
                    Debug.LogError($"��ġ ID {deviceIdToTest}��(��) ã�� �� �����ϴ�.");
                }
                else if (device.IsUnlocked)
                {
                    Debug.Log($"��ġ '{device.DeviceName}'��(��) �̹� �رݵǾ� �ֽ��ϴ�.");
                }
                else
                {
                    int crystalCount = inventorySystem.GetItemQuantity(3001);
                    Debug.Log($"�ð� ���� ����: �ʿ� {device.TimeCrystalCost}, ���� {crystalCount}");
                }
            }

            // UI ������Ʈ
            UpdateUI();
        }
    }

    // �׽�Ʈ�� �ð� ���� �߰�
    private void AddTestCrystals()
    {
        if (inventorySystem != null)
        {
            inventorySystem.AddItem(3001, 20);
            Debug.Log("�׽�Ʈ�� �ð� ���� 20�� �߰�");
        }
    }

    // ��ġ �ر� �̺�Ʈ ó��
    private void OnDeviceUnlocked(TemporalDevice device)
    {
        Debug.Log($"��ġ �ر� �̺�Ʈ ����: '{device.DeviceName}'");

        // ID�� �׽�Ʈ ���� ��ġ�� ��ġ�ϸ� UI ������Ʈ
        if (device.ID == deviceIdToTest)
        {
            UpdateUI();
        }
    }
}