using UnityEngine;

public class SummonSkillEffect : ISkillEffect
{
    private GameObject summonPrefab;
    private int count;
    private float summonRadius;  // 소환 범위
    private ICreatureStatus monsterStatus;
    private Transform target;

    public Transform transform { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public SummonSkillEffect(GameObject prefab, int count, float summonRadius)
    {
        this.summonPrefab = prefab;
        this.count = count;
        this.summonRadius = summonRadius;
    }

    public void Initialize(ICreatureStatus status, Transform target)
    {
        this.monsterStatus = status;
        this.target = target;
    }

    public void Execute()
    {
        // 소환 로직 구현
    }

    public void OnComplete()
    {
        // 소환 완료 처리
    }
}