using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIDebugLogger : MonoBehaviour
{
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image successBar;
    [SerializeField] private Image progressArrow;

    void Start()
    {
        LogRectTransformInfo("BackgroundBar", backgroundBar.rectTransform);
        LogRectTransformInfo("SuccessBar", successBar.rectTransform);
        LogRectTransformInfo("ProgressArrow", progressArrow.rectTransform);
    }

    private void LogRectTransformInfo(string name, RectTransform rt)
    {
        Debug.Log($"{name} - Anchored Position: {rt.anchoredPosition}, " +
                  $"Size Delta: {rt.sizeDelta}, " +
                  $"Pivot: {rt.pivot}, " +
                  $"Anchors: Min {rt.anchorMin} Max {rt.anchorMax}, " +
                  $"Rect: {rt.rect}");
    }
}
