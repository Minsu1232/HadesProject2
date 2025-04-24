using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;

[System.Serializable]
public abstract class DungeonAbility
{
    public string id;             // 고유 식별자
    public string name;           // 이름
    public string description;    // 설명
    public Sprite icon;           // 아이콘
    public string iconAddress;    // 아이콘 주소
    public Rarity rarity;         // 희귀도
    public float effectValue;     // 효과
    public int level = 1;         // 능력 레벨
    public float levelMultiplier = 0.3f; // 레벨업 증가 계수 (기본값 30%)
    protected bool isLevelingUp = false;
    protected float originalValue; // 원래 효과값 (레벨업 시 사용)

    // CSV 데이터에서 어빌리티 생성을 위한 제네릭 메서드
    // T: 생성할 어빌리티 타입, TEnum: 해당 어빌리티의 타입 열거형
    public static T FromCSVData<T, TEnum>(Dictionary<string, string> csvData, string typeFieldName) where T : DungeonAbility, new()
    {
        T ability = new T();

        // 필수 값 파싱
        string id = csvData["ID"];
        // 타입 파싱은 자식 클래스에서 처리해야 함
        string name = csvData["Name"];
        string description = csvData["Description"];
        Rarity rarity = (Rarity)int.Parse(csvData["Rarity"]);
        float baseValue = float.Parse(csvData["BaseValue"]);

        // 레벨업 배율 파싱 추가
        if (csvData.ContainsKey("LevelMultiplier") && !string.IsNullOrEmpty(csvData["LevelMultiplier"]))
        {
            ability.levelMultiplier = float.Parse(csvData["LevelMultiplier"]);
        }

        // 아이콘 주소 저장
        if (csvData.ContainsKey("IconPath") && !string.IsNullOrEmpty(csvData["IconPath"]))
        {
            ability.iconAddress = csvData["IconPath"];

            // 아이콘 경로가 있다면 어드레서블로 로드
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

        // 기본 속성 설정
        ability.id = id;
        ability.name = name;
        ability.description = description;
        ability.rarity = rarity;
        ability.effectValue = baseValue;
        ability.originalValue = baseValue;

        return ability;
    }

    // 아래는 기존의 메서드들
    public virtual void OnAcquire(PlayerClass player)
    {
        // 효과 적용 (자식 클래스에서 구현)
        ApplyEffect(player, effectValue);

        // 디버그 로그
        Debug.Log($"획득한 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public virtual void OnLevelUp(PlayerClass player)
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
        Debug.Log($"레벨업한 능력: {name} (Lv.{level}) - {effectValue}");
    }

    public virtual void OnReset(PlayerClass player)
    {
        // 효과 제거 (자식 클래스에서 구현)
        RemoveEffect(player, effectValue);
    }

    // 자식 클래스에서 구현해야 하는 메서드
    protected abstract void ApplyEffect(PlayerClass player, float value);
    protected abstract void RemoveEffect(PlayerClass player, float value);

    // 컴포넌트를 안전하게 제거하는 헬퍼 메서드
    protected void SafeDestroy<TComponent>(GameObject obj) where TComponent : Component
    {
        TComponent component = obj.GetComponent<TComponent>();
        if (component != null && !isLevelingUp)
        {
            GameObject.Destroy(component);
        }
    }
}

// 희귀도 enum
public enum Rarity
{
    Common,     // 일반
    Uncommon,   // 고급
    Rare,       // 희귀
    Epic,       // 에픽
    Legendary   // 전설
}