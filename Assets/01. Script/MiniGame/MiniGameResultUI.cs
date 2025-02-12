using System.Collections;
using TMPro;
using UnityEngine;

public class MiniGameResultUI : MonoBehaviour
{
    [SerializeField] private Canvas resultCanvas;
    [SerializeField] private TextMeshProUGUI resultText;
    private Coroutine fadeOutCoroutine;

    private void Awake()
    {
        resultCanvas.gameObject.SetActive(false);
    }

    public void ShowResult(string result, Color color)
    {
        resultCanvas.gameObject.SetActive(true);
        resultText.color = color;
        resultText.text = result;

        if (fadeOutCoroutine != null)
            StopCoroutine(fadeOutCoroutine);

        fadeOutCoroutine = StartCoroutine(FadeOutText());
    }

    private IEnumerator FadeOutText()
    {
        yield return new WaitForSecondsRealtime(1f);  // 1초 동안 보여줌
        resultCanvas.gameObject.SetActive(false);
    }
}