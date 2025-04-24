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
    private string[] loadingTips = new string[]
    {
        "������ ������ ������ ������ �����ϴ� ������ �ؿԽ��ϴ�.",
        "�ɿ��� ������ �ΰ� ���迡 ó�� ������ ���� 200�� ���̶�� �մϴ�.",
        "���� ������ �����ڴ� �Ѷ� ������ �����̾��ٰ� �������ϴ�.",
        "ESC Ű�� ������ ���� ������ �������� �� �� �ֽ��ϴ�.",
        "���� ����� ���� ������ Ȯ���ϼ���.",
        "���� �ʴ� ���� �¸��� ���� Ȯ���� ����Դϴ�.",
        "������ ����ġ�� �͵� �����Դϴ�.",
        "������ �칰 �Ʒ����� ����� ������ �ִٰ� �մϴ�."
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

        // ���� �Ͻ� ����
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
            Debug.Log("�ε� ����: ���� �Ͻ� ������ (���� ��)");
        }

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

        // ���� �簳
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeMusic();
            Debug.Log("�ε� �Ϸ�: ���� �簳�� (���� ��)");
        }

        onComplete?.Invoke();
    }

    private IEnumerator LoadSceneAsync(string sceneName, Action onComplete)
    {
        isLoading = true;

        // ���� �Ͻ� ����
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseMusic();
            Debug.Log("�ε� ����: ���� �Ͻ� ������ (�� ��ȯ)");
        }

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

        // �ε� ���� ��Ȳ ǥ�� (0 ~ 90%)
        while (progress < 0.90f)
        {
            progress = Mathf.MoveTowards(progress, asyncOperation.progress, Time.deltaTime);
            progressBar.fillAmount = progress;
            yield return null;
        }

        // 90 ~ 100% �κ��� �������� �ε巴�� �ϼ�
        float finalProgress = 0.9f;
        while (finalProgress < 1.0f)
        {
            finalProgress += Time.deltaTime * 0.2f; // �ӵ� ����
            progressBar.fillAmount = finalProgress;
            yield return null;
        }

        progressBar.fillAmount = 1f;

        yield return new WaitForSeconds(0.5f);

        // �� Ȱ��ȭ
        asyncOperation.allowSceneActivation = true;

        // ���� �ε�� ������ ���
        while (!asyncOperation.isDone)
        {
            yield return null;
        }

        // ���� �ε�� �� ���̵� �ƿ�
        yield return new WaitForSeconds(0.5f);
        DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0f, fadeTime);
        yield return new WaitForSeconds(fadeTime);

        canvasGroup.gameObject.SetActive(false);
        isLoading = false;

        // �� ��ȯ �Ŀ��� ������ �簳���� ����
        // AudioManager�� OnSceneLoaded �̺�Ʈ�� �� ���� �´� ������ �ڵ����� ���
        Debug.Log("�ε� �Ϸ�: �� ���� �´� ������ AudioManager�� ���� �ڵ� �����");

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

                    // ���� �簳
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.ResumeMusic();
                        Debug.Log("�ε� ���: ���� �簳��");
                    }
                });
        }
    }
}