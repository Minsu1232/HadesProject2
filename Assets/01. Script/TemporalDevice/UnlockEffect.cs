using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnlockEffect : MonoBehaviour
{
    [Header("UI ���")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Image deviceIcon;
    [SerializeField] private Image runeImage;
    [SerializeField] private TextMeshProUGUI unlockText;

    [Header("��Ÿ�� ȿ�� ����")]
    [SerializeField] private Material burnMaterial;
    [SerializeField] private float burnSpeed = 0.7f;
    [SerializeField] private float burnSize = 0.15f;
    [SerializeField] private Color burnColor1 = new Color(1f, 0f, 0f);
    [SerializeField] private Color burnColor2 = new Color(1f, 0.7f, 0f);
    [SerializeField] private float noiseScale = 20f;
    [SerializeField] private float flameNoiseScale = 30f;
    [SerializeField] private float flameSpeed = 5f;
    [SerializeField] private float glowIntensity = 2f;
    [SerializeField] private float rotationSpeed = 30f;

    [Header("����")]
    [SerializeField] private AudioSource unlockSound;
    [SerializeField] private AudioSource burnSound;

    private Material burnMaterialInstance;
    private bool isEffectPlaying = false;

    private void Awake()
    {
        // �ʱ� ���� ����
        gameObject.SetActive(false);
    }

    // ȿ�� ����
    public void PlayEffect(string deviceName, Sprite icon)
    {
        if (isEffectPlaying) return;

        gameObject.SetActive(true);
        isEffectPlaying = true;

        // �ʱ� ���� ����
        if (backgroundPanel != null)
            backgroundPanel.color = new Color(0, 0, 0, 0);

        if (deviceIcon != null)
        {
            deviceIcon.sprite = icon;
            deviceIcon.rectTransform.localScale = Vector3.zero;
            deviceIcon.color = new Color(1, 1, 1, 0);
        }

        if (unlockText != null)
        {
            unlockText.text = deviceName + " �ر�!";
            unlockText.alpha = 0;
        }

        // �� �̹��� �غ�
        PrepareRuneImage();

        // ȿ�� �ڷ�ƾ ����
        StartCoroutine(UnlockEffectSequence());
    }

    private void PrepareRuneImage()
    {
        if (runeImage == null || burnMaterial == null) return;

        // ��Ƽ���� �ν��Ͻ� ����
        burnMaterialInstance = new Material(burnMaterial);
        runeImage.material = burnMaterialInstance;

        // ���ν����� ���̴� �Ķ���� ����
        burnMaterialInstance.SetFloat("_DissolveAmount", 0);
        burnMaterialInstance.SetFloat("_BurnSize", burnSize);
        burnMaterialInstance.SetColor("_BurnColor1", burnColor1);
        burnMaterialInstance.SetColor("_BurnColor2", burnColor2);
        burnMaterialInstance.SetFloat("_NoiseScale", noiseScale);
        burnMaterialInstance.SetFloat("_FlameNoiseScale", flameNoiseScale);
        burnMaterialInstance.SetFloat("_FlameSpeed", flameSpeed);
        burnMaterialInstance.SetFloat("_EmissionIntensity", glowIntensity);

        // �� �̹��� �ʱ� ����
        runeImage.color = Color.white;
        runeImage.transform.localScale = Vector3.one;
        runeImage.gameObject.SetActive(true);
    }

    private IEnumerator UnlockEffectSequence()
    {
        // ���� ���
        if (unlockSound != null)
            unlockSound.Play();

        // ��� ���̵� ��
        float elapsed = 0f;
        while (elapsed < 0.6f)
        {
            float t = elapsed / 0.6f;

            if (backgroundPanel != null)
                backgroundPanel.color = new Color(0, 0, 0, Mathf.Lerp(0, 0.8f, t));

            elapsed += Time.deltaTime;
            yield return null;
        }

        // �� ȸ�� �� �߱� ȿ�� ����
        StartCoroutine(RuneGlowEffect());

        // �� ǥ�� ���
        yield return new WaitForSeconds(1.0f);

        // ��Ÿ�� ȿ�� ���
        if (burnSound != null)
            burnSound.Play();

        // �� ��Ÿ�� ȿ�� ����
        StartCoroutine(RuneBurningEffect());

        // Ÿ������ ���� ���
        yield return new WaitForSeconds(1.5f);

        // ������ ���̵� �� + Ȯ��
        elapsed = 0f;
        while (elapsed < 0.8f)
        {
            float t = elapsed / 0.8f;
            float scale = Mathf.SmoothStep(0, 1f, t);
            float alpha = Mathf.SmoothStep(0, 1f, t);

            if (deviceIcon != null)
            {
                deviceIcon.rectTransform.localScale = new Vector3(scale, scale, scale);
                deviceIcon.color = new Color(1, 1, 1, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // �ؽ�Ʈ ���̵� ��
        elapsed = 0f;
        while (elapsed < 0.5f)
        {
            float t = elapsed / 0.5f;

            if (unlockText != null)
                unlockText.alpha = Mathf.Lerp(0, 1, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ȿ�� ���� ���
        yield return new WaitForSeconds(2.0f);

        // ��� ��� ���̵� �ƿ�
        elapsed = 0f;
        while (elapsed < 1.0f)
        {
            float t = elapsed / 1.0f;

            if (backgroundPanel != null)
                backgroundPanel.color = new Color(0, 0, 0, Mathf.Lerp(0.8f, 0, t));

            if (deviceIcon != null)
                deviceIcon.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, t));

            if (unlockText != null)
                unlockText.alpha = Mathf.Lerp(1, 0, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // ȿ�� �Ϸ� �� ��Ȱ��ȭ
        isEffectPlaying = false;
        gameObject.SetActive(false);
    }

    private IEnumerator RuneGlowEffect()
    {
        if (runeImage == null || burnMaterialInstance == null) yield break;

        float elapsed = 0;
        float duration = 3.0f;

        while (elapsed < duration && runeImage.gameObject.activeSelf)
        {
            // ȸ��
            runeImage.rectTransform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // ������ ȿ�� (�޽�)
            float pulseAmount = (Mathf.Sin(elapsed * 3f) * 0.5f + 0.5f) * glowIntensity;
            burnMaterialInstance.SetFloat("_EmissionIntensity", pulseAmount + 1.5f);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator RuneBurningEffect()
    {
        if (runeImage == null || burnMaterialInstance == null) yield break;

        float dissolveAmount = 0;

        // ���� �� ������ Ÿ������ ȿ��
        while (dissolveAmount < 1)
        {
            // �������� �ӵ��� Ÿ������ (ó���� õõ��, ���߿� ������)
            float speed = burnSpeed * (0.1f + dissolveAmount);
            dissolveAmount += Time.deltaTime * speed;

            // ������ �� ����
            burnMaterialInstance.SetFloat("_DissolveAmount", dissolveAmount);

            // Ÿ�������� �� ũ�� ����
            float currentBurnSize = burnSize * (1f + dissolveAmount * 0.5f);
            burnMaterialInstance.SetFloat("_BurnSize", currentBurnSize);

            yield return null;
        }

        // ������ ������� ��Ȱ��ȭ
        runeImage.gameObject.SetActive(false);
    }
}