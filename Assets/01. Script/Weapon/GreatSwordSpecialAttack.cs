using DG.Tweening;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GreatSwordSpecialAttack : SpecialAttackBase
{
    private PlayerClass playerclass;
    private GameObject activeVFX;

    private StatModifierData cachedStatModifierData;
    private bool isDataLoaded = false;

    public GreatSwordSpecialAttack(WeaponManager weapon) : base(weapon) { }

    protected override void PerformSkillEffect()
    {
        if (!isDataLoaded)
        {
            // ���� ���� �� StatModifierData �ε�
            Addressables.LoadAssetAsync<StatModifierData>("GreatSwordStatData").Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    cachedStatModifierData = handle.Result;
                    isDataLoaded = true;
                    Debug.Log("StatModifierData �ε� �Ϸ�");

                    // �ε� �Ϸ� �� ��ų ȿ�� ����
                    ApplySkillEffect();
                }
                else
                {
                    Debug.LogError("StatModifierData �ε� ����");
                }
            };
        }
        else
        {
            // �̹� �ε�� �����ͷ� ��ų ȿ�� ����
            ApplySkillEffect();
        }
    }

    private void ApplySkillEffect()
    {
        playerclass = GameInitializer.Instance.GetPlayerClass();
        if (playerclass == null)
        {
            Debug.LogError("PlayerClass�� �ʱ�ȭ���� �ʾҽ��ϴ�.");
            return;
        }

        // VFX ����
        if (cachedStatModifierData.buffParticle != null)
        {
            if (activeVFX == null)
            {
                activeVFX = GameObject.Instantiate(
                    cachedStatModifierData.buffParticle,
                    playerclass.playerTransform.position,
                    Quaternion.identity
                );
                activeVFX.transform.SetParent(playerclass.playerTransform);
                Debug.Log("VFX ���� �Ϸ�");
            }
            else
            {
                RestartVFX();
            }
        }
        else
        {
            Debug.LogWarning("Buff Particle Prefab�� �������� �ʾҽ��ϴ�.");
        }

        // �ɷ�ġ ����
        playerclass.ModifyPower(
            cachedStatModifierData.healthBoost,
            0,
            cachedStatModifierData.attackBoost,
            0,
            cachedStatModifierData.speedBoost,
            cachedStatModifierData.criticalChanceBoost
        );

        Debug.Log("��ų ȿ�� ���� �Ϸ�");

        // ���� ���� �ð� �� ȿ�� ����
        DOVirtual.DelayedCall(cachedStatModifierData.buffDuration, () =>
        {
            if (activeVFX != null)
            {
                activeVFX.SetActive(false); // VFX ��Ȱ��ȭ
                Debug.Log("VFX ��Ȱ��ȭ");
            }

            playerclass.ResetPower();
            Debug.Log("��ų ȿ�� ����, �ɷ�ġ ���� �Ϸ�");
        });
    }

    private void RestartVFX()
    {
        if (activeVFX != null)
        {
            activeVFX.transform.position = playerclass.playerTransform.position;
            activeVFX.transform.rotation = Quaternion.identity;
            activeVFX.SetActive(true);

            // ParticleSystem �����
            ParticleSystem particle = activeVFX.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Clear();
                particle.Play();
            }

            Debug.Log("VFX �����");
        }
    }
    public void ReleaseResources()
    {
        // VFX ������
        if (activeVFX != null)
        {
            GameObject.Destroy(activeVFX); // VFX ����
            activeVFX = null;
            Debug.Log("VFX ���� �Ϸ�");
        }

        // StatModifierData ������
        if (isDataLoaded && cachedStatModifierData != null)
        {
            Addressables.Release(cachedStatModifierData);
            cachedStatModifierData = null;
            isDataLoaded = false;
            Debug.Log("StatModifierData ������ �Ϸ�");
        }
    }
}
