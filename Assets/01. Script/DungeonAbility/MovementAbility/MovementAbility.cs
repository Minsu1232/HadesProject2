// MovementAbility.cs - 이동 관련 능력 클래스 (DungeonAbility 확장)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class MovementAbility : DungeonAbility
{
    public enum MovementAbilityType
    {
        DashForce,       // 대시 힘 증가
        DashInvincible,  // 대시 중 짧은 무적 시간
        DashImpact,      // 대시 충돌 시 적에게 데미지
        DashFlame        // 대시 시 불꽃 생성하여 데미지
    }

    public MovementAbilityType movementType;


    // 생성자로 초기화
    public void Initialize(MovementAbilityType type, float value, string abilityName, string abilityDescription, Rarity abilityRarity)
    {
        movementType = type;
        effectValue = value;
        originalValue = value;

        id = $"movement_{type.ToString().ToLower()}";
        name = abilityName;
        description = abilityDescription;
        rarity = abilityRarity;
    }

    // CSV 데이터로 초기화하는 메서드 추가
    public static MovementAbility FromCSVData(Dictionary<string, string> csvData)
    {
        MovementAbility ability = new MovementAbility();

        // 필수 값 파싱
        string id = csvData["ID"];
        MovementAbilityType type = (MovementAbilityType)System.Enum.Parse(typeof(MovementAbilityType), csvData["Type"]);
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

        // 아이콘 경로가 있다면 어드레서블로 로드
        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            string iconAddress = csvData["IconPath"];
            Debug.Log(iconAddress);
            // 어드레서블 비동기 로드
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
        // 이동 능력 효과 적용
        ApplyEffect(player, effectValue);

        // 디버그 로그
        Debug.Log($"획득한 이동 능력: {name} (Lv.{level}) - {effectValue}");
    }

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
        Debug.Log($"레벨업한 이동 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public override void OnReset(PlayerClass player)
    {
        // 이동 능력 효과 제거
        RemoveEffect(player, effectValue);
    }

    // 이동 능력 효과 적용
    protected override void ApplyEffect(PlayerClass player, float value)
    {
        switch (movementType)
        {
            case MovementAbilityType.DashForce:
                ApplyDashForce(player, value);
                break;
            case MovementAbilityType.DashInvincible:
                // 주석 처리: 아직 구현되지 않음
                ApplyDashInvincible(player, value);
                Debug.Log($"대시 무적 효과는 아직 구현되지 않았습니다: {value}");
                break;
            case MovementAbilityType.DashImpact:
                // 주석 처리: 아직 구현되지 않음
                ApplyDashImpact(player, value);
                Debug.Log($"대시 충격 효과는 아직 구현되지 않았습니다: {value}");
                break;
            case MovementAbilityType.DashFlame:
                // 주석 처리: 아직 구현되지 않음
                ApplyDashFlame(player, value);
                Debug.Log($"대시 불꽃 효과는 아직 구현되지 않았습니다: {value}");
                break;
        }
    }

    // 이동 능력 효과 제거
    protected override void RemoveEffect(PlayerClass player, float value)
    {
        switch (movementType)
        {
            case MovementAbilityType.DashForce:
                RemoveDashForce(player, value);
                break;
            case MovementAbilityType.DashInvincible:

                RemoveDashInvincible(player, value);
                break;
            case MovementAbilityType.DashImpact:

                RemoveDashImpact(player, value);
                break;
            case MovementAbilityType.DashFlame:

                RemoveDashFlame(player, value);
                break;
        }
    }

    // 대시 힘 효과 적용
    private void ApplyDashForce(PlayerClass player, float forceAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        PlayerDashComponent dashComp = playerObj.GetComponent<PlayerDashComponent>();
        if (dashComp == null)
        {
            dashComp = playerObj.AddComponent<PlayerDashComponent>();
        }

        dashComp.IncreaseDashForce(forceAmount);
        Debug.Log($"대시 힘 효과 적용: +{forceAmount}, 현재: {dashComp.GetDashForce()}");
    }

    // 대시 힘 효과 제거
    private void RemoveDashForce(PlayerClass player, float forceAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        PlayerDashComponent dashComp = playerObj.GetComponent<PlayerDashComponent>();
        if (dashComp != null)
        {
            dashComp.IncreaseDashForce(-forceAmount); // 음수값으로 감소
        }
    }

    // 대시 무적 효과 적용
    private void ApplyDashInvincible(PlayerClass player, float duration)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        PlayerDashComponent dashComp = playerObj.GetComponent<PlayerDashComponent>();
        if (dashComp == null)
        {
            dashComp = playerObj.AddComponent<PlayerDashComponent>();
        }

        // 대시 무적 컴포넌트 적용
        DashInvincibleComponent invincibleComp = playerObj.GetComponent<DashInvincibleComponent>();
        if (invincibleComp == null)
        {
            invincibleComp = playerObj.AddComponent<DashInvincibleComponent>();
        }

        invincibleComp.SetInvincibleDuration(duration);
        Debug.Log($"대시 무적 효과 적용: {duration}초");
    }

    // 대시 무적 효과 제거
    private void RemoveDashInvincible(PlayerClass player, float duration)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        DashInvincibleComponent invincibleComp = playerObj.GetComponent<DashInvincibleComponent>();
        if (invincibleComp != null)
        {
            invincibleComp.SetInvincibleDuration(invincibleComp.GetInvincibleDuration() - duration);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (invincibleComp.GetInvincibleDuration() <= 0f && !isLevelingUp)
            {
                GameObject.Destroy(invincibleComp);
            }
        }
    }

    // 대시 충격 데미지 효과 적용
    private void ApplyDashImpact(PlayerClass player, float multiplierAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        PlayerDashComponent dashComp = playerObj.GetComponent<PlayerDashComponent>();
        if (dashComp == null)
        {
            dashComp = playerObj.AddComponent<PlayerDashComponent>();
        }

        // 대시 충돌 데미지 컴포넌트 적용  
        DashImpactComponent impactComp = playerObj.GetComponent<DashImpactComponent>();
        if (impactComp == null)
        {
            impactComp = playerObj.AddComponent<DashImpactComponent>();
        }

        // 증가 비율을 백분율로 변환 (예: 20% -> 0.2)
        float multiplier = multiplierAmount / 100f;
        impactComp.SetDamageMultiplier(impactComp.GetDamageMultiplier() + multiplier);
        Debug.Log($"대시 충격 데미지 효과 적용: 공격력의 {multiplier * 100}%");
    }

    // 대시 충격 데미지 효과 제거
    private void RemoveDashImpact(PlayerClass player, float multiplierAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        DashImpactComponent impactComp = playerObj.GetComponent<DashImpactComponent>();
        if (impactComp != null)
        {
            // 증가 비율을 백분율로 변환 (예: 20% -> 0.2)
            float multiplier = multiplierAmount / 100f;
            impactComp.SetDamageMultiplier(impactComp.GetDamageMultiplier() - multiplier);

            // 효과가 0이면 컴포넌트 제거 (레벨업 중이 아닐 때만)
            if (impactComp.GetDamageMultiplier() <= 0f && !isLevelingUp)
            {
                GameObject.Destroy(impactComp);
            }
        }
    }

    private void ApplyDashFlame(PlayerClass player, float damageAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        DashFlameComponent flameComp = playerObj.GetComponent<DashFlameComponent>();

        if (flameComp == null)
        {
            flameComp = playerObj.AddComponent<DashFlameComponent>();
        }

        // 컴포넌트 활성화
        flameComp.enabled = true;

        // 증가 비율 설정
        float multiplier = damageAmount / 100f;
        flameComp.SetDamageMultiplier(flameComp.GetDamageMultiplier() + multiplier);

        // 희귀도 설정
        flameComp.SetFlameRarity(this.rarity);

        Debug.Log($"대시 불꽃 효과 적용: 공격력의 {multiplier * 100}%, 희귀도: {this.rarity}");
    }

    // 대시 불꽃 효과 제거
    private void RemoveDashFlame(PlayerClass player, float damageAmount)
    {
        GameObject playerObj = GameInitializer.Instance.gameObject;
        DashFlameComponent flameComp = playerObj.GetComponent<DashFlameComponent>();
        if (flameComp != null)
        {
            // 증가 비율 감소
            float multiplier = damageAmount / 100f;
            flameComp.SetDamageMultiplier(flameComp.GetDamageMultiplier() - multiplier);

            // 효과가 0이면 컴포넌트 비활성화 (레벨업 중이 아닐 때만)
            if (flameComp.GetDamageMultiplier() <= 0f && !isLevelingUp)
            {
                flameComp.enabled = false;
            }
        }
    }
}