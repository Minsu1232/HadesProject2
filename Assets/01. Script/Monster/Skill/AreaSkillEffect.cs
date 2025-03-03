using UnityEngine;

public class AreaSkillEffect : ISkillEffect
{
    private GameObject areaEffectPrefab;
    private float radius;
    private ICreatureStatus monsterStatus;
    private Transform target;
    private float damage;

    public Transform transform { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public AreaSkillEffect(GameObject prefab, float radius, float damage)
    {
        this.areaEffectPrefab = prefab;
        this.radius = radius;
        this.damage = damage;
    }

    public void Initialize(ICreatureStatus status, Transform target)
    {
        this.monsterStatus = status;
        this.target = target;
    }

    public void Execute()
    {
        GameObject effect = GameObject.Instantiate(areaEffectPrefab,
            target.position,
            Quaternion.identity);
        // 범위 공격 로직 구현
    }

    public void OnComplete()
    {
        // 범위 공격 완료 처리
    }
}