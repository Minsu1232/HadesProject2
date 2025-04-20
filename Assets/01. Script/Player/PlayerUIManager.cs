using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterAttackBase characterAttack;
    private PlayerClass player;
    private WeaponManager weaponManager;

    [Header("Status UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private TextMeshProUGUI criticalChanceText;

    [Header("Screen Space Weapon UI")]
    [SerializeField] private TextMeshProUGUI weaponGageText;
    [SerializeField] private Image weaponGageBar;

    [Header("World Space Charge UI")]
    [SerializeField] private Canvas chargeCanvas;
    [SerializeField] private Image chargeGageBar;
    [SerializeField] private TextMeshProUGUI chargeTimeText;

    private void Start()
    {
        InitializePlayerReferences();
        SubscribeToEvents();
        UpdateAllUI();
        SetChargeUIActive(false);
    }

    private void InitializePlayerReferences()
    {
        player = GameInitializer.Instance.GetPlayerClass();
        if (player == null || characterAttack == null)
        {
            Debug.LogError("Required references are missing!");
            return;
        }
    }

    private void SubscribeToEvents()
    {
        if (player != null)
        {
            player.OnWeaponSelected += HandleWeaponChanged;
            var stats = player.PlayerStats;
            if (stats != null)
            {
                stats.OnHealthChanged += UpdateHealthUI;                
                stats.OnAttackPowerChanged += UpdateAttackPowerUI;
                stats.OnAttackSpeedChanged += UpdateAttackSpeedUI;
                stats.OnSpeedChanged += UpdateSpeedUI;
                stats.OnCriticalChanceChanged += UpdateCriticalChanceUI;
            }
        }

        if (characterAttack != null)
        {
            characterAttack.OnChargeTimeUpdated += UpdateChargeTimeUI;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (player != null)
        {
            player.OnWeaponSelected -= HandleWeaponChanged;
            var stats = player.PlayerStats;
            if (stats != null)
            {
                stats.OnHealthChanged -= UpdateHealthUI;
                stats.OnAttackPowerChanged -= UpdateAttackPowerUI;
                stats.OnAttackSpeedChanged -= UpdateAttackSpeedUI;
                stats.OnSpeedChanged -= UpdateSpeedUI;
                stats.OnCriticalChanceChanged -= UpdateCriticalChanceUI;
            }
        }

        if (characterAttack != null)
        {
            characterAttack.OnChargeTimeUpdated -= UpdateChargeTimeUI;
        }

        UnsubscribeFromWeaponEvents();
    }

    private void HandleWeaponChanged(IWeapon newWeapon)
    {
        UnsubscribeFromWeaponEvents();

        // 무기가 null이거나 무기가 해제된 경우
        if (newWeapon == null)
        {
            ClearWeaponUI();
            return;
        }

        SetupNewWeapon(newWeapon);
    }

    private void UnsubscribeFromWeaponEvents()
    {
        if (weaponManager != null)
        {
            weaponManager.OnGageChanged -= UpdateWeaponGageUI;
        }
    }

    private void SetupNewWeapon(IWeapon weapon)
    {
        weaponManager = weapon as WeaponManager;
        bool hasWeapon = weaponManager != null;
        SetWeaponUIActive(hasWeapon);

        if (hasWeapon)
        {
            weaponManager.OnGageChanged += UpdateWeaponGageUI;
            UpdateWeaponGageUI(weaponManager.CurrentGage);
            UpdateGageColor();
        }
    }

    private void UpdateAllUI()
    {
        if (player?.PlayerStats == null) return;

        var stats = player.PlayerStats;
        UpdateHealthUI(stats.Health,stats.MaxHealth);
        UpdateAttackPowerUI(stats.AttackPower);
        UpdateAttackSpeedUI(stats.AttackSpeed);
        UpdateSpeedUI(stats.Speed);
        UpdateCriticalChanceUI(stats.CriticalChance);
    }

    private void UpdateHealthUI(int health,int maxHealth)
    {
        if (healthText != null)
            healthText.text = $"{health}/{maxHealth}";

        if (healthBar != null)
            healthBar.fillAmount = (float)health / maxHealth;
    }

   


    private void UpdateWeaponGageUI(int gage)
    {
        if (weaponGageText != null)
            weaponGageText.text = $"{gage}/100";

        if (weaponGageBar != null)
            weaponGageBar.fillAmount = gage / 100f;
    }

    private void UpdateChargeTimeUI(float chargeTime)
    {
        Debug.Log(player.weaponType);
        if (player.weaponType == PlayerClass.WeaponType.None) return;
        float chargeRatio = Mathf.Clamp01(chargeTime / weaponManager.MaxChargeTime);

        if (chargeGageBar != null)
            chargeGageBar.fillAmount = chargeRatio;

        if (chargeTimeText != null)
            chargeTimeText.text = $"{chargeTime:F1}s";

        SetChargeUIActive(chargeTime > 0);
    }

    private void UpdateGageColor()
    {
        if (weaponGageBar != null && weaponManager != null)
        {
            weaponGageBar.color = weaponManager.GageColor;
        }
        if (chargeGageBar != null && weaponManager != null)
        {
            chargeGageBar.color = weaponManager.GageColor;
        }
    }

    private void UpdateAttackPowerUI(int attackPower) =>
        SafeUpdateText(attackPowerText, $"ATK: {attackPower}");

    private void UpdateAttackSpeedUI(int attackSpeed) =>
        SafeUpdateText(attackSpeedText, $"SPD: {attackSpeed}");

    private void UpdateSpeedUI(float speed) =>
        SafeUpdateText(speedText, $"MOV: {speed:F1}");

    private void UpdateCriticalChanceUI(float critChance) =>
        SafeUpdateText(criticalChanceText, $"CRIT: {critChance:F1}%");

    private void SafeUpdateText(TMP_Text textComponent, string newText)
    {
        if (textComponent != null)
            textComponent.text = newText;
    }

    private void SetWeaponUIActive(bool active)
    {
        if (weaponGageText != null)
            weaponGageText.gameObject.SetActive(active);
        if (weaponGageBar != null)
            weaponGageBar.gameObject.SetActive(active);
    }


    // 무기 UI 초기화 메서드 추가
    private void ClearWeaponUI()
    {
        weaponManager = null;
        SetWeaponUIActive(false);
        SetChargeUIActive(false);
        Debug.Log("무기 UI가 초기화되었습니다.");
    }
    private void SetChargeUIActive(bool active)
    {
        if (chargeCanvas != null)
            chargeCanvas.enabled = active;
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }
}