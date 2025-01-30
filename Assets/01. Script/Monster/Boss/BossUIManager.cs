// BossUIManager.cs
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AttackData;

public class BossUIManager : MonsterUIManager
{
    [Header("Boss Info")]
    [SerializeField] private Image phaseBackgroundImage;
    [SerializeField] private Image[] phaseImages;
    [SerializeField] private TextMeshProUGUI bossName;

    [Header("Pattern Success UI")]
    [SerializeField] private GameObject patternSuccessGroup;
    [SerializeField] private Image patternSuccessIcon;
    [SerializeField] private TextMeshProUGUI patternSuccessText;

    [Header("State System")]
    [SerializeField] private Transform stateGroup;
    [SerializeField] private GameObject stateIconPrefab;
    private Dictionary<string, GameObject> activeStates = new Dictionary<string, GameObject>();

    [Header("State Sprites")]
    [SerializeField] private Sprite groggySprite;
    [SerializeField] private Sprite rageSprite;
    [SerializeField] private Sprite defenseDownSprite;
    [SerializeField] private Sprite attackUpSprite;
    [SerializeField] private Sprite bleedingSprite;
    [SerializeField] private Sprite poisonSprite;
    [SerializeField] private Sprite stunSprite;

    private BossMonster bossMonster;
    private BossData bossData;
    private int currentPhase = 0;
    public enum StateType
    {
        Groggy,
        Rage,
        DefenseDown,
        AttackUp,
        Bleeding,
        Poison,
        Stun
    }
    public override void Initialize(IMonsterClass monster)
    {
        base.Initialize(monster);

        bossMonster = monster as BossMonster;
        if (bossMonster != null)
        {
            bossData = bossMonster.GetMonsterData() as BossData;
            InitializeBossUI();
            bossName.text = bossMonster.MONSTERNAME;
        }
    }

    private void InitializeBossUI()
    {
        if (patternSuccessGroup != null)
        {
            patternSuccessGroup.SetActive(false);
        }

        UpdatePhaseUI(0);
    }

    #region Phase Management
    public void UpdatePhaseUI()
    {
        if (bossMonster != null)
        {
            UpdateHealthUI(bossMonster.CurrentHealth);
            UpdatePhaseUI(bossMonster.CurrentPhase);
        }
    }

    private void UpdatePhaseUI(int newPhase)
    {
        if (newPhase == currentPhase || phaseImages == null ||
            newPhase >= phaseImages.Length) return;

        currentPhase = newPhase;
        UpdatePhaseVisuals(newPhase);
    }

    private void UpdatePhaseVisuals(int phaseIndex)
    {
        for (int i = 0; i < phaseImages.Length; i++)
        {
            if (i == phaseIndex)
            {
                phaseImages[i].gameObject.SetActive(true);
                phaseImages[i].DOFade(1f, 0.5f);

                if (bossData.phaseColors != null && phaseIndex < bossData.phaseColors.Length)
                {
                    phaseImages[i].DOColor(bossData.phaseColors[phaseIndex], 0.5f);
                }
            }
            else
            {
                phaseImages[i].DOFade(0f, 0.3f)
                    .OnComplete(() => phaseImages[i].gameObject.SetActive(false));
            }
        }
    }
    #endregion

    #region Pattern Success Management
    public void UpdatePatternSuccess(AttackPatternData pattern, int currentSuccess)
    {
        if (patternSuccessGroup == null) return;

        patternSuccessGroup.SetActive(true);

        if (patternSuccessText != null)
        {
            patternSuccessText.text = $"{currentSuccess}/{pattern.requiredSuccessCount}";
        }

        if (patternSuccessIcon != null)
        {
            patternSuccessIcon.transform.DOScale(1.2f, 0.2f)
                .SetLoops(2, LoopType.Yoyo)
                .SetEase(Ease.OutBack);
        }
    }
    #endregion

    #region State System
    public void AddState(StateType type, float duration = 0f)
    {
        string stateId = type.ToString();
        Sprite stateSprite = GetSpriteForState(type);

        if (stateSprite == null)
        {
            Debug.LogWarning($"No sprite found for state type: {type}");
            return;
        }

        AddState(stateId, stateSprite, duration);
    }

    private Sprite GetSpriteForState(StateType type)
    {
        return type switch
        {
            StateType.Groggy => groggySprite,
            StateType.Rage => rageSprite,
            StateType.DefenseDown => defenseDownSprite,
            StateType.AttackUp => attackUpSprite,
            StateType.Bleeding => bleedingSprite,
            StateType.Poison => poisonSprite,
            StateType.Stun => stunSprite,
            _ => null
        };
    }

    private void AddState(string stateId, Sprite stateIcon, float duration = 0f)
    {
        if (activeStates.ContainsKey(stateId))
        {
            UpdateStateDuration(stateId, duration);
            return;
        }

        GameObject stateObj = Instantiate(stateIconPrefab, stateGroup);
        if (stateObj.TryGetComponent<Image>(out Image stateImage))
        {
            stateImage.sprite = stateIcon;
        }

        RectTransform rect = stateObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30, 30);
        stateObj.transform.localPosition += Vector3.right * 50;

        stateObj.transform.DOLocalMoveX(0, 0.3f).SetEase(Ease.OutBack);
        activeStates.Add(stateId, stateObj);

        if (duration > 0)
        {
            StartCoroutine(RemoveStateAfterDuration(stateId, duration));
        }
    }

    public void RemoveState(StateType type)
    {
        RemoveState(type.ToString());
    }

    private void RemoveState(string stateId)
    {
        if (!activeStates.ContainsKey(stateId)) return;

        GameObject stateObj = activeStates[stateId];
        activeStates.Remove(stateId);

        stateObj.transform.DOLocalMoveX(-50, 0.3f).SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                Destroy(stateObj);
                RearrangeStateIcons();
            });
    }

    private void RearrangeStateIcons()
    {
        foreach (var stateObj in activeStates.Values)
        {
            stateObj.transform.DOScale(1.1f, 0.1f).SetLoops(2, LoopType.Yoyo);
        }
    }

    private IEnumerator RemoveStateAfterDuration(string stateId, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveState(stateId);
    }

    private void UpdateStateDuration(string stateId, float newDuration)
    {
        if (!activeStates.ContainsKey(stateId)) return;

        StopCoroutine($"RemoveStateAfterDuration_{stateId}");
        if (newDuration > 0)
        {
            StartCoroutine(RemoveStateAfterDuration(stateId, newDuration));
        }
    }

    public void ClearAllStates()
    {
        foreach (var stateId in new List<string>(activeStates.Keys))
        {
            RemoveState(stateId);
        }
    }
    #endregion

    public override void SpawnDamageText(int damage)
    {
        base.SpawnDamageText(damage);

        if (bossMonster != null && damage > bossMonster.CurrentAttackPower * 1.5f)
        {
            // 크리티컬 히트 이펙트 추가 가능
        }
    }

    protected override void OnDestroy()
    {
        DOTween.Kill(transform);
        base.OnDestroy();
    }
}