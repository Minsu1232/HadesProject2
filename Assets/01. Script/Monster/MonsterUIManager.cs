// MonsterUIManager.cs
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AttackData;
using DG.Tweening;

public class MonsterUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Canvas worldSpaceCanvas;
    protected ICreatureStatus monsterStatus;
    protected IMonsterClass monsterClass;
    protected Camera mainCamera;

    [Header("UI Elements")]
    [SerializeField] protected Image healthBar;
    [SerializeField] protected Image armorBar;

    [Header("Damage Text")]
    [SerializeField] protected GameObject damageTextPrefab;
    protected readonly Vector2[] damageTextOffsets = new Vector2[]
    {
       new Vector2(0.5f, 2f),
       new Vector2(-0.5f, 2f),
    };

    protected int maxHealth;
    protected int maxArmor;
    public virtual void Initialize(IMonsterClass monster)
    {
        monsterClass = monster;
        // MonsterStatus로 GetComponent 후 ICreatureStatus로 할당
        MonsterStatus status = GetComponent<MonsterStatus>();
        monsterStatus = status;  // 인터페이스로 암시적 변환
        InitializeReferences();
        SetupUI();

        if (monsterClass != null)
        {
            monsterClass.OnArmorBreak += HandleArmorBreak;
        }
    }
    protected virtual void Start()
    {
        if (monsterClass == null)
        {
            mainCamera = Camera.main;
            // MonsterStatus로 GetComponent 후 GetMonsterClass 호출
            MonsterStatus status = GetComponent<MonsterStatus>();
            if(status == null)
            {
                Debug.LogWarning("널인디용");
            }
            Initialize(status.GetMonsterClass());
        }
    }

    protected virtual void OnDestroy()
    {
        if (monsterClass != null)
        {
            monsterClass.OnArmorBreak -= HandleArmorBreak;
        }
    }

    protected virtual void LateUpdate()
    {
        if (mainCamera != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(
                worldSpaceCanvas.transform.position - mainCamera.transform.position
            );
        }
    }

    protected virtual void InitializeReferences()
    {
        if (worldSpaceCanvas != null)
        {
            worldSpaceCanvas.renderMode = RenderMode.WorldSpace;
            worldSpaceCanvas.worldCamera = Camera.main;
        }

        if (monsterStatus != null)
        {
            monsterClass = monsterStatus.GetMonsterClass();
            if (monsterClass != null)
            {
                maxHealth = monsterClass.GetMonsterData().initialHp; 
                maxArmor = monsterClass.GetMonsterData().armorValue; 
            }
        }
    }

    protected virtual void SetupUI()
    {
        if (monsterClass != null)
        {
            UpdateHealthUI(monsterClass.CurrentHealth);
            UpdateArmorUI(monsterClass.CurrentArmor);
        }
    }

    public virtual void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
        {
            maxHealth = monsterClass.MaxHealth;
            healthBar.fillAmount = (float)currentHealth / maxHealth;
        }
    }

    public virtual void UpdateArmorUI(int currentHealth)
    {
        if (armorBar != null && monsterClass != null)
        {
            armorBar.fillAmount = (float)currentHealth / maxArmor;
        }
    }

    protected virtual void HandleArmorBreak()
    {
        if (armorBar != null)
        {
            armorBar.DOFade(0f, 0.5f).OnComplete(() => {
                armorBar.gameObject.SetActive(false);
            });
            SpawnArmorBreakText();
        }
    }

    protected virtual void SpawnArmorBreakText()
    {
        if (damageTextPrefab == null) return;

        GameObject armorBreakTextObj = Instantiate(damageTextPrefab, worldSpaceCanvas.transform);
        armorBreakTextObj.transform.position = transform.position + new Vector3(0, 2.5f, 0);

        TextMeshProUGUI armorBreakText = armorBreakTextObj.GetComponent<TextMeshProUGUI>();
        if (armorBreakText != null)
        {
            armorBreakText.color = Color.yellow;
            armorBreakText.fontSize *= 1.2f;
            armorBreakText.text = "Baam";
            HitStopManager.TriggerHitStop(0.5f, 3);
            StartCoroutine(AnimateDamageText(armorBreakTextObj));
        }
    }

    public virtual void SpawnDamageText(int damage)
    {
        if (damageTextPrefab == null) return;

        Vector2 offset = damageTextOffsets[Random.Range(0, damageTextOffsets.Length)];
        GameObject damageTextObj = Instantiate(damageTextPrefab, worldSpaceCanvas.transform);
        damageTextObj.transform.position = transform.position + new Vector3(offset.x, offset.y, 0);

        TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.color = Color.white;
            damageText.text = damage.ToString();
            StartCoroutine(AnimateDamageText(damageTextObj));
        }
    }

    protected virtual IEnumerator AnimateDamageText(GameObject textObj)
    {
        float duration = 1f;
        float elapsed = 0f;
        Vector3 startPos = textObj.transform.position;
        Vector3 endPos = startPos + Vector3.up * 1f;

        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        Color startColor = tmp.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            textObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            tmp.color = Color.Lerp(startColor, endColor, t);

            yield return null;
        }

        Destroy(textObj);
    }
}