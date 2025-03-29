using UnityEngine;

public class GameplayModifiers : MonoBehaviour
{
    public static GameplayModifiers Instance { get; private set; }

    // ��� �����ڵ�
    public float RareAbilityChanceMultiplier { get; set; } = 1f;
    public bool EnableSameTypeAbilityGuarantee { get; set; } = false;
    public float ExtraRewardChance { get; set; } = 0f;
    // ��Ÿ �����ڵ�...

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