using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeviceVisualController : MonoBehaviour
{
    [Header("�⺻ ����")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color emissionColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;

    [Header("���ڸ� ȿ��")]
    [SerializeField] private Transform starsTransform; // Stars ������Ʈ
    [SerializeField] private Transform lineTransform; // Line ������Ʈ
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float lineBrightness = 2f; // �� ��� ����

    [Header("ȿ�� ������Ʈ")]
    [SerializeField] private ParticleSystem[] activeParticles;
    [SerializeField] private Light[] activeLights;
    [SerializeField] private AudioSource activationSound;
    [SerializeField] private GameObject[] activeGameObjects;

    [SerializeField] private Renderer[] deviceRenderers;

    [SerializeField] bool isActive = false;

    private Coroutine animationCoroutine;
    private Renderer starsRenderer;
    private Renderer lineRenderer;
    private Vector3 originalStarsScale;
    private Vector3 originalLineScale;

    private void Start()
    {
        TemporalDeviceManager.Instance.OnVillageEnter();
        // Stars�� Line ������Ʈ ã��
        if (starsTransform == null || lineTransform == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Equals("Stars"))
                {
                    starsTransform = child;
                    starsRenderer = child.GetComponent<Renderer>();
                }
                else if (child.name.Equals("Line"))
                {
                    lineTransform = child;
                    lineRenderer = child.GetComponent<Renderer>();
                }
            }
        }

        // �������� ���� �Ҵ���� �ʾҴٸ� ��������
        if (starsRenderer == null && starsTransform != null)
            starsRenderer = starsTransform.GetComponent<Renderer>();

        if (lineRenderer == null && lineTransform != null)
            lineRenderer = lineTransform.GetComponent<Renderer>();

        // ��ƼŬ �ý����� ���ٸ� ã��
        if (activeParticles == null || activeParticles.Length == 0)
        {
            activeParticles = GetComponentsInChildren<ParticleSystem>();
        }

        // ����Ʈ�� ���ٸ� ã��
        if (activeLights == null || activeLights.Length == 0)
        {
            activeLights = GetComponentsInChildren<Light>();
        }

        // ����̽� �������� �������� �ʾҴٸ� ��� ������ ã��
        if (deviceRenderers == null || deviceRenderers.Length == 0)
        {
            deviceRenderers = GetComponentsInChildren<Renderer>();
        }

        // ���� ������ ���� �� �α� �߰�
        if (starsTransform != null)
        {
            originalStarsScale = starsTransform.localScale;
            Debug.Log($"Stars ���� ������: {originalStarsScale}");

            // �������� 0�̸� �⺻������ ����
            if (originalStarsScale.magnitude < 0.001f)
            {
                Debug.LogWarning("Stars ���� �������� 0�� ����� �⺻�� ����");
                originalStarsScale = Vector3.one;
            }
        }

        if (lineTransform != null)
        {
            originalLineScale = lineTransform.localScale;
            Debug.Log($"Line ���� ������: {originalLineScale}");

            // �������� 0�̸� �⺻������ ����
            if (originalLineScale.magnitude < 0.001f)
            {
                Debug.LogWarning("Line ���� �������� 0�� ����� �⺻�� ����");
                originalLineScale = Vector3.one;
            }
        }

        // �ʱ� ���� ����
        SetActiveState(isActive);


    }

    public void SetActiveState(bool active)
    {
        isActive = active;

        // ���ӿ�����Ʈ�� ��Ȱ��ȭ ���¸� �ִϸ��̼��� �������� ����
        if (!gameObject.activeInHierarchy) return;

        // ���� �ִϸ��̼� �ڷ�ƾ ����
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        // ������ ȿ�� ����
        ApplyRendererEffects();

        // ��ƼŬ �ý��� ����
        foreach (var ps in activeParticles)
        {
            if (ps == null) continue;

            if (active)
            {
                if (!ps.isPlaying) ps.Play();
            }
            else
            {
                if (ps.isPlaying) ps.Stop();
            }
        }

        // ����Ʈ ����
        foreach (var light in activeLights)
        {
            if (light == null) continue;
            light.enabled = active;
        }

        // Ȱ��ȭ ���� ���ӿ�����Ʈ ����
        foreach (var go in activeGameObjects)
        {
            if (go == null) continue;
            go.SetActive(active);
        }

        // Ȱ��ȭ ���� ���
        if (active && activationSound != null)
        {
            activationSound.Play();
        }

        // �ִϸ��̼� ����
        if (active)
        {
            animationCoroutine = StartCoroutine(AnimateStarsAndLines());
        }
        else
        {
            // ��Ȱ��ȭ �� �ʱ� ���·� ����
            ResetObjectsState();
        }
    }

    // ������ ȿ�� ����
    private void ApplyRendererEffects()
    {
        foreach (Renderer renderer in deviceRenderers)
        {
            if (renderer == null) continue;

            foreach (Material mat in renderer.materials)
            {
                // �⺻ ���� ����
                mat.color = isActive ? activeColor : inactiveColor;

                // �߱� ȿ�� ����
                if (mat.HasProperty("_EmissionColor"))
                {
                    if (isActive)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }

                // FlatKit ���̴� Ưȭ ����
                if (mat.HasProperty("_RimEnabled"))
                {
                    mat.SetInt("_RimEnabled", isActive ? 1 : 0);

                    if (isActive && mat.HasProperty("_FlatRimColor"))
                    {
                        mat.SetColor("_FlatRimColor", emissionColor * 1.5f);
                        mat.SetFloat("_FlatRimSize", 0.6f);
                        mat.SetFloat("_FlatRimEdgeSmoothness", 0.3f);
                    }
                }

                // ����ŧ�� ȿ��
                if (mat.HasProperty("_SpecularEnabled"))
                {
                    mat.SetInt("_SpecularEnabled", isActive ? 1 : 0);

                    if (isActive && mat.HasProperty("_FlatSpecularColor"))
                    {
                        mat.SetColor("_FlatSpecularColor", emissionColor);
                        mat.SetFloat("_FlatSpecularSize", 0.5f);
                        mat.SetFloat("_FlatSpecularEdgeSmoothness", 0.3f);
                    }
                }
            }
        }
    }

    // ����� ���� �ִϸ��̼�
    // ����� ���� �ִϸ��̼�
    private IEnumerator AnimateStarsAndLines()
    {
        if (starsTransform == null && lineTransform == null) yield break;

        // �ֻ��� Transform ���� (���ڸ� ��ü)
        Transform constellationTransform = transform;
        Vector3 originalConstellationScale = constellationTransform.localScale;

        float time = 0f;

        while (isActive && gameObject.activeInHierarchy)
        {
            time += Time.deltaTime;

            // ��ü ���ڸ��� ���� ���� �Ƶ� ȿ��
            float pulseFactor = (Mathf.Sin(time * pulseSpeed) * 0.05f) + 1.0f; // �Ƶ� �� ����

            // ��ü ���ڸ� ������ �ִϸ��̼�
            constellationTransform.localScale = originalConstellationScale * pulseFactor;

            // ���ڸ� ��ü ȸ�� (���� �� ȸ�� �ӵ��� �����ϰ�)
            constellationTransform.Rotate(Vector3.up, rotationSpeed * 0.5f * Time.deltaTime);

            // ��Ƽ���� �߱� ȿ�� ������Ʈ
            float brightness = 0.8f + 0.4f * (Mathf.Sin(time * 1.2f) * 0.5f + 0.5f);

            if (starsRenderer != null && starsRenderer.material.HasProperty("_EmissionColor"))
            {
                starsRenderer.material.SetColor("_EmissionColor", emissionColor * emissionIntensity * brightness);
            }

            if (lineRenderer != null && lineRenderer.material.HasProperty("_EmissionColor"))
            {
                lineRenderer.material.SetColor("_EmissionColor", emissionColor * lineBrightness * brightness);
            }

            yield return null;
        }

        // ���ӿ�����Ʈ�� ��Ȱ��ȭ�Ǹ� �ʱ� ���·� ����
        constellationTransform.localScale = originalConstellationScale;

        // ȸ���� �ʱ�ȭ�� �ʿ��� �� ����
        if (!isActive)
        {
            constellationTransform.rotation = Quaternion.identity;
        }
    }

    // �ʱ� ���·� ����
    private void ResetObjectsState()
    {
        // Stars ������Ʈ ����
        if (starsTransform != null)
        {
            starsTransform.localScale = originalStarsScale;
            starsTransform.rotation = Quaternion.identity;
        }

        // Line ������Ʈ ����
        if (lineTransform != null)
        {
            lineTransform.localScale = originalLineScale;
        }
    }

    // Ȱ��ȭ �ִϸ��̼� ��� (�ܺ� ȣ���)
    public void PlayActivationAnimation()
    {
        if (!isActive) return;

        // Ȱ��ȭ �ִϸ��̼� ���
        Animation anim = GetComponent<Animation>();
        if (anim != null && anim.clip != null)
        {
            anim.Play();
        }

        // Ȱ��ȭ ���� ���
        if (activationSound != null)
        {
            activationSound.Play();
        }
    }
}