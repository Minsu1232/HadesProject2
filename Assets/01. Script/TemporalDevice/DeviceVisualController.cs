using UnityEngine;

// �ð� ��ġ�� �ð��� ȿ���� �����ϴ� Ŭ����
public class DeviceVisualController : MonoBehaviour
{
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;
    [SerializeField] private Color emissionColor = new Color(0.5f, 0.5f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 2f;

    [SerializeField] private ParticleSystem[] activeParticles;
    [SerializeField] private Light[] activeLights;
    [SerializeField] private AudioSource activationSound;
    [SerializeField] private GameObject[] activeGameObjects;

    [SerializeField] private Renderer[] deviceRenderers;

    [SerializeField] bool isActive = false;

    private void Start()
    {
        // �������� �ڵ����� ã�� �ʾҴٸ� ã��
        if (deviceRenderers == null || deviceRenderers.Length == 0)
        {
            deviceRenderers = GetComponentsInChildren<Renderer>();
        }

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

        // �ʱ� ���� ����
        SetActiveState(isActive);
    }

    // ��ġ�� Ȱ�� ���� ����
    public void SetActiveState(bool active)
    {
        isActive = active;

        // ������ ���� �� �̹̼� ����
        foreach (var renderer in deviceRenderers)
        {
            foreach (Material mat in renderer.materials)
            {
                // �⺻ ���� ����
                mat.color = active ? activeColor : inactiveColor;

                // �̹̼� ����
                if (mat.HasProperty("_EmissionColor"))
                {
                    if (active)
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    }
                    else
                    {
                        mat.DisableKeyword("_EMISSION");
                    }
                }
            }
        }

        // ��ƼŬ �ý��� ����
        foreach (var ps in activeParticles)
        {
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
            light.enabled = active;
        }

        // Ȱ��ȭ ���� ���ӿ�����Ʈ ����
        foreach (var go in activeGameObjects)
        {
            go.SetActive(active);
        }

        // Ȱ��ȭ ���� ���
        if (active && activationSound != null)
        {
            activationSound.Play();
        }
    }

    // Ȱ��ȭ �ִϸ��̼� ��� (�ܺο��� ȣ�� ����)
    public void PlayActivationAnimation()
    {
        if (!isActive) return;

        // �߰� Ȱ��ȭ �ִϸ��̼� ���
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