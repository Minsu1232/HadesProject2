using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : Singleton<LoadingUI>
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("Settings")]
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private string[] loadingTips;

    private void Awake()
    {


        DontDestroyOnLoad(gameObject);
        InitializeUI();



    }

    private void InitializeUI()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Hide(); // 초기 상태는 숨김
    }

    public void Show()
    {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
        SetRandomTip();
        UpdateProgress(0);
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{(progress * 100):F0}%";
    }

    private void SetRandomTip()
    {
        if (tipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }
    }

    // 부드러운 페이드 인/아웃이 필요한 경우 사용
    public async void FadeIn()
    {
        float elapsedTime = 0;
        canvasGroup.blocksRaycasts = true;

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = elapsedTime;
            await System.Threading.Tasks.Task.Yield();
        }

        canvasGroup.alpha = 1;
    }

    public async void FadeOut()
    {
        float elapsedTime = 1f;

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime * fadeSpeed;
            canvasGroup.alpha = elapsedTime;
            await System.Threading.Tasks.Task.Yield();
        }

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
}
