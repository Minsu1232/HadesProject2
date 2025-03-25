// LightningJudgmentComponent.cs
using UnityEngine;
using System.Collections.Generic;
public class LightningJudgmentComponent : MonoBehaviour
{
    private float damageMultiplier = 0f; // 플레이어 공격력 대비 번개 데미지 비율
    private PlayerClass playerClass;

    [SerializeField] private GameObject lightningEffectPrefab; // 번개 VFX 프리팹
    [SerializeField] private AudioClip thunderSound; // 번개 사운드
    [SerializeField] private float hitDelay = 0.1f; // 번개가 떨어지는 간격
    

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("LightningJudgmentComponent: PlayerClass를 찾을 수 없습니다.");
            Destroy(this);
            return;
        }

        // 기본 프리팹 로드 (없을 경우)
        if (lightningEffectPrefab == null)
        {
            // 어드레서블에서 로드하거나 Resources에서 로드
            lightningEffectPrefab = GameInitializer.Instance.lightningEffect;
        }
  
    }

    // 특수 스킬 사용 시 호출될 메서드
    public void ExecuteLightningJudgment()
    {
        if (damageMultiplier <= 0f)
            return;

        StartCoroutine(StrikeLightningOnAllMonsters());
    }

    // 모든 몬스터에게 번개 내리치기
    private System.Collections.IEnumerator StrikeLightningOnAllMonsters()
    {
        // 던전 매니저에서 활성화된 몬스터 목록 가져오기
        List<ICreatureStatus> activeMonsters = DungeonManager.Instance.GetActiveMonsters();

        // 각 몬스터에게 번개 내리치기
        foreach (var monster in activeMonsters)
        {
            // null 체크 및 생존 여부 확인
            if (monster == null || monster.GetMonsterClass() == null || !monster.GetMonsterClass().IsAlive)
                continue;

            // 몬스터 위치 가져오기
            Vector3 monsterPosition;
            try
            {
                monsterPosition = monster.GetMonsterTransform().position;
            }
            catch (System.Exception)
            {
                // Transform을 가져오는데 문제가 있으면 건너뛰기
                continue;
            }

            // 번개 이펙트 생성
            GameObject lightningEffect = Instantiate(
                lightningEffectPrefab,
                new Vector3(monsterPosition.x, monsterPosition.y + 2f, monsterPosition.z),
                Quaternion.identity
            );

            // 번개 사운드 재생
            if (thunderSound != null)
            {
                AudioSource.PlayClipAtPoint(thunderSound, monsterPosition);
            }

            // 데미지 계산 및 적용
            int damage = CalculateLightningDamage();
            monster.TakeDamage(damage);

            Debug.Log($"번개 심판 발동: {monster.GetType().Name}에게 {damage} 데미지!");

            // 번개 이펙트는 일정 시간 후 자동 제거
            Destroy(lightningEffect, 2f);

            // 다음 번개까지 약간의 딜레이
            yield return new WaitForSeconds(hitDelay);
        }
    }

    // 번개 데미지 계산
    private int CalculateLightningDamage()
    {
        int baseDamage = playerClass.PlayerStats.AttackPower;
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    // 데미지 배율 설정
    public void SetDamageMultiplier(float multiplier)
    {
        damageMultiplier = Mathf.Max(0f, multiplier);
        Debug.Log($"번개 심판 데미지 배율 설정: 공격력의 {damageMultiplier * 100}%");
    }

    // 현재 데미지 배율 반환
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }
}