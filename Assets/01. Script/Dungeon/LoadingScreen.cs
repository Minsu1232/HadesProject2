using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("로딩 UI 요소")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("설정")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private string[] loadingTips;

    private bool isLoading = false;

   

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 로딩 화면 표시 (씬 전환 시 사용)
    public void ShowLoading(string sceneName, Action onComplete = null)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }
    }

    // 로딩 화면 표시 (포탈 이동 등 같은 씬에서 사용)
    public void ShowLoadingForDurationAndHide(float duration, Action onComplete = null)
    {
        if (!isLoading)
        {
            StartCoroutine(ShowLoadingForDuration(duration, onComplete));
        }
    }

    private IEnumerator ShowLoadingForDuration(float duration, Action onComplete)
    {
        isLoading = true;

        // 랜덤 팁 선택
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }

        // 로딩 UI 활성화 및 페이드 인
        canvasGroup.gameObject.SetActive(true);
        
        progressBar.fillAmount = 0f;

        // 페이드 인
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        // 프로그레스 바 애니메이션
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Clamp01(timeElapsed / duration);
            yield return null;
        }

        // 로딩 완료
     
        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(0.5f);

        // 페이드 아웃
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        canvasGroup.gameObject.SetActive(false);
        isLoading = false;

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
    {
        isLoading = true;

        // 로딩 UI 활성화 및 페이드 인
        canvasGroup.gameObject.SetActive(true);
        

        // 랜덤 팁 선택
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }

        // 페이드 인
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        // 씬 로드 시작
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        float progress = 0f;

        // 로딩 진행 상황 표시
        while (progress < 0.9f)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressBar.fillAmount = progress;
            yield return null;
        }

       
       
        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(1f);

        // 씬 활성화
        asyncOperation.allowSceneActivation = true;

        // 씬이 로드된 후 페이드 아웃
        yield return new WaitForSeconds(0.5f);
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        canvasGroup.gameObject.SetActive(false);
        isLoading = false;

        onComplete?.Invoke();
      
    }

    public void HideLoading()
    {
        if (isLoading)
        {
            // 즉시 페이드 아웃
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime)
                .OnComplete(() => {
                    canvasGroup.gameObject.SetActive(false);
                    isLoading = false;
                });
        }
    }
}