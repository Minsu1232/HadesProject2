// 실제 스킬 프로젝타일 구현
using UnityEngine;

public class SkillProjectile : BaseProjectile
{
    private bool hasDamageApplied = false;
    [SerializeField] GameObject particle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster")) return;  // 몬스터는 무시

        // 충돌 지점에 효과 생성
        impactEffect?.OnImpact(transform.position, damage);
        OnImpact(other);
        Debug.Log("독구름펑");
        // 발사체 제거
        Destroy(gameObject);
    }

    protected override void OnImpact(Collider other)
    {
        GameObject effect = Instantiate(hitEffect, transform.position, transform.rotation);
    }

}