using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronofractureSpecialAttack : SpecialAttackBase
{
    private PlayerClass playerClass;
    private Chronofracture chronofractureWeapon; // 부모 무기 참조

    // 공격 범위와 데미지 관련 변수
    private float attackRadius = 5f; // 공격 범위 (반경)
    private float attackAngle = 120f; // 공격 각도 (전방 콘 형태)
    private float baseDamageMultiplier = 2.5f; // 기본 데미지 배율
    private float maxDamageMultiplier = 4f; // 최대 데미지 배율 (적이 많을 때)
    private int maxTargetsForScaling = 5; // 데미지 스케일링을 위한 최대 타겟 수
    private GameObject fusionExplosionEffectPrefab;
    public ChronofractureSpecialAttack(WeaponManager weapon) : base(weapon)
    {
        chronofractureWeapon = weapon as Chronofracture;
        WeaponresetGage = 0; // 게이지 사용 후 초기화
        fusionExplosionEffectPrefab = chronofractureWeapon.specialBladeWavePrefab;
    }

    protected override void PerformSkillEffect()
    {
        if (isSpecialAttack) return;

        // 특수 공격 활성화
        isSpecialAttack = true;

        // 플레이어 클래스 참조 가져오기
        playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass == null)
        {
            Debug.LogError("플레이어 클래스를 찾을 수 없습니다.");
            isSpecialAttack = false;
            return;
        }

        // 시간 융합 타격 효과 실행
        Debug.Log("시간 융합 타격 특수 스킬 발동!");

        // 전방 콘 형태의 공격 실행
        PerformTemporalFusionStrike();

        // 약간의 시간 후 특수 공격 상태 해제 (애니메이션 길이에 맞춰 조정)
        MonoBehaviour mono = weaponManager as MonoBehaviour;
        if (mono != null)
        {
            mono.StartCoroutine(ResetSpecialAttackState(1.0f));
        }
        else
        {
            // MonoBehaviour가 없는 경우 바로 초기화
            isSpecialAttack = false;
        }
    }

    private void PerformTemporalFusionStrike()
    {
        Transform playerTransform = playerClass.playerTransform;
        if (playerTransform == null) return;

        Vector3 playerPosition = playerTransform.position;
        Vector3 playerForward = playerTransform.forward;

        // 공격 범위 내의 적 탐색
        Collider[] hitColliders = Physics.OverlapSphere(playerPosition, attackRadius);

        // 이미 처리한 몬스터를 추적하기 위한 HashSet
        HashSet<ICreatureStatus> processedMonsters = new HashSet<ICreatureStatus>();

        // 공격 범위 내 유효한 적 목록
        List<IDamageable> validTargets = new List<IDamageable>();

        foreach (Collider collider in hitColliders)
        {
            // 몬스터 히트박스 확인
            MonsterHitBox hitBox = collider.GetComponent<MonsterHitBox>();
            if (hitBox == null) continue;

            ICreatureStatus monster = hitBox.GetMonsterStatus();
            if (monster == null || !(monster is IDamageable)) continue;

            // 이미 처리한 몬스터인지 확인
            if (processedMonsters.Contains(monster)) continue;

            // 적이 전방 콘 범위 안에 있는지 확인
            Vector3 directionToTarget = (collider.transform.position - playerPosition).normalized;
            float angleToTarget = Vector3.Angle(playerForward, directionToTarget);

            if (angleToTarget <= attackAngle / 2) // 콘 형태 공격 범위 체크
            {
                validTargets.Add(monster as IDamageable);
                processedMonsters.Add(monster); // 처리된 몬스터로 표시
            }
        }

        // 데미지 계산: 적의 수에 따라 데미지 증가
        int targetCount = validTargets.Count;
        float damageMultiplier = baseDamageMultiplier;

        if (targetCount > 0)
        {
            // 적의 수에 비례해 데미지 배율 증가 (최대값 제한)
            float scalingFactor = Mathf.Min(targetCount, maxTargetsForScaling) / (float)maxTargetsForScaling;
            damageMultiplier = Mathf.Lerp(baseDamageMultiplier, maxDamageMultiplier, scalingFactor);
        }

        int baseDamage = weaponManager.BaseDamage;
        int finalDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
        // 이펙트 생성 - 플레이어 위치에 생성하되 전방을 향하도록
        if (fusionExplosionEffectPrefab != null)
        {
            Vector3 effectPosition = playerPosition + playerForward * 1f; // 약간 앞에 생성
            GameObject effectObj = GameObject.Instantiate(
                fusionExplosionEffectPrefab,
                effectPosition,
                Quaternion.LookRotation(playerForward)
            );

            // 몇 초 후 자동 파괴
            GameObject.Destroy(effectObj, 2f);
        }
        // 디버그 메시지 (적의 수에 따른 데미지 스케일링)
        Debug.Log($"시간 융합 타격: 대상 {targetCount}개, 데미지 배율 {damageMultiplier:F2}x, 최종 데미지 {finalDamage}");

        // 모든 유효 타겟에 데미지와 메아리 3스택 적용
        foreach (IDamageable target in validTargets)
        {
            // 데미지 적용
            target.TakeDamage(finalDamage);

            // 메아리 3스택 적용 (한번에 3번 호출)
            if (chronofractureWeapon != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    chronofractureWeapon.AddEchoStackToTarget(target);
                }
            }

            Debug.Log($"시간 융합 타격이 {target}에게 {finalDamage} 데미지와 메아리 3스택을 적용했습니다.");
        }
    }

    private IEnumerator ResetSpecialAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isSpecialAttack = false;
        Debug.Log("시간 융합 타격 효과 종료");
    }

    public override void PlayVFX()
    {
        // VFX 재생 (실제 구현 시 추가)
        Debug.Log("시간 융합 타격 VFX 재생");
    }

    public override void PlaySound()
    {
        // 사운드 재생 (실제 구현 시 추가)
        Debug.Log("시간 융합 타격 사운드 재생");
    }
}