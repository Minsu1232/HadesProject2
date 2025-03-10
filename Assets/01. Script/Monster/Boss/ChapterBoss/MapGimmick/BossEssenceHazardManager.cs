using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossEssenceHazardManager : MonoBehaviour
{
    private List<IBossEssenceHazard> registeredHazards = new List<IBossEssenceHazard>();
    private IBossEssenceSystem essenceSystem;
    private float hazardCooldown = 5f;
    private float minCooldown = 2f;
    private float maxCooldown = 5f;
    private float timeSinceLastHazard = 0f;
    private bool isActive = false;
    private float defaultActivationThreshold = 70f; // 추가된 기본 임계값

    public void Initialize(IBossEssenceSystem essenceSystem, float minCooldown = 2f, float maxCooldown = 5f)
    {
        this.essenceSystem = essenceSystem;
        this.minCooldown = minCooldown;
        this.maxCooldown = maxCooldown;
        this.hazardCooldown = maxCooldown;

        essenceSystem.OnEssenceChanged += OnEssenceChanged;
        essenceSystem.OnEssenceStateChanged += OnEssenceStateChanged;

        Debug.Log("BossEssenceHazardManager 초기화 완료");
    }

    public void RegisterHazard(IBossEssenceHazard hazard)
    {
        hazard.Initialize(essenceSystem);
        registeredHazards.Add(hazard);
        Debug.Log($"위험요소 등록: {hazard.HazardName}");
    }

    private void Update()
    {
        if (!isActive) return;

        timeSinceLastHazard += Time.deltaTime;

        // 충분한 시간이 지났고 에센스가 임계점 이상이면 위험요소 발동
        if (timeSinceLastHazard >= hazardCooldown && essenceSystem.IsInEssenceState)
        {
            TriggerRandomHazard();

            // 에센스 수치에 따라 다음 쿨다운 조정
            // 위험요소의 평균 임계값 사용 또는 70%로 고정
            float thresholdValue = registeredHazards.Count > 0
     ? registeredHazards.Average(h => h.ActivationThreshold)
     : defaultActivationThreshold;

            float essenceRatio = Mathf.Clamp01((essenceSystem.CurrentEssence - thresholdValue) /
                                (100f - thresholdValue));
            hazardCooldown = Mathf.Lerp(maxCooldown, minCooldown, essenceRatio);

            timeSinceLastHazard = 0f;
        }
    }

    private void TriggerRandomHazard()
    {
        if (registeredHazards.Count == 0) return;

        // 현재 활성화 가능한 위험요소 필터링
        var activeHazards = registeredHazards
            .Where(h => essenceSystem.CurrentEssence >= h.ActivationThreshold)
            .ToList();

        if (activeHazards.Count > 0)
        {
            // 랜덤 위험요소 선택
            int randomIndex = UnityEngine.Random.Range(0, activeHazards.Count);
            IBossEssenceHazard selectedHazard = activeHazards[randomIndex];

            // 플레이어 주변에 위험요소 생성
            Vector3 position = GetHazardPosition();
            float intensity = CalculateHazardIntensity(selectedHazard);

            selectedHazard.ActivateHazard(position, intensity);
            Debug.Log($"위험요소 발동: {selectedHazard.HazardName}, 강도: {intensity:F2}");
        }
    }

    private Vector3 GetHazardPosition()
    {
        // 플레이어 위치 확인
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        Vector3 playerPos = player.playerTransform.position;

        // 플레이어 근처에 위험요소 배치 (약간의 랜덤성 추가)
        float radius = UnityEngine.Random.Range(2f, 5f);
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle) * radius,
            0f,
            Mathf.Sin(angle) * radius
        );

        return playerPos + offset;
    }

    private float CalculateHazardIntensity(IBossEssenceHazard hazard)
    {
        // 에센스 수치에 따른 위험요소 강도 계산
        float normalizedEssence = Mathf.Clamp01(
            (essenceSystem.CurrentEssence - hazard.ActivationThreshold) /
            (100f - hazard.ActivationThreshold)
        );

        // 0.8~1.5 범위의 강도로 변환 (최소치와 최대치 설정)
        return Mathf.Lerp(0.8f, 1.5f, normalizedEssence);
    }

    private void OnEssenceChanged(float newValue)
    {
        foreach (var hazard in registeredHazards)
        {
            hazard.UpdateHazardIntensity(newValue);
        }
    }

    private void OnEssenceStateChanged()
    {
        isActive = essenceSystem.IsInEssenceState;

        // 광기 상태에 진입하면 첫 번째 위험요소 즉시 발동
        if (isActive && timeSinceLastHazard > minCooldown)
        {
            timeSinceLastHazard = hazardCooldown;
        }
    }

    private void OnDestroy()
    {
        if (essenceSystem != null)
        {
            essenceSystem.OnEssenceChanged -= OnEssenceChanged;
            essenceSystem.OnEssenceStateChanged -= OnEssenceStateChanged;
        }
    }
}