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
    [SerializeField] private Color highStateColor = Color.magenta;  // 70% 이상
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
        // AudioSource가 없다면 추가
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

        // 이벤트 구독
        essenceSystem.OnEssenceChanged += UpdateEssenceBar;
        essenceSystem.OnEssenceStateChanged += UpdateEssenceState;
        essenceSystem.OnMaxEssenceStateChanged += UpdateMaxEssenceState;
        essenceName.text = currentEssenceSystem.BossEssenceName; 
        // 초기 상태 설정
        essenceBarFill.fillAmount = 0f;
        essenceBarFill.color = normalColor;
        UpdatePercentageText(0);

        Debug.Log("시~작");
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))  // 테스트용 키
        {
            // 게이지 증가 테스트
            currentEssenceSystem.IncreaseEssence(10f);
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            // 게이지 감소 테스트
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
            // 비네트 강도를 더 높게 설정
            DOTween.To(() => vignetteMaterial.GetFloat("_VignetteIntensity"),
                value => vignetteMaterial.SetFloat("_VignetteIntensity", value),
                0.75f, transitionDuration)  // 0.7f로 강도 증가
                .SetEase(Ease.InOutQuad);

          

            // 심장 박동 사운드 시작
            if (!audioSource.isPlaying)
            {
                audioSource.clip = heartbeatSound;
                audioSource.Play();
                // 볼륨 페이드인
                DOTween.To(() => audioSource.volume,
                    value => audioSource.volume = value,
                    heartbeatVolume, transitionDuration);
            }
        }
        else
        {
            essenceBarFill.DOColor(normalColor, barUpdateDuration);
            // 페이드아웃은 그대로
            DOTween.To(() => vignetteMaterial.GetFloat("_VignetteIntensity"),
                value => vignetteMaterial.SetFloat("_VignetteIntensity", value),
                0f, transitionDuration)
                .SetEase(Ease.InOutQuad);

            //소리off
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