using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class SuccessUI : MonoBehaviour, ISuccessUI
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Transform iconParent;
    // ���� ��������Ʈ ���
    [SerializeField] private Sprite baseSprite;

    // �� ���¿� ä���� ������ ������ �����մϴ�.
    [SerializeField] private Color emptyColor = Color.white;
    [SerializeField] private Color filledColor = Color.green;

    private List<Image> successIcons = new List<Image>();

    public void InitializeSuccessUI(int maxSuccessCount)
    {
        // ���� ������ ����
        foreach (Transform child in iconParent)
        {
            Destroy(child.gameObject);
        }
        successIcons.Clear();

        // maxSuccessCount ��ŭ ���� ����
        for (int i = 0; i < maxSuccessCount; i++)
        {
            GameObject iconObj = Instantiate(iconPrefab, iconParent);
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = baseSprite;
                // �ʱ� ���´� emptyColor�� ����
                iconImage.color = emptyColor;
                successIcons.Add(iconImage);
            }
        }
    }

    public void UIOff()
    {
     
        gameObject.SetActive(false);
        
    }

    public void UpdateSuccessCount(int currentSuccessCount)
    {
        for (int i = 0; i < successIcons.Count; i++)
        {
            if (i < currentSuccessCount)
            {
                // ������ �������� filledColor�� ���� (Yoyo ���� ����)
                successIcons[i].DOColor(filledColor, 0.2f).SetEase(Ease.OutBack);
            }
            else
            {
                // �������� emptyColor ����
                successIcons[i].DOColor(emptyColor, 0.2f).SetEase(Ease.OutBack);
            }
        }
    }
}
