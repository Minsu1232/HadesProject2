
// MaterialItemUI.cs - �ʿ� ��� ǥ�� UI
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MaterialItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image sufficientIndicator;

    public void SetMaterial(Item item, int requiredQuantity, int ownedQuantity)
    {
        if (itemIcon != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;
        }

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        if (quantityText != null)
            quantityText.text = $"{ownedQuantity}/{requiredQuantity}";

        // ����� ��ᰡ �ִ��� ǥ��
        if (sufficientIndicator != null)
        {
            sufficientIndicator.color = ownedQuantity >= requiredQuantity ?
                Color.green : new Color(1, 0.5f, 0.5f);
        }
    }

    public void SetMaxLevel()
    {
        if (itemIcon != null)
            itemIcon.enabled = false;

        if (itemNameText != null)
            itemNameText.text = "�ִ� ���� �޼�";

        if (quantityText != null)
            quantityText.enabled = false;

        if (sufficientIndicator != null)
            sufficientIndicator.enabled = false;
    }
}