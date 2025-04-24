// AttackAbility.cs - 공격 능력 클래스 (DungeonAbility 상속)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class AttackAbility : DungeonAbility
{
    public enum AttackAbilityType
    {
        ComboEnhancement,  // 콤보 강화 (연속타격 데미지 증가)
        WeakPrey,          // 약자 공격 (체력이 낮은 적에게 추가 피해)
        ChainStrike,       // 연쇄 타격 (추가 타격 확률)
        GaugeBoost,        // 게이지 강화 (무기 게이지 충전량 증가)
        ArmorCrush         // 방어력 파괴 (적 방어력 감소)
    }

    public AttackAbilityType attackType;
  

    // 생성자로 초기화
    public void Initialize(AttackAbilityType type, float value, string abilityName, string abilityDescription, Rarity abilityRarity)
    {
        attackType = type;
        effectValue = value;
        originalValue = value;

        id = $"attack_{type.ToString().ToLower()}";
        name = abilityName;
        description = abilityDescription;
        rarity = abilityRarity;
    }

    // CSV 데이터로 초기화하는 메서드 추가
    public static AttackAbility FromCSVData(Dictionary<string, string> csvData)
    {
        AttackAbility ability = new AttackAbility();

        // 필수 값 파싱
        string id = csvData["ID"];
        AttackAbilityType type = (AttackAbilityType)System.Enum.Parse(typeof(AttackAbilityType), csvData["Type"]);
        string name = csvData["Name"];
        string description = csvData["Description"];
        Rarity rarity = (Rarity)int.Parse(csvData["Rarity"]);
        float baseValue = float.Parse(csvData["BaseValue"]);
        // 레벨업 배율 파싱 추가
        if (csvData.ContainsKey("LevelMultiplier") && !string.IsNullOrEmpty(csvData["LevelMultiplier"]))
        {
            ability.levelMultiplier = float.Parse(csvData["LevelMultiplier"]);
        }

        // 능력 초기화
        ability.Initialize(type, baseValue, name, description, rarity);
        ability.id = id; // ID 값 덮어쓰기 (유니크한 ID 사용)

        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            string iconAddress = csvData["IconPath"];
            Debug.Log(iconAddress);
            // 애드레서블 비동기 로드
            Addressables.LoadAssetAsync<Sprite>(iconAddress).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    ability.icon = handle.Result;
                    Debug.Log($"아이콘 로드 성공: {iconAddress}");
                }
                else
                {
                    Debug.LogWarning($"아이콘을 로드할 수 없습니다: {iconAddress}");
                }
            };
        }
        else
        {
            Debug.LogWarning($"아이콘을 로드할 수 없습니다");
        }

        return ability;
    }

    public override void OnAcquire(PlayerClass player)
    {
        // 공격 능력 효과 적용
        ApplyEffect(player, effectValue);

        // 디버그 로그
        Debug.Log($"획득한 공격 능력: {name} (Lv.{level}) - {effectValue}");
    }

    // OnLevelUp 메서드 수정
    public override void OnLevelUp(PlayerClass player)
    {
        // 레벨업 플래그 설정
        isLevelingUp = true;

        // 기존 효과 제거
        OnReset(player);

        // 레벨업 및 효과 증가
        level++;
        effectValue = originalValue * (1 + (level - 1) * levelMultiplier);

        // 새 효과 적용
        OnAcquire(player);

        // 레벨업 플래그 해제
        isLevelingUp = false;

        // 디버그 로그
        Debug.Log($"레벨업한 공격 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // 공격 능력 효과 제거
        RemoveEffect(player, effectValue);
    }

    // 공격 능력 효과 적용
    protected override void ApplyEffect(PlayerClass player, float value)
    {
        switch (attackType)
        {
            case AttackAbilityType.ComboEnhancement:
                ApplyComboEnhancement(player, value);
                break;
            case AttackAbilityType.WeakPrey:
                ApplyWeakPrey(player, value);
                break;
            case AttackAbilityType.ChainStrike:
                ApplyChainStrike(player, value);
                break;
            case AttackAbilityType.GaugeBoost:
                ApplyGaugeBoost(player, value);
                break;
            case AttackAbilityType.ArmorCrush:
                ApplyArmorCrush(player, value);
                break;
        }
    }

    // 공격 능력 효과 제거
    protected override void RemoveEffect(PlayerClass player, float value)
    {
        switch (attackType)
        {
            case AttackAbilityType.ComboEnhancement:
                RemoveComboEnhancement(player, value);
                break;
            case AttackAbilityType.WeakPrey:
                RemoveWeakPrey(player, value);
                break;
            case AttackAbilityType.ChainStrike:
                RemoveChainStrike(player, value);
                break;
            case AttackAbilityType.GaugeBoost:
                RemoveGaugeBoost(player, value);
                break;
            case AttackAbilityType.ArmorCrush:
                RemoveArmorCrush(player, value);
                break;
        }
    }

    // 콤보 강화 효과 적용
    private void ApplyComboEnhancement(PlayerClass player, float enhancementPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        // 컴포넌트 추가 또는 참조
        ComboEnhancementComponent comboComp = playerObj.GetComponent<ComboEnhancementComponent>();
        if (comboComp == null)
        {
            comboComp = playerObj.AddComponent<ComboEnhancementComponent>();
        }

        comboComp.AddEnhancement(enhancementPercent / 100f);
        Debug.Log($"콤보 강화 효과 적용: {enhancementPercent}%, 현재 콤보 증가율: {comboComp.GetEnhancementAmount() * 100f}%");
    }

    // 콤보 강화 효과 제거
    private void RemoveComboEnhancement(PlayerClass player, float enhancementPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ComboEnhancementComponent comboComp = playerObj.GetComponent<ComboEnhancementComponent>();
        if (comboComp != null)
        {
            comboComp.RemoveEnhancement(enhancementPercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(comboComp.GetEnhancementAmount(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(comboComp);
            }
        }
    }

    //약자 공격 효과 적용
    private void ApplyWeakPrey(PlayerClass player, float damageBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        WeakPreyComponent weakPreyComp = playerObj.GetComponent<WeakPreyComponent>();
        if (weakPreyComp == null)
        {
            weakPreyComp = playerObj.AddComponent<WeakPreyComponent>();
        }

        weakPreyComp.AddDamageBonus(damageBonus / 100f);
        Debug.Log($"약자 공격 효과 적용: {damageBonus}%, 현재 추가 피해: {weakPreyComp.GetDamageBonus() * 100f}%");
    }

    // 약자 공격 효과 제거
    private void RemoveWeakPrey(PlayerClass player, float damageBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        WeakPreyComponent weakPreyComp = playerObj.GetComponent<WeakPreyComponent>();
        if (weakPreyComp != null)
        {
            weakPreyComp.RemoveDamageBonus(damageBonus / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(weakPreyComp.GetDamageBonus(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(weakPreyComp);
            }
        }
    }

    // 연쇄 타격 효과 적용
    private void ApplyChainStrike(PlayerClass player, float chancePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ChainStrikeComponent chainStrikeComp = playerObj.GetComponent<ChainStrikeComponent>();
        if (chainStrikeComp == null)
        {
            chainStrikeComp = playerObj.AddComponent<ChainStrikeComponent>();
        }

        chainStrikeComp.AddChainChance(chancePercent / 100f);
        Debug.Log($"연쇄 타격 효과 적용: {chancePercent}%, 현재 확률: {chainStrikeComp.GetChainChance() * 100f}%");
    }

    // 연쇄 타격 효과 제거
    private void RemoveChainStrike(PlayerClass player, float chancePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ChainStrikeComponent chainStrikeComp = playerObj.GetComponent<ChainStrikeComponent>();
        if (chainStrikeComp != null)
        {
            chainStrikeComp.RemoveChainChance(chancePercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(chainStrikeComp.GetChainChance(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(chainStrikeComp);
            }
        }
    }

    // 게이지 강화 효과 적용
    private void ApplyGaugeBoost(PlayerClass player, float boostPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        GaugeBoostComponent gaugeBoostComp = playerObj.GetComponent<GaugeBoostComponent>();
        if (gaugeBoostComp == null)
        {
            gaugeBoostComp = playerObj.AddComponent<GaugeBoostComponent>();
        }

        gaugeBoostComp.AddGaugeBoost(boostPercent / 100f);
        Debug.Log($"게이지 강화 효과 적용: {boostPercent}%, 현재 충전률: {gaugeBoostComp.GetGaugeBoost() * 100f}%");
    }

    // 게이지 강화 효과 제거
    private void RemoveGaugeBoost(PlayerClass player, float boostPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        GaugeBoostComponent gaugeBoostComp = playerObj.GetComponent<GaugeBoostComponent>();
        if (gaugeBoostComp != null)
        {
            gaugeBoostComp.RemoveGaugeBoost(boostPercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(gaugeBoostComp.GetGaugeBoost(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(gaugeBoostComp);
            }
        }
    }

    // 방어력 파괴 효과 적용
    private void ApplyArmorCrush(PlayerClass player, float crushPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ArmorCrushComponent armorCrushComp = playerObj.GetComponent<ArmorCrushComponent>();
        if (armorCrushComp == null)
        {
            armorCrushComp = playerObj.AddComponent<ArmorCrushComponent>();
        }

        armorCrushComp.AddCrushAmount(crushPercent / 100f);
        Debug.Log($"방어력 파괴 효과 적용: {crushPercent}%, 현재 감소율: {armorCrushComp.GetCrushAmount() * 100f}%");
    }

    // 방어력 파괴 효과 제거
    private void RemoveArmorCrush(PlayerClass player, float crushPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ArmorCrushComponent armorCrushComp = playerObj.GetComponent<ArmorCrushComponent>();
        if (armorCrushComp != null)
        {
            armorCrushComp.RemoveCrushAmount(crushPercent / 100f);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (Mathf.Approximately(armorCrushComp.GetCrushAmount(), 0f) && !isLevelingUp)
            {
                GameObject.Destroy(armorCrushComp);
            }
        }
    }
}