// FlameAreaEffect.cs - 불꽃 데미지 영역 효과
using System.Collections.Generic;
using UnityEngine;

public class FlameAreaEffect : BaseAreaEffect
{
    [SerializeField] private LayerMask monsterLayer;

    private void Awake()
    {
        // 몬스터 레이어 설정
        if (monsterLayer == 0)
        {
            monsterLayer = LayerMask.GetMask("Monster");
        }
    }

    protected override void ApplyAreaDamage()
    {
       
        // 범위 내 몬스터 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, monsterLayer);

        // 이미 처리한 몬스터 객체 추적
        HashSet<ICreatureStatus> processedCreatures = new HashSet<ICreatureStatus>();

        foreach (var hitCollider in hitColliders)
        {
            Debug.Log(hitCollider.gameObject.name); 
            // 몬스터의 ICreatureStatus 컴포넌트 찾기
            ICreatureStatus creature = hitCollider.GetComponentInParent<ICreatureStatus>();

            // 유효한 몬스터이고 아직 처리하지 않은 경우에만 데미지 적용
            if (creature != null && !processedCreatures.Contains(creature))
            {
                // 처리한 몬스터로 기록
                processedCreatures.Add(creature);

                // 데미지 적용 (tick 당 데미지)
                int tickDamage = Mathf.RoundToInt(damage * tickRate);
                creature.TakeDamage(tickDamage);

                Debug.Log($"불꽃 데미지 적용: {tickDamage} 데미지");
            }
        }
    }

    // 디버그용 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}