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

    [Header("�ε� UI ���")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI tipText;

    [Header("����")]
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

    // �ε� ȭ�� ǥ�� (�� ��ȯ �� ���)
    public void ShowLoading(string sceneName, Action onComplete = null)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneAsync(sceneName, onComplete));
        }
    }

    // �ε� ȭ�� ǥ�� (��Ż �̵� �� ���� ������ ���)
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

        // ���� �� ����
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }

        // �ε� UI Ȱ��ȭ �� ���̵� ��
        canvasGroup.gameObject.SetActive(true);
        
        progressBar.fillAmount = 0f;

        // ���̵� ��
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        // ���α׷��� �� �ִϸ��̼�
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Clamp01(timeElapsed / duration);
            yield return null;
        }

        // �ε� �Ϸ�
     
        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(0.5f);

        // ���̵� �ƿ�
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        canvasGroup.gameObject.SetActive(false);
        isLoading = false;

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
    {
        isLoading = true;

        // �ε� UI Ȱ��ȭ �� ���̵� ��
        canvasGroup.gameObject.SetActive(true);
        

        // ���� �� ����
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[UnityEngine.Random.Range(0, loadingTips.Length)];
        }

        // ���̵� ��
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        // �� �ε� ����
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        asyncOperation.allowSceneActivation = false;

        float progress = 0f;

        // �ε� ���� ��Ȳ ǥ��
        while (progress < 0.9f)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressBar.fillAmount = progress;
            yield return null;
        }

       
       
        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(1f);

        // �� Ȱ��ȭ
        asyncOperation.allowSceneActivation = true;

        // ���� �ε�� �� ���̵� �ƿ�
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
            // ��� ���̵� �ƿ�
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime)
                .OnComplete(() => {
                    canvasGroup.gameObject.SetActive(false);
                    isLoading = false;
                });
        }
    }
}