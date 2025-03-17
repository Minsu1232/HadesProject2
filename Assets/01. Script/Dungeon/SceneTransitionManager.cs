using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("��ȯ ȿ�� ����")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color defaultFadeColor = Color.black;

    [Header("�̹��� ����")]
    [SerializeField] private Image fadeImage;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // �ʱ� ����
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���̵� �� (ȭ���� ��ο���)
    public void FadeIn(Action onComplete = null, Color? color = null)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);

        // ���� ����
        if (color.HasValue)
        {
            fadeImage.color = color.Value;
        }
        else
        {
            fadeImage.color = defaultFadeColor;
        }

        // ���̵� �� �ִϸ��̼�
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 1f, fadeDuration)
            .OnComplete(() => {
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    // ���̵� �ƿ� (��ο� ȭ���� �����)
    public void FadeOut(Action onComplete = null)
    {
        if (isTransitioning) return;

        isTransitioning = true;

        // ���̵� �ƿ� �ִϸ��̼�
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, fadeDuration)
            .OnComplete(() => {
                fadeCanvasGroup.gameObject.SetActive(false);
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    // ���� ��ȯ �� ª�� �÷��� ȿ��
    public void FlashEffect(Color flashColor, float duration = 0.2f)
    {
        if (isTransitioning) return;

        StartCoroutine(FlashCoroutine(flashColor, duration));
    }

    private IEnumerator FlashCoroutine(Color flashColor, float duration)
    {
        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeImage.color = flashColor;

        // ���� ���̵� ��
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0.5f, duration * 0.3f);
        yield return new WaitForSeconds(duration * 0.3f);

        // ª�� ����
        yield return new WaitForSeconds(duration * 0.1f);

        // ���̵� �ƿ�
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, duration * 0.6f);
        yield return new WaitForSeconds(duration * 0.6f);

        fadeCanvasGroup.gameObject.SetActive(false);
        isTransitioning = false;
    }

    // �÷� ��ũ�� ȿ�� (��: �ǰ� �� ������ ȿ��)
    public void ScreenColorEffect(Color color, float intensity, float duration)
    {
        if (isTransitioning) return;

        StartCoroutine(ColorEffectCoroutine(color, intensity, duration));
    }

    private IEnumerator ColorEffectCoroutine(Color color, float intensity, float duration)
    {
        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);

        // ���� ���İ� ����
        Color effectColor = color;
        effectColor.a = intensity;
        fadeImage.color = effectColor;

        // ���̵� ��
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 1f, duration * 0.2f);
        yield return new WaitForSeconds(duration * 0.2f);

        // ����
        yield return new WaitForSeconds(duration * 0.2f);

        // ���̵� �ƿ�
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, duration * 0.6f);
        yield return new WaitForSeconds(duration * 0.6f);

        fadeCanvasGroup.gameObject.SetActive(false);
        isTransitioning = false;
    }
}