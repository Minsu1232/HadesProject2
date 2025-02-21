using UnityEngine;

public class ChapterBossRoom : MonoBehaviour
{
    [SerializeField] private BossEssenceUIManager essenceUIPrefab;
    private BossEssenceUIManager essenceUI;
    private Canvas bossUICanvas;

    private void Awake()
    {
        bossUICanvas = GetComponentInChildren<Canvas>();
        if (bossUICanvas == null)
        {
            Debug.LogError("BossRoom needs a UI Canvas!");
        }
    }

    public void OnBossSpawned(BossAI bossAI)
    {
        if (bossAI.GetStatus().GetMonsterClass() is AlexanderBoss alexanderBoss)
        {
            InitializeBossUI(alexanderBoss);
        }
    }

    private void InitializeBossUI(AlexanderBoss boss)
    {
        if (essenceUI == null && bossUICanvas != null)
        {
            essenceUI = Instantiate(essenceUIPrefab, bossUICanvas.transform);
        }

        var essenceSystem = boss.GetEssenceSystem();
        essenceUI.Initialize(essenceSystem.BossEssenceName, essenceSystem);
    }

    private void OnDestroy()
    {
        if (essenceUI != null)
        {
            Destroy(essenceUI.gameObject);
        }
    }
}