// PassiveAbility 완성본
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class PassiveAbility : DungeonAbility
{
    public enum PassiveType
    {
        DamageReduction,  // 피해 감소
        LifeSteal,        // 흡혈
        Counterattack,    // 피격 시 반격
        ItemFind          // 아이템 찾기 확률 증가
    }

    public PassiveType passiveType;       

    private float originalValue;         // 원래 효과값 (레벨업 시 사용)

    // 생성자로 초기화
    public void Initialize(PassiveType type, float value, string abilityName, string abilityDescription, Rarity abilityRarity)
    {
        passiveType = type;
        effectValue = value;
        originalValue = value;

        id = $"passive_{type.ToString().ToLower()}";
        name = abilityName;
        description = abilityDescription;
        rarity = abilityRarity;
    }

    // CSV 데이터로 초기화하는 메서드 추가
    public static PassiveAbility FromCSVData(Dictionary<string, string> csvData)
    {
        PassiveAbility ability = new PassiveAbility();

        // 필수 값 파싱
        string id = csvData["ID"];
        PassiveType type = (PassiveType)System.Enum.Parse(typeof(PassiveType), csvData["Type"]);
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
                         // 아이콘 경로가 있다면 애드레서블로 로드
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
        // 패시브 효과 적용
        ApplyPassiveEffect(player, effectValue);

        // 디버그 로그
        Debug.Log($"획득한 패시브 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnLevelUp(PlayerClass player)
    {
        // 기존 효과 제거
        OnReset(player);

        // 레벨업 및 효과 증가
        level++;
        effectValue = originalValue * (1 + (level - 1) * levelMultiplier);

        // 새 효과 적용
        OnAcquire(player);

        // 디버그 로그
        Debug.Log($"레벨업한 패시브 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // 패시브 효과 제거
        RemovePassiveEffect(player, effectValue);
    }

    // 패시브 효과 적용
    private void ApplyPassiveEffect(PlayerClass player, float value)
    {
        switch (passiveType)
        {
            case PassiveType.DamageReduction:
                ApplyDamageReduction(player, value);
                break;
            case PassiveType.LifeSteal:
                ApplyLifeSteal(player, value);
                break;
            case PassiveType.Counterattack:
                ApplyCounterattack(player, value);
                break;
            case PassiveType.ItemFind:
                ApplyItemFind(player, value);
                break;
        }
    }

    // 패시브 효과 제거
    private void RemovePassiveEffect(PlayerClass player, float value)
    {
        switch (passiveType)
        {
            case PassiveType.DamageReduction:
                RemoveDamageReduction(player, value);
                break;
            case PassiveType.LifeSteal:
                RemoveLifeSteal(player, value);
                break;
            case PassiveType.Counterattack:
                RemoveCounterattack(player, value);
                break;
            case PassiveType.ItemFind:
                RemoveItemFind(player, value);
                break;
        }
    }

    // 피해 감소 효과 적용
    private void ApplyDamageReduction(PlayerClass player, float reductionPercent)
    {
        // damageReceiveRate 감소 (값이 작을수록 피해가 적음)
        float reductionFactor = reductionPercent / 100f;
        player.ModifyPower(0, 0, 0, 0, 0, 0, 0, -reductionFactor);

        Debug.Log($"피해 감소 효과 적용: {reductionPercent}%, 현재 피해 계수: {player.PlayerStats.DamageReceiveRate}");
    }

    // 피해 감소 효과 제거
    private void RemoveDamageReduction(PlayerClass player, float reductionPercent)
    {       
        player.ResetPower(false, false, false, false, false, false, false, true); // 리셋

        Debug.Log($"피해 감소 효과 제거: {reductionPercent}%, 현재 피해 계수: {player.PlayerStats.DamageReceiveRate}");
    }

    // 흡혈 효과 적용
    private void ApplyLifeSteal(PlayerClass player, float lifeStealPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LifeStealComponent lifeStealComp = playerObj.GetComponent<LifeStealComponent>();
        if (lifeStealComp == null)
        {
            lifeStealComp = playerObj.AddComponent<LifeStealComponent>();
        }

        lifeStealComp.AddLifeSteal(lifeStealPercent / 100f);
        Debug.Log($"흡혈 효과 적용: {lifeStealPercent}%, 현재 흡혈율: {lifeStealComp.GetLifeStealAmount() * 100f}%");
    }

    // 흡혈 효과 제거
    private void RemoveLifeSteal(PlayerClass player, float lifeStealPercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        LifeStealComponent lifeStealComp = playerObj.GetComponent<LifeStealComponent>();
        if (lifeStealComp != null)
        {
            lifeStealComp.RemoveLifeSteal(lifeStealPercent / 100f);

            // 흡혈 효과가 0이면 컴포넌트 제거
            if (Mathf.Approximately(lifeStealComp.GetLifeStealAmount(), 0f))
            {
                GameObject.Destroy(lifeStealComp);
            }
        }
    }

    // 피격 시 반격 효과 적용
    private void ApplyCounterattack(PlayerClass player, float counterDamagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        CounterattackComponent counterComp = playerObj.GetComponent<CounterattackComponent>();
        if (counterComp == null)
        {
            counterComp = playerObj.AddComponent<CounterattackComponent>();
        }

        counterComp.AddCounterDamage(counterDamagePercent / 100f);
        Debug.Log($"반격 효과 적용: {counterDamagePercent}%, 현재 반격 데미지: {counterComp.GetCounterDamageAmount() * 100f}%");
    }

    // 반격 효과 제거
    private void RemoveCounterattack(PlayerClass player, float counterDamagePercent)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        CounterattackComponent counterComp = playerObj.GetComponent<CounterattackComponent>();
        if (counterComp != null)
        {
            counterComp.RemoveCounterDamage(counterDamagePercent / 100f);

            // 반격 효과가 0이면 컴포넌트 제거
            if (Mathf.Approximately(counterComp.GetCounterDamageAmount(), 0f))
            {
                GameObject.Destroy(counterComp);
            }
        }
    }

    // 아이템 찾기 확률 증가 효과 적용
    private void ApplyItemFind(PlayerClass player, float findChanceBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ItemFindComponent itemFindComp = playerObj.GetComponent<ItemFindComponent>();
        if (itemFindComp == null)
        {
            itemFindComp = playerObj.AddComponent<ItemFindComponent>();
        }

        itemFindComp.AddItemFindBonus(findChanceBonus / 100f);
        Debug.Log($"아이템 찾기 효과 적용: +{findChanceBonus}%, 현재 보너스: +{itemFindComp.GetItemFindBonus() * 100f}%");
    }

    // 아이템 찾기 확률 증가 효과 제거
    private void RemoveItemFind(PlayerClass player, float findChanceBonus)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        ItemFindComponent itemFindComp = playerObj.GetComponent<ItemFindComponent>();
        if (itemFindComp != null)
        {
            itemFindComp.RemoveItemFindBonus(findChanceBonus / 100f);

            // 아이템 찾기 효과가 0이면 컴포넌트 제거
            if (Mathf.Approximately(itemFindComp.GetItemFindBonus(), 0f))
            {
                GameObject.Destroy(itemFindComp);
            }
        }
    }
}