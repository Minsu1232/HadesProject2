using UnityEngine;

public class GameplayModifiers : MonoBehaviour
{
    public static GameplayModifiers Instance { get; private set; }

    // 모든 수정자들
    public float RareAbilityChanceMultiplier { get; set; } = 1f;
    public bool EnableSameTypeAbilityGuarantee { get; set; } = false;
    public float ExtraRewardChance { get; set; } = 0f;
    // 기타 수정자들...

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}