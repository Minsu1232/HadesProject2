using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("전환 효과 설정")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private Color defaultFadeColor = Color.black;

    [Header("이미지 설정")]
    [SerializeField] private Image fadeImage;

    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 초기 설정
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 페이드 인 (화면이 어두워짐)
    public void FadeIn(Action onComplete = null, Color? color = null)
    {
        if (isTransitioning) return;

        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);

        // 색상 설정
        if (color.HasValue)
        {
            fadeImage.color = color.Value;
        }
        else
        {
            fadeImage.color = defaultFadeColor;
        }

        // 페이드 인 애니메이션
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 1f, fadeDuration)
            .OnComplete(() => {
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    // 페이드 아웃 (어두운 화면이 밝아짐)
    public void FadeOut(Action onComplete = null)
    {
        if (isTransitioning) return;

        isTransitioning = true;

        // 페이드 아웃 애니메이션
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, fadeDuration)
            .OnComplete(() => {
                fadeCanvasGroup.gameObject.SetActive(false);
                isTransitioning = false;
                onComplete?.Invoke();
            });
    }

    // 몬스터 소환 시 짧은 플래시 효과
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

        // 빠른 페이드 인
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0.5f, duration * 0.3f);
        yield return new WaitForSeconds(duration * 0.3f);

        // 짧게 유지
        yield return new WaitForSeconds(duration * 0.1f);

        // 페이드 아웃
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, duration * 0.6f);
        yield return new WaitForSeconds(duration * 0.6f);

        fadeCanvasGroup.gameObject.SetActive(false);
        isTransitioning = false;
    }

    // 컬러 스크린 효과 (예: 피격 시 빨간색 효과)
    public void ScreenColorEffect(Color color, float intensity, float duration)
    {
        if (isTransitioning) return;

        StartCoroutine(ColorEffectCoroutine(color, intensity, duration));
    }

    private IEnumerator ColorEffectCoroutine(Color color, float intensity, float duration)
    {
        isTransitioning = true;
        fadeCanvasGroup.gameObject.SetActive(true);

        // 색상 알파값 조정
        Color effectColor = color;
        effectColor.a = intensity;
        fadeImage.color = effectColor;

        // 페이드 인
        fadeCanvasGroup.alpha = 0f;
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 1f, duration * 0.2f);
        yield return new WaitForSeconds(duration * 0.2f);

        // 유지
        yield return new WaitForSeconds(duration * 0.2f);

        // 페이드 아웃
        DOTween.To(() => fadeCanvasGroup.alpha, x => fadeCanvasGroup.alpha = x, 0f, duration * 0.6f);
        yield return new WaitForSeconds(duration * 0.6f);

        fadeCanvasGroup.gameObject.SetActive(false);
        isTransitioning = false;
    }
}