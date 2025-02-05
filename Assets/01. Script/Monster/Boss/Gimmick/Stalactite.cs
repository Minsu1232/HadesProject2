using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class Stalactite : HazardObject
{
    private float spawnAreaRadius;
    private float groundHeight = float.MinValue;

    public override void Initialize(float radius, float dmg, float speed, HazardSpawnType type,
       TargetType targetType, float height, float areaRadius, IGimmickStrategy strategy)
    {
        base.Initialize(radius, dmg, speed, type, targetType, height, areaRadius, strategy);
        this.spawnHeight = height;
        this.spawnAreaRadius = areaRadius;
        this.warningDuration = CalculateWarningDuration(height);
        this.warningTimer = warningDuration;
        transform.position = new Vector3(transform.position.x, spawnHeight, transform.position.z);
    }

    protected override float CalculateWarningDuration(float height)
    {
        float fallDistance = spawnHeight - 0.01f;
        return fallDistance / moveSpeed;
    }

    public override void StartMove()
    {
        isWarning = true;
    }

    protected override void Update()
    {
        base.Update();
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }

    protected override void OnWarningUpdate()
    {
        if (warningIndicator == null) return;

        Vector3 hazardPos = transform.position;
        warningIndicator.transform.position = new Vector3(
            hazardPos.x,
            GetGroundHeight(hazardPos) + 0.01f,
            hazardPos.z
        );

        if (warningTimer <= 0)
        {
            isWarning = false;
            warningIndicator.SetActive(false);
        }
    }

    private float GetGroundHeight(Vector3 position)
    {
        if (groundHeight == float.MinValue)
        {
            if (Physics.Raycast(position + Vector3.up * 100f, Vector3.down, out RaycastHit hit, 1000f, LayerMask.GetMask("Ground")))
            {
                groundHeight = hit.point.y;
            }
            else
            {
                groundHeight = position.y;
            }
        }
        return groundHeight;
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        HashSet<IDamageable> damagedEntities = new HashSet<IDamageable>();
        bool hasHitAnything = false;

        foreach (var hit in hits)
        {
            IDamageable damageable = null;

            // 먼저 MonsterHitBox를 확인
            MonsterHitBox hitBox = hit.GetComponent<MonsterHitBox>();
            if (hitBox != null)
            {
                var monsterStatus = hitBox.GetMonsterStatus();
                if (monsterStatus != null)
                {
                    damageable = monsterStatus;
                    Debug.Log($"HitBox를 통해 몬스터 감지: {monsterStatus.gameObject.name}");
                }
            }
            // Player 태그 확인
            else if (hit.CompareTag("Player"))
            {
                damageable = GameInitializer.Instance.GetPlayerClass();
                Debug.Log("플레이어 감지");
            }

            if (damageable != null && !damagedEntities.Contains(damageable))
            {
                damagedEntities.Add(damageable);
                DamageType damageType = damageable.GetDamageType();
                Debug.Log($"데미지 타입: {damageType}");

                switch (damageType)
                {
                    case DamageType.Player:
                        damageable.TakeDamage((int)damage);
                        hasHitAnything = true;
                        continue;
                    case DamageType.Monster:
                        damageable.TakeDamage((int)(damage * 0.5f));
                        hasHitAnything = true;
                        continue;
                    case DamageType.Boss:
                        gimmickStrategy?.SucessTrigget();
                        hasHitAnything = true;
                        Debug.Log("보스히트");
                        continue;
                }
            }
        }

        if (hasHitAnything)
        {
            OnImpact();
        }

    }

    protected override void OnImpact()
    {
        if (warningIndicator != null)
        {
            warningIndicator.SetActive(false);
        }

        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    protected override Vector3 GetRandomSpawnPosition()
    {
        float randomX = Random.Range(-spawnAreaRadius, spawnAreaRadius);
        float randomZ = Random.Range(-spawnAreaRadius, spawnAreaRadius);
        return new Vector3(randomX, spawnHeight, randomZ);
    }

    protected override Vector3 GetAboveTargetPosition(Transform target)
    {
        return new Vector3(target.position.x, spawnHeight, target.position.z);
    }

    protected override Vector3 GetFixedSpawnPosition()
    {
        Vector3[] fixedPositions = {
           new Vector3(-5, spawnHeight, -5),
           new Vector3(0, spawnHeight, 0),
           new Vector3(5, spawnHeight, 5)
       };
        return fixedPositions[Random.Range(0, fixedPositions.Length)];
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, damageRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, damageRadius);
    }

    protected override void CheckDamage()
    {
        throw new System.NotImplementedException();
    }
#endif
}