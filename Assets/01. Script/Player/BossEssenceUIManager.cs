using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class BossEssenceUIManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image essenceBarFill;
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private TextMeshProUGUI essenceName;

    [Header("Visual Settings")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color highStateColor = Color.magenta;  // 70% �̻�
    [SerializeField] private Color maxStateColor = Color.yellow;    // 100%
    [SerializeField] private float barUpdateDuration = 0.2f;
 
    [Header("Vignette Effect")]
    [SerializeField] private Material vignetteMaterial;
    [SerializeField] private float transitionDuration = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip heartbeatSound;
    [SerializeField] private float heartbeatVolume = 0.5f;
    private IBossEssenceSystem currentEssenceSystem;
    private void Awake()
    {
        // AudioSource�� ���ٸ� �߰�
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            audioSource.volume = heartbeatVolume;
        }
    }
    public void Initialize(IBossEssenceSystem essenceSystem)
    {
        currentEssenceSystem = essenceSystem;

        // �̺�Ʈ ����
        essenceSystem.OnEssenceChanged += UpdateEssenceBar;
        essenceSystem.OnEssenceStateChanged += UpdateEssenceState;
        essenceSystem.OnMaxEssenceStateChanged += UpdateMaxEssenceState;
        essenceName.text = currentEssenceSystem.BossEssenceName; 
        // �ʱ� ���� ����
        essenceBarFill.fillAmount = 0f;
        essenceBarFill.color = normalColor;
        UpdatePercentageText(0);

        Debug.Log("��~��");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // �׽�Ʈ�� Ű
        {
            // ������ ���� �׽�Ʈ
            currentEssenceSystem.IncreaseEssence(10f);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // ������ ���� �׽�Ʈ
            currentEssenceSystem.DecreaseEssence(10f);
        }
    }
    private void UpdateEssenceBar(float value)
    {
        float fillAmount = value / 100f;
        essenceBarFill.DOFillAmount(fillAmount, barUpdateDuration);
        UpdatePercentageText(value);
    }

    private void UpdateEssenceState()
    {
        if (currentEssenceSystem.IsInEssenceState)
        {
            essenceBarFill.DOColor(highStateColor, barUpdateDuration);
            // ���Ʈ ������ �� ���� ����
            DOTween.To(() => vignetteMaterial.GetFloat("_VignetteIntensity"),
                value => vignetteMaterial.SetFloat("_VignetteIntensity", value),
                0.75f, transitionDuration)  // 0.7f�� ���� ����
                .SetEase(Ease.InOutQuad);

          

            // ���� �ڵ� ���� ����
            if (!audioSource.isPlaying)
            {
                audioSource.clip = heartbeatSound;
                audioSource.Play();
                // ���� ���̵���
                DOTween.To(() => audioSource.volume,
                    value => audioSource.volume = value,
                    heartbeatVolume, transitionDuration);
            }
        }
        else
        {
            essenceBarFill.DOColor(normalColor, barUpdateDuration);
            // ���̵�ƿ��� �״��
            DOTween.To(() => vignetteMaterial.GetFloat("_VignetteIntensity"),
                value => vignetteMaterial.SetFloat("_VignetteIntensity", value),
                0f, transitionDuration)
                .SetEase(Ease.InOutQuad);

            //�Ҹ�off
            DOTween.To(() => audioSource.volume,
               value => audioSource.volume = value,
               0f, transitionDuration)
               .OnComplete(() => audioSource.Stop());
        }
    }

    private void UpdateMaxEssenceState()
    {
        if (currentEssenceSystem.IsMaxEssence)
        {
            essenceBarFill.DOColor(maxStateColor, barUpdateDuration);
        }
    }

    private void UpdatePercentageText(float value)
    {
        percentageText.text = $"{Mathf.Round(value)}%";
    }
   
    private void OnDestroy()
    {
        if (currentEssenceSystem != null)
        {
            currentEssenceSystem.OnEssenceChanged -= UpdateEssenceBar;
            currentEssenceSystem.OnEssenceStateChanged -= UpdateEssenceState;
            currentEssenceSystem.OnMaxEssenceStateChanged -= UpdateMaxEssenceState;
            if(vignetteMaterial != null)
            {
                vignetteMaterial.SetFloat("_VignetteIntensity", 0);
            }
            
                
        }

      
    }
}