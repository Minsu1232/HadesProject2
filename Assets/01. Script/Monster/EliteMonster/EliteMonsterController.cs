using static AttackData;
using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class EliteMonsterController : MonoBehaviour
{
    private MonsterStatus monsterStatus;
    private EliteMonster eliteMonster;
    private const float SIZE_INCREASE = 1.3f;

    private List<Material> outlineMaterials = new List<Material>();
    private Color[] targetColors = new Color[]
    {
        Color.yellow,
        Color.red,
        Color.green
    };
    private float colorChangeDuration = 0.2f;
    private DG.Tweening.Sequence colorSequence;

    private void Awake()
    {
        monsterStatus = GetComponent<MonsterStatus>();
    }

    private void Start()
    {
        if (monsterStatus != null && monsterStatus.GetMonsterClass() is EliteMonster elite)
        {
            eliteMonster = elite;
            foreach (var ability in eliteMonster.GetEliteAbilities())
            {
                ability.ApplyAbility(monsterStatus);
                gameObject.transform.localScale *= SIZE_INCREASE;
            }
            ApplyEliteOutline();
        }
    }

    private void ApplyEliteOutline()
    {
        var skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        MonsterData data = monsterStatus.GetMonsterClass().GetMonsterData();

        foreach (var skinnedRenderer in skinnedMeshRenderers)
        {
            GameObject outlineObject = new GameObject("EliteOutline");
            outlineObject.transform.parent = skinnedRenderer.transform;
            outlineObject.transform.localPosition = Vector3.zero;
            outlineObject.transform.localRotation = Quaternion.identity;
            outlineObject.transform.localScale = Vector3.one * 1.02f;

            SkinnedMeshRenderer outlineRenderer = outlineObject.AddComponent<SkinnedMeshRenderer>();
            outlineRenderer.sharedMesh = skinnedRenderer.sharedMesh;

            Material instanceMaterial = Instantiate(data.eliteOutlineMaterial);
            outlineRenderer.material = instanceMaterial;
            outlineMaterials.Add(instanceMaterial);

            if (instanceMaterial.HasProperty("_OutlineScale"))
            {
                instanceMaterial.SetFloat("_OutlineScale", 1.1f);
            }

            if (instanceMaterial.HasProperty("_OutlineColor"))
            {
                instanceMaterial.SetColor("_OutlineColor", Color.yellow);
            }

            outlineRenderer.bones = skinnedRenderer.bones;
            outlineRenderer.rootBone = skinnedRenderer.rootBone;
        }

        SetupColorSequence();
    }

    private void SetupColorSequence()
    {
        if (outlineMaterials.Count == 0)
        {
            Debug.LogWarning("No outline materials found to animate!");
            return;
        }

        colorSequence?.Kill();
        colorSequence = DOTween.Sequence();

        for (int i = 0; i < targetColors.Length; i++)
        {
            int currentIndex = i;
            int nextIndex = (i + 1) % targetColors.Length;

            colorSequence.Append(DOVirtual.Color(
                targetColors[currentIndex],
                targetColors[nextIndex],
                colorChangeDuration,
                (Color newColor) =>
                {
                    foreach (var material in outlineMaterials)
                    {
                        if (material != null && material.HasProperty("_OutlineColor"))
                        {
                            material.SetColor("_OutlineColor", newColor);
                        }
                    }
                }
            ).SetEase(Ease.InOutSine));
        }

        colorSequence.SetLoops(-1);
    }

    private void Update()
    {
        if (eliteMonster != null)
        {
            foreach (var ability in eliteMonster.GetEliteAbilities())
            {
                ability.OnUpdate(monsterStatus);
            }
        }
    }

    public void OnAttackEffect()
    {
        foreach (var ability in eliteMonster.GetEliteAbilities())
        {
            ability.OnAttack(monsterStatus);
        }
    }

    public void OnHitEffect(int damage, AttackType attackType)
    {
        foreach (var ability in eliteMonster.GetEliteAbilities())
        {
            ability.OnHit(monsterStatus, damage, attackType);
        }
    }

    private void OnDestroy()
    {
        colorSequence?.Kill();

        // 积己等 赣萍府倔 沥府
        foreach (var material in outlineMaterials)
        {
            if (material != null)
            {
                Destroy(material);
            }
        }
        outlineMaterials.Clear();
    }
}