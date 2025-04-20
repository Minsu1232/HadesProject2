using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChronofractureCharge : WeaponChargeBase
{
    private GameObject energyRing; // 차지 중 생성되는 시계 톱니바퀴 에너지 링
    private Transform playerTransform; // 플레이어 위치 참조
    private Vector3 dashDirection; // 대시 방향
    private GameObject bladeWavePrefab; // 검기 프리팹
    private Chronofracture parentWeapon; // 부모 무기 참조 (시간 메아리 추가용)
    private float minChargeRatio = 0.5f; // 최소 차지 비율 (50%)

    public ChronofractureCharge(WeaponManager manager, GameObject bladeWavePrefab) : base(manager)
    {
        this.bladeWavePrefab = bladeWavePrefab;
        this.parentWeapon = manager as Chronofracture;

        // 플레이어 Transform 참조 가져오기
        playerTransform = GameInitializer.Instance.GetPlayerClass().playerTransform;
    }

    public override void StartCharge()
    {
        base.StartCharge();

        // 차지 시작 시 시계 톱니바퀴 에너지 링 생성
        //CreateEnergyRing();
    }

    public override void UpdateCharge(float deltaTime)
    {
        base.UpdateCharge(deltaTime);

        // 에너지 링 크기와 효과 업데이트
        if (energyRing != null && isCharging)
        {
            // 차지 시간에 따라 링 효과 강화
            float chargeRatio = currentChargeTime / weaponManager.weaponData.maxChargeTime;
            UpdateEnergyRingEffect(chargeRatio);
        }
    }

    private void CreateEnergyRing()
    {
        // 에너지 링 게임 오브젝트 생성
        energyRing = new GameObject("TimeEnergyRing");
        energyRing.transform.position = playerTransform.position;

        // 파티클 시스템 추가
        ParticleSystem ringParticles = energyRing.AddComponent<ParticleSystem>();

        // 파티클 시스템 설정 전에 정지
        ringParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // 파티클 시스템 설정 (시계 톱니바퀴 에너지 링 형태)
        var main = ringParticles.main;
        main.startLifetime = 1.0f;
        main.startSize = 0.5f;
        main.startColor = new Color(0.2f, 0.6f, 1.0f, 0.8f); // 파란색 기반

        var shape = ringParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 1.0f;

        // 링을 플레이어 주변에 배치
        energyRing.transform.SetParent(playerTransform);
        energyRing.transform.localPosition = Vector3.zero;

        // 설정 후 재생
        ringParticles.Play();
    }

    private void UpdateEnergyRingEffect(float chargeRatio)
    {
        if (energyRing == null) return;

        // 파티클 시스템 참조
        ParticleSystem ps = energyRing.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 차지 비율에 따라 링 크기 증가
            var main = ps.main;
            main.startSize = 0.5f + chargeRatio;

            // 차지 비율에 따라 색상 변화 (파란색 -> 보라색 -> 빨간색)
            Color startColor = Color.Lerp(new Color(0.2f, 0.6f, 1.0f, 0.8f), new Color(0.8f, 0.2f, 1.0f, 0.8f), chargeRatio);
            main.startColor = startColor;

            // 차지 비율에 따라 파티클 방출 속도 증가
            var emission = ps.emission;
            emission.rateOverTime = 10f + 40f * chargeRatio;

            // 최소 차지 비율 도달 시 시각적 피드백
            if (chargeRatio >= minChargeRatio && chargeRatio < minChargeRatio + 0.05f)
            {
                // 최소 차지 도달 시 잠깐 파티클 증가
                emission.rateOverTime = 70f;
            }
        }

        // 링 회전 효과
        energyRing.transform.Rotate(0, 100f * chargeRatio * Time.deltaTime, 0);
    }

    protected override void PerformChargeAttack(float chargeRatio)
    {
        // 차지 비율이 최소값보다 낮으면 공격하지 않음
        if (chargeRatio < minChargeRatio)
        {
            Debug.Log($"차지가 부족합니다. 현재: {chargeRatio * 100:F1}%, 필요: {minChargeRatio * 100:F1}%");

            // 에너지 링 제거
            if (energyRing != null)
            {
                GameObject.Destroy(energyRing);
                energyRing = null;
            }

            return;
        }

        // 기본 데미지 계산
        int baseDamage = weaponManager.BaseDamage;
        int totalDamage = Mathf.RoundToInt(baseDamage * (1 + chargeRatio * weaponManager.weaponData.chargeMultiplier));
        Damage = totalDamage;

        // 대시 방향 결정 (입력 방향 또는 플레이어가 바라보는 방향)
        dashDirection = GetDashDirection();

        // 대시 거리 계산 (차지 시간에 비례)
        float dashDistance = 3f + 5f * chargeRatio; // 최소 3, 최대 8 유닛

        Debug.Log($"시간의 단절 발동! 차지율: {chargeRatio * 100:F1}%, 대시 거리: {dashDistance:F1}, 데미지: {totalDamage}");

        // 대시 및 검기 발사 실행
        PerformDashAndBladeWave(dashDirection, dashDistance, totalDamage, chargeRatio);

        // 에너지 링 제거
        if (energyRing != null)
        {
            GameObject.Destroy(energyRing);
            energyRing = null;
        }
    }

    private Vector3 GetDashDirection()
    {
        // 이 부분은 실제 게임에서 플레이어의 입력 방향을 가져오는 부분
        // 플레이어 입력 관리자에서 입력 방향 가져오기

        Vector3 inputDirection = Vector3.zero;
        if (GameInitializer.Instance.gameObject != null)
        {
            // 플레이어 입력 방향 가져오기 (실제 구현에 맞게 수정 필요)
            inputDirection = GameInitializer.Instance.gameObject.transform.forward;
        }

        // 입력 방향이 있으면 사용, 없으면 플레이어가 바라보는 방향 사용
        Vector3 direction = inputDirection.magnitude > 0.1f ? inputDirection : playerTransform.forward;
        direction.y = 0; // 수직 이동 제한

        return direction.normalized;
    }

    private void PerformDashAndBladeWave(Vector3 direction, float distance, int damage, float chargeRatio)
    {
        // 대시 시작 위치 저장
        Vector3 startPos = playerTransform.position;
        Vector3 targetPos = startPos + direction * distance;

        // 장애물 체크 (선택적)
        RaycastHit hit;
        bool hitObstacle = Physics.Raycast(startPos, direction, out hit, distance, LayerMask.GetMask("Wall", "Obstacle"));
        if (hitObstacle)
        {
            // 장애물이 있으면 그 앞까지만 이동
            targetPos = hit.point - direction * 0.5f; // 살짝 앞에서 정지
        }

        // 플레이어 대시 애니메이션 호출 (구현 필요)
        PlayerClass playerClass = GameInitializer.Instance.GetPlayerClass();
        if (playerClass != null)
        {
            // 실제 구현에서는 플레이어의 대시 메서드 호출
            // playerClass.PerformDash(targetPos);

            // 임시 구현: 플레이어 위치 직접 이동
            // playerClass.transform.position = targetPos;

            // 이벤트 발생 등의 방법으로 플레이어에게 대시 알림
            Debug.Log($"플레이어 대시: {startPos} -> {targetPos}");
        }

        // 검기 생성 및 발사
        CreateBladeWaveEffect(startPos, targetPos, damage, chargeRatio);
    }

    private void CreateBladeWaveEffect(Vector3 startPos, Vector3 endPos, int damage, float chargeRatio)
    {
        if (bladeWavePrefab == null)
        {
            Debug.LogError("검기 프리팹이 설정되지 않았습니다!");
            return;
        }

        // 시작 위치와 방향 계산
        Vector3 direction = (endPos - startPos).normalized;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // 검기 프리팹 인스턴스화
        GameObject bladeWave = GameObject.Instantiate(bladeWavePrefab, startPos, rotation);

        // BladeWave 컴포넌트 설정
        BladeWave bladeWaveComponent = bladeWave.GetComponent<BladeWave>();
        if (bladeWaveComponent != null)
        {
            // 데미지, 차지 비율, 방향 등 초기화
            bladeWaveComponent.Initialize(damage, chargeRatio, direction, parentWeapon);

            // 날아가는 속도 설정 (BladeWave 내부에서 처리하거나 여기서 직접 처리)
            float speed = 15f + 10f * chargeRatio; // 차지 비율에 따라 속도 증가
            Rigidbody rb = bladeWave.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = direction * speed;
            }
        }
        else
        {
            Debug.LogWarning("BladeWave 컴포넌트가 없습니다.");
        }

        // 일정 시간 후 제거 (BladeWave 내부에서 처리하지 않는 경우)
        GameObject.Destroy(bladeWave, 3.0f);
    }
}