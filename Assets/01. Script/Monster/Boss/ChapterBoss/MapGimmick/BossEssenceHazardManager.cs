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
    private float defaultActivationThreshold = 70f; // �߰��� �⺻ �Ӱ谪

    public void Initialize(IBossEssenceSystem essenceSystem, float minCooldown = 2f, float maxCooldown = 5f)
    {
        this.essenceSystem = essenceSystem;
        this.minCooldown = minCooldown;
        this.maxCooldown = maxCooldown;
        this.hazardCooldown = maxCooldown;

        essenceSystem.OnEssenceChanged += OnEssenceChanged;
        essenceSystem.OnEssenceStateChanged += OnEssenceStateChanged;

        Debug.Log("BossEssenceHazardManager �ʱ�ȭ �Ϸ�");
    }

    public void RegisterHazard(IBossEssenceHazard hazard)
    {
        hazard.Initialize(essenceSystem);
        registeredHazards.Add(hazard);
        Debug.Log($"������ ���: {hazard.HazardName}");
    }

    private void Update()
    {
        if (!isActive) return;

        timeSinceLastHazard += Time.deltaTime;

        // ����� �ð��� ������ �������� �Ӱ��� �̻��̸� ������ �ߵ�
        if (timeSinceLastHazard >= hazardCooldown && essenceSystem.IsInEssenceState)
        {
            TriggerRandomHazard();

            // ������ ��ġ�� ���� ���� ��ٿ� ����
            // �������� ��� �Ӱ谪 ��� �Ǵ� 70%�� ����
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

        // ���� Ȱ��ȭ ������ ������ ���͸�
        var activeHazards = registeredHazards
            .Where(h => essenceSystem.CurrentEssence >= h.ActivationThreshold)
            .ToList();

        if (activeHazards.Count > 0)
        {
            // ���� ������ ����
            int randomIndex = UnityEngine.Random.Range(0, activeHazards.Count);
            IBossEssenceHazard selectedHazard = activeHazards[randomIndex];

            // �÷��̾� �ֺ��� ������ ����
            Vector3 position = GetHazardPosition();
            float intensity = CalculateHazardIntensity(selectedHazard);

            selectedHazard.ActivateHazard(position, intensity);
            Debug.Log($"������ �ߵ�: {selectedHazard.HazardName}, ����: {intensity:F2}");
        }
    }

    private Vector3 GetHazardPosition()
    {
        // �÷��̾� ��ġ Ȯ��
        PlayerClass player = GameInitializer.Instance.GetPlayerClass();
        Vector3 playerPos = player.playerTransform.position;

        // �÷��̾� ��ó�� ������ ��ġ (�ణ�� ������ �߰�)
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
        // ������ ��ġ�� ���� ������ ���� ���
        float normalizedEssence = Mathf.Clamp01(
            (essenceSystem.CurrentEssence - hazard.ActivationThreshold) /
            (100f - hazard.ActivationThreshold)
        );

        // 0.8~1.5 ������ ������ ��ȯ (�ּ�ġ�� �ִ�ġ ����)
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

        // ���� ���¿� �����ϸ� ù ��° ������ ��� �ߵ�
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