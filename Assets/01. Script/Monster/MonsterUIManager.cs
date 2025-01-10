using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AttackData;

public class MonsterUIManager : MonoBehaviour
{

    [Header("References")]
    [SerializeField] private Canvas worldSpaceCanvas;
    private MonsterStatus monsterStatus;
    private MonsterClass monsterClass;
    private Camera mainCamera;
    [Header("UI Elements")]
    [SerializeField] private Image healthBar;
    

    [Header("Damage Text")]
    [SerializeField] private GameObject damageTextPrefab;
    private readonly Vector2[] damageTextOffsets = new Vector2[]
    {
        new Vector2(0.5f, 2f),
        new Vector2(-0.5f, 2f),
        
    };

    private int maxHealth;

  

    private void Start()
    {
        mainCamera = Camera.main;
        monsterStatus = GetComponent<MonsterStatus>();
        InitializeReferences();
        SetupUI();
    }
    private void LateUpdate()
    {
        if (mainCamera != null && worldSpaceCanvas != null)
        {
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(
                worldSpaceCanvas.transform.position - mainCamera.transform.position
            );
        }
    }
    private void InitializeReferences()
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
            }
        }
    }

    private void SetupUI()
    {
        if (monsterClass != null)
        {
            UpdateHealthUI(monsterClass.CurrentHealth);
        }
    }

    public void UpdateHealthUI(int currentHealth)
    {
        if (healthBar != null)
            healthBar.fillAmount = (float)currentHealth / maxHealth;
       
    }
    public void SpawnDamageText(int damage, AttackType attackType)
    {
        if (damageTextPrefab == null) return;

        Vector2 offset = damageTextOffsets[Random.Range(0, damageTextOffsets.Length)];

        GameObject damageTextObj = Instantiate(damageTextPrefab, worldSpaceCanvas.transform);
        damageTextObj.transform.position = transform.position + new Vector3(offset.x, offset.y, 0);

        TextMeshProUGUI damageText = damageTextObj.GetComponent<TextMeshProUGUI>();
        if (damageText != null)
        {
            switch (attackType)
            {
                //case AttackType.Critical:
                //    damageText.color = Color.red;
                //    damageText.fontSize *= 1.2f;
                //    break;
                //case AttackType.Dot:
                //    damageText.color = Color.green;
                //    break;
                default:
                    damageText.color = Color.white;
                    break;
            }

            damageText.text = damage.ToString();
            StartCoroutine(AnimateDamageText(damageTextObj));
        }
    }

    private System.Collections.IEnumerator AnimateDamageText(GameObject textObj)
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
