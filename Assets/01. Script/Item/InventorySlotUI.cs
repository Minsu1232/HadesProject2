// ItemSlotUI.cs - �κ��丮 ������ ���� UI ������Ʈ
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemQuantityText;
    [SerializeField] private Image rarityBorder;
    [SerializeField] private GameObject selectedHighlight;
    [SerializeField] private TextMeshProUGUI quantityText;
    private Item currentItem;
    private Action onClickCallback;

    private Color[] rarityColors = {
        new Color(0.7f, 0.7f, 0.7f), // Common - ȸ��
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - ���
        new Color(0.3f, 0.3f, 0.9f), // Rare - �Ķ���
        new Color(0.7f, 0.3f, 0.9f), // Epic - �����
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - �ݻ�
    };
   

    // �ʱ�ȭ
    private void Start()
    {
        // Ŭ�� �̺�Ʈ ����
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => {
                onClickCallback?.Invoke();
                SetSelected(true);
            });
        }

        // ���� ���� �ʱ�ȭ
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(false);
        }
    }

    // ������ ����
    // ItemSlotUI.cs�� SetItem �޼��� ����
    public void SetItem(Item item, int quantity)
    {
        currentItem = item;

        if (item != null)
        {
            // ������ ����: ������ ǥ��
            if (itemIcon != null)
            {
                itemIcon.sprite = item.icon;
                itemIcon.enabled = true;
            }

            // ���� ǥ�� (2�� �̻��� ���� ǥ��)
            if (quantityText != null)
            {
                if (quantity > 1)
                {
                    quantityText.text = "x" + quantity.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            // �� ����: ������ ����
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }

            if (quantityText != null)
            {
                quantityText.gameObject.SetActive(false);
            }
        }
    }

    // Ŭ�� �ݹ� ����
    public void SetClickCallback(Action callback)
    {
        onClickCallback = callback;
    }

    // ���� ���� ����
    public void SetSelected(bool selected)
    {
        if (selectedHighlight != null)
        {
            selectedHighlight.SetActive(selected);
        }

        // �ٸ� ������ ���� ����
        if (selected)
        {
            ItemSlotUI[] allSlots = FindObjectsOfType<ItemSlotUI>();
            foreach (var slot in allSlots)
            {
                if (slot != this && slot.selectedHighlight != null)
                {
                    slot.selectedHighlight.SetActive(false);
                }
            }
        }
    }

    // ������ ��ȯ
    public Item GetItem()
    {
        return currentItem;
    }
}