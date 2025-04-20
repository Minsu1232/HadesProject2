using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TimeResidue : MonoBehaviour
{
    private float duration;
    private float damage;
    private int comboStep;
    private float timer;
    private Chronofracture parentWeapon;
    private HashSet<ICreatureStatus> damagedMonsters = new HashSet<ICreatureStatus>();
    private bool isClearingDamaged = false;

    private ParticleSystem residueParticleSystem;
    private ParticleSystem collisionParticleSystem;
    private Color residueColor;

    public void Initialize(Chronofracture weapon, float duration, int comboStep, float damage)
    {
        this.parentWeapon = weapon;
        this.duration = duration;
        this.comboStep = comboStep;
        this.damage = damage;

        // 파티클 시스템 참조 (이미 프리팹에 있음)
        residueParticleSystem = GetComponent<ParticleSystem>();
        if (residueParticleSystem != null)
        {
            // 이미 실행 중인 파티클 시스템을 정지한 후 속성 변경
            residueParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = residueParticleSystem.main;
            main.duration = duration;
            main.startLifetime = duration;

            // 수정 후 다시 재생
            residueParticleSystem.Play();
        }

        // 충돌 효과용 파티클 시스템 (자식 오브젝트에 있을 수 있음)
        Transform collisionEffect = transform.Find("CollisionEffect");
        if (collisionEffect != null)
        {
            collisionParticleSystem = collisionEffect.GetComponent<ParticleSystem>();
            if (collisionParticleSystem != null)
            {
                collisionParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, duration + 0.1f);
    }

    public void SetColor(Color color)
    {
        residueColor = color;

        if (residueParticleSystem != null)
        {
            var main = residueParticleSystem.main;
            main.startColor = residueColor;
        }

        // 자식 파티클 시스템들의 색상도 설정
        ParticleSystem[] childParticles = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in childParticles)
        {
            if (ps != residueParticleSystem) // 메인 파티클은 이미 처리했으므로 스킵
            {
                var childMain = ps.main;
                childMain.startColor = residueColor;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // ClearDamagedMonsters() 호출 직후 특정 시간 동안은 OnTriggerEnter 무시
        if (isClearingDamaged)
        {
            Debug.Log("Clear 중이므로 OnTriggerEnter 무시");
            return;
        }

        // 더미 확인
        if (other.CompareTag("Dummy"))
        {
            Debug.Log("1");
            IDamageable dummyTarget = other.GetComponent<IDamageable>();
            if (dummyTarget != null)
            {
                Debug.Log("2");
                int finalDamage_ = Mathf.RoundToInt(damage);
                dummyTarget.TakeDamage(finalDamage_);

                // 충돌 효과 재생
                PlayCollisionEffect(other);

                Debug.Log($"시간 잔상이 더미에게 {finalDamage_} 데미지를 입혔습니다.");
                return; // 더미 처리 후 종료
            }
        }

        // 몬스터 히트박스 확인
        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox == null)
        {
            return;
        }

        ICreatureStatus monster = hitBox.GetMonsterStatus();
        if (monster == null)
        {
            return;
        }

        // 이미 때린 몬스터면 리턴
        if (damagedMonsters.Contains(monster))
        {
            return;
        }

        // 데미지 계산 및 적용
        float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
        int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // 보스와 일반 몬스터 모두 처리
        if (monster is IDamageable damageable)
        {
            if (damageMultiplier > 1f)
            {
                Debug.Log("시간 잔상 백어택!");
            }

            damageable.TakeDamage(finalDamage);

            // 충돌 효과 재생
            PlayCollisionEffect(other);

            string monsterType = monster is BossStatus ? "보스" : "일반몹";
            Debug.Log($"시간 잔상이 {monsterType}에게 {finalDamage} 데미지를 입혔습니다.");

            damagedMonsters.Add(monster);

            // 게이지 충전 (선택적으로 적용)
            if (parentWeapon != null)
            {
                // 시간 잔상은 기본 게이지의 일부만 충전하도록 설정 가능
                parentWeapon.GetGage(Mathf.RoundToInt(parentWeapon.GagePerHit * 0.3f));
            }
        }
    }

    // 충돌 효과 재생 메서드
    private void PlayCollisionEffect(Collider other)
    {
        if (collisionParticleSystem != null)
        {
            // 파티클 위치를 충돌 지점으로 설정
            Vector3 collisionPoint = other.ClosestPointOnBounds(transform.position);
            collisionParticleSystem.transform.position = collisionPoint;

            // 파티클 색상 설정
            var main = collisionParticleSystem.main;
            main.startColor = residueColor;

            // 파티클 재생
            collisionParticleSystem.Play();
        }

    }

    public void ClearDamagedMonsters()
    {
        damagedMonsters.Clear();
        Debug.Log("시간 잔상 damagedMonsters 클리어됨");

        // Clear 직후, 일정 시간 동안 충돌을 무시하도록 설정
        isClearingDamaged = true;
        StartCoroutine(ResetClearingFlag());
    }

    private IEnumerator ResetClearingFlag()
    {
        // 원하는 시간(예: 0.3초) 동안 OnTriggerEnter를 무시
        yield return new WaitForSeconds(0.3f);
        isClearingDamaged = false;
    }

    private void Update()
    {
        // 잔여 시간에 따라 효과 조정 가능 (선택적)
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            // 지속 시간이 끝나면 자연스럽게 사라지게
            Destroy(gameObject);
        }
    }

    // 메모리 정리 및 로그 출력
    private void OnDestroy()
    {
        damagedMonsters.Clear();
    }
}