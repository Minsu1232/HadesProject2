// �ε������� ��Ʈ�ѷ�
using UnityEngine;
using UnityEngine.UI;

public class IndicatorController : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void UpdateFill(float amount)
    {
        fillImage.fillAmount = amount;
    }
}