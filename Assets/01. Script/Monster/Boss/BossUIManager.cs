// BossUIManager.cs
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AttackData;

public class BossUIManager : MonsterUIManager
{
    [Header("Boss UI References")]
    [SerializeField] private Canvas screenSpaceCanvas;

    [Header("Boss Health Bar")]
    [SerializeField] private Image bossHealthBarBackground;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI phaseNameText;

    [Header("Phase UI")]
    [SerializeField] private Image[] phaseThresholdMarkers;
    [SerializeField] private Image phaseBackgroundBar;

    [Header("Rage Mode UI")]
    [SerializeField] private CanvasGroup rageModeGroup;
    [SerializeField] private Image rageModeBackgroundEffect;

    private BossMonster bossMonster;
    private BossData bossData;

    protected override void Start()
    {
        base.Start();

        bossMonster = monsterStatus.GetMonsterClass() as BossMonster;
        if (bossMonster != null)
        {
            bossData = bossMonster.GetMonsterData() as BossData;
            InitializeBossUI();
        }
    }

    private void InitializeBossUI()
    {
        if (bossNameText != null)
        {
            bossNameText.text = bossData.monsterName;
            if (bossData.showPhaseNames)
            {
                phaseNameText.gameObject.SetActive(true);
                UpdatePhaseText(0);
            }
        }

        InitializePhaseMarkers();
        SetupRageModeUI(false);
    }

    private void InitializePhaseMarkers()
    {
        if (phaseThresholdMarkers != null && bossData.phaseData != null)
        {
            for (int i = 0; i < phaseThresholdMarkers.Length && i < bossData.phaseData.Count; i++)
            {
                float threshold = bossData.phaseData[i].healthThreshold;
                phaseThresholdMarkers[i].fillAmount = threshold;

                if (bossData.phaseColors != null && i < bossData.phaseColors.Length)
                {
                    phaseThresholdMarkers[i].color = bossData.phaseColors[i];
                }
            }
        }
    }

    public void UpdatePhase(int newPhase)
    {
        UpdatePhaseText(newPhase);
        PlayPhaseTransitionEffect(newPhase);
    }

    private void UpdatePhaseText(int phase)
    {
        if (phaseNameText != null && bossData.phaseData != null && phase < bossData.phaseData.Count)
        {
            phaseNameText.text = bossData.phaseData[phase].phaseName;

            phaseNameText.transform.DOScale(1.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo);

            if (bossData.phaseColors != null && phase < bossData.phaseColors.Length)
            {
                phaseNameText.DOColor(bossData.phaseColors[phase], 0.3f);
            }
        }
    }

    private void PlayPhaseTransitionEffect(int phase)
    {
        if (healthBar != null && bossData.phaseColors != null && phase < bossData.phaseColors.Length)
        {
            healthBar.DOColor(bossData.phaseColors[phase], 0.5f);
        }

        if (rageModeBackgroundEffect != null)
        {
            rageModeBackgroundEffect.DOFade(0.3f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => rageModeBackgroundEffect.gameObject.SetActive(false));
        }
    }

    public override void UpdateHealthUI(int currentHealth)
    {
        base.UpdateHealthUI(currentHealth);

        if (currentHealth < maxHealth * 0.3f)
        {
            healthBar.DOColor(Color.red, 0.3f).SetLoops(-1, LoopType.Yoyo);
        }
    }

    public void SetupRageModeUI(bool active)
    {
        if (rageModeGroup != null)
        {
            float targetAlpha = active ? 1f : 0f;
            rageModeGroup.DOFade(targetAlpha, 0.5f);

            if (active)
            {
                healthBar.DOColor(Color.red, 0.3f);
                phaseBackgroundBar?.DOColor(new Color(1f, 0f, 0f, 0.3f), 0.3f);
            }
            else
            {
                Color currentPhaseColor = bossData.phaseColors?[bossMonster.CurrentPhase] ?? Color.white;
                healthBar.DOColor(currentPhaseColor, 0.3f);
                phaseBackgroundBar?.DOColor(new Color(0f, 0f, 0f, 0.3f), 0.3f);
            }
        }
    }

    public override void SpawnDamageText(int damage, AttackType attackType)
    {
        base.SpawnDamageText(damage, attackType);

        if (damage > bossMonster.CurrentAttackPower * 1.5f)
        {
            PlayCriticalHitEffect();
        }
    }

    private void PlayCriticalHitEffect()
    {
        if (rageModeBackgroundEffect != null)
        {
            rageModeBackgroundEffect.gameObject.SetActive(true);
            rageModeBackgroundEffect.DOFade(0.2f, 0.1f)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => rageModeBackgroundEffect.gameObject.SetActive(false));
        }
    }

    protected override void OnDestroy()
    {
        DOTween.Kill(transform);
        base.OnDestroy();
    }

    //protected override void AnimateDamageText(GameObject textObj)
    //{
    //    StartCoroutine(base.AnimateDamageText(textObj));
    //}
}