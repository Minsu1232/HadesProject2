using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeviceButton : MonoBehaviour
{
    [SerializeField] private Image deviceIcon;
    [SerializeField] private TextMeshProUGUI deviceNameText;
    [SerializeField] private TextMeshProUGUI deviceCostText;
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private GameObject selectedIndicator;
    [SerializeField] private Button button;

    // ��ư Ŭ�� �̺�Ʈ
    public event Action<TemporalDevice> OnButtonClicked;

    private TemporalDevice device;
    private bool isSelected = false;

    private void Awake()
    {
        // ��ư �̺�Ʈ ����
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        // ���� ǥ�� �ʱ� ����
        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(false);
        }
    }

    // ��ġ ������ ��ư �ʱ�ȭ
    public void Initialize(TemporalDevice deviceData, Sprite icon)
    {
        device = deviceData;

        // ������ ����
        if (deviceIcon != null && icon != null)
        {
            deviceIcon.sprite = icon;
        }

        // ���� ������Ʈ
        UpdateButtonState();
    }

    // ��ư ���� ������Ʈ
    public void UpdateButtonState()
    {
        if (device == null) return;

        // �̸� ����
        if (deviceNameText != null)
        {
            deviceNameText.text = device.DeviceName;
        }

        // ��� ����
        if (deviceCostText != null)
        {
            deviceCostText.text = device.TimeCrystalCost.ToString();
        }

        // ��� �������� ����
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!device.IsUnlocked);
        }

        // �ر� ���¿� ���� ������ ���� ����
        if (deviceIcon != null)
        {
            Color iconColor = device.IsUnlocked ? Color.white : new Color(0.7f, 0.7f, 0.7f, 0.8f);
            deviceIcon.color = iconColor;
        }
    }

    // ���� ���� ����
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }

    // ��ư Ŭ�� ó��
    private void OnClick()
    {
        // Ŭ�� �̺�Ʈ �߻�
        OnButtonClicked?.Invoke(device);

        // �ڵ����� ���� ���·� ����
        SetSelected(true);

        // �ٸ� ��ư���� ���� ���� ����
        DeviceButton[] otherButtons = transform.parent.GetComponentsInChildren<DeviceButton>();
        foreach (var otherButton in otherButtons)
        {
            if (otherButton != this)
            {
                otherButton.SetSelected(false);
            }
        }
    }
}