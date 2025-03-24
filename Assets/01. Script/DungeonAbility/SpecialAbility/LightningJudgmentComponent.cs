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
    [SerializeField] private LayerMask monsterLayer; // 몬스터 레이어 마스크
    
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
            lightningEffectPrefab = Resources.Load<GameObject>("Effects/LightningStrike");
        }
        
        // 몬스터 레이어 설정
        if (monsterLayer == 0)
        {
            monsterLayer = LayerMask.GetMask("Monster");
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
        // 씬의 모든 몬스터 찾기
        Collider[] monsters = Physics.OverlapSphere(transform.position, 100f, monsterLayer);
        List<ICreatureStatus> uniqueMonsters = new List<ICreatureStatus>(); 
        
        // 중복 제거하여 유니크한 몬스터만 추출
        foreach (var collider in monsters)
        {
            ICreatureStatus monster = collider.GetComponentInParent<ICreatureStatus>();
            if (monster != null && !uniqueMonsters.Contains(monster))
            {
                uniqueMonsters.Add(monster);
            }
        }
        
        // 각 몬스터에게 번개 내리치기
        foreach (var monster in uniqueMonsters)
        {
            // 몬스터 위치 가져오기
            Vector3 monsterPosition = monster.GetMonsterTransform().position;
            
            // 번개 이펙트 생성
            GameObject lightningEffect = Instantiate(
                lightningEffectPrefab, 
                new Vector3(monsterPosition.x, monsterPosition.y + 10f, monsterPosition.z),
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