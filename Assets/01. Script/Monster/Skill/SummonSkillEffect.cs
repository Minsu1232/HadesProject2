using UnityEngine;

public class SummonSkillEffect : ISkillEffect
{
    private GameObject summonPrefab;
    private int count;
    private float summonRadius;  // ��ȯ ����
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
        // ��ȯ ���� ����
    }

    public void OnComplete()
    {
        // ��ȯ �Ϸ� ó��
    }
}