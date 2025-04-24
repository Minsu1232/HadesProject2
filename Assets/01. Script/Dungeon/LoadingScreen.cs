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
    private string[] loadingTips = new string[]
    {
        "용족은 고대부터 세계의 균형을 유지하는 역할을 해왔습니다.",
        "심연의 파편이 인간 세계에 처음 등장한 것은 200년 전이라고 합니다.",
        "데스 영역의 지배자는 한때 위대한 영웅이었다고 전해집니다.",
        "ESC 키를 누르면 게임 설정을 언제든지 열 수 있습니다.",
        "우측 상단을 눌러 업적을 확인하세요.",
        "죽지 않는 것이 승리의 가장 확실한 방법입니다.",
        "가끔은 도망치는 것도 전략입니다.",
        "마을의 우물 아래에는 비밀이 숨겨져 있다고 합니다."
    };

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

        // 음악 일시 정지
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
            Debug.Log("로딩 시작: 음악 일시 정지됨 (같은 씬)");
        }

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

        // 음악 재개
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
            Debug.Log("로딩 완료: 음악 재개됨 (같은 씬)");
        }

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
    {
        isLoading = true;

        // 음악 일시 정지
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
            Debug.Log("로딩 시작: 음악 일시 정지됨 (씬 전환)");
        }

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

        // 로딩 진행 상황 표시 (0 ~ 90%)
        while (progress < 0.90f)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressBar.fillAmount = progress;
            yield return null;
        }

        // 90 ~ 100% 부분을 수동으로 부드럽게 완성
        float finalProgress = 0.9f;
        while (finalProgress < 1.0f)
        {
            finalProgress += Time.deltaTime * 0.2f; // 속도 조절
            progressBar.fillAmount = finalProgress;
            yield return null;
        }

        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(0.5f);

        // 씬 활성화
        asyncOperation.allowSceneActivation = true;

        // 씬이 로드될 때까지 대기
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // 씬이 로드된 후 페이드 아웃
        yield return new WaitForSeconds(0.5f);
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        canvasGroup.gameObject.SetActive(false);
        isLoading = false;

        // 씬 전환 후에는 음악을 재개하지 않음
        // AudioManager의 OnSceneLoaded 이벤트가 새 씬에 맞는 음악을 자동으로 재생
        Debug.Log("로딩 완료: 새 씬에 맞는 음악이 AudioManager에 의해 자동 재생됨");

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

                    // 음악 재개
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.ResumeMusic();
                        Debug.Log("로딩 취소: 음악 재개됨");
                    }
                });
        }
    }
}