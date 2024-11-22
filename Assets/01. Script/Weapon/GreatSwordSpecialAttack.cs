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
            // 최초 실행 시 StatModifierData 로드
            Addressables.LoadAssetAsync<StatModifierData>("GreatSwordStatData").Completed += handle =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    cachedStatModifierData = handle.Result;
                    isDataLoaded = true;
                    Debug.Log("StatModifierData 로드 완료");

                    // 로드 완료 후 스킬 효과 적용
                    ApplySkillEffect();
                }
                else
                {
                    Debug.LogError("StatModifierData 로드 실패");
                }
            };
        }
        else
        {
            // 이미 로드된 데이터로 스킬 효과 적용
            ApplySkillEffect();
        }
    }

    private void ApplySkillEffect()
    {
        playerclass = GameInitializer.Instance.GetPlayerClass();
        if (playerclass == null)
        {
            Debug.LogError("PlayerClass가 초기화되지 않았습니다.");
            return;
        }

        // VFX 생성
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
                Debug.Log("VFX 생성 완료");
            }
            else
            {
                RestartVFX();
            }
        }
        else
        {
            Debug.LogWarning("Buff Particle Prefab이 설정되지 않았습니다.");
        }

        // 능력치 증가
        playerclass.ModifyPower(
            cachedStatModifierData.healthBoost,
            0,
            cachedStatModifierData.attackBoost,
            0,
            cachedStatModifierData.speedBoost,
            cachedStatModifierData.criticalChanceBoost
        );

        Debug.Log("스킬 효과 적용 완료");

        // 버프 지속 시간 후 효과 종료
        DOVirtual.DelayedCall(cachedStatModifierData.buffDuration, () =>
        {
            if (activeVFX != null)
            {
                activeVFX.SetActive(false); // VFX 비활성화
                Debug.Log("VFX 비활성화");
            }

            playerclass.ResetPower();
            Debug.Log("스킬 효과 종료, 능력치 복구 완료");
        });
    }

    private void RestartVFX()
    {
        if (activeVFX != null)
        {
            activeVFX.transform.position = playerclass.playerTransform.position;
            activeVFX.transform.rotation = Quaternion.identity;
            activeVFX.SetActive(true);

            // ParticleSystem 재시작
            ParticleSystem particle = activeVFX.GetComponent<ParticleSystem>();
            if (particle != null)
            {
                particle.Clear();
                particle.Play();
            }

            Debug.Log("VFX 재시작");
        }
    }
    public void ReleaseResources()
    {
        // VFX 릴리즈
        if (activeVFX != null)
        {
            GameObject.Destroy(activeVFX); // VFX 제거
            activeVFX = null;
            Debug.Log("VFX 제거 완료");
        }

        // StatModifierData 릴리즈
        if (isDataLoaded && cachedStatModifierData != null)
        {
            Addressables.Release(cachedStatModifierData);
            cachedStatModifierData = null;
            isDataLoaded = false;
            Debug.Log("StatModifierData 릴리즈 완료");
        }
    }
}
