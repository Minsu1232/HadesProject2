// 인디케이터 컨트롤러
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