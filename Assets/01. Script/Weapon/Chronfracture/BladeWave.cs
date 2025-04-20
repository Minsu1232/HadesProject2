using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BladeWave : MonoBehaviour
{
    private int damage;
    private float chargeRatio;
    private Vector3 moveDirection;
    private Chronofracture parentWeapon;
    private float speed;
    private HashSet<ICreatureStatus> hitTargets = new HashSet<ICreatureStatus>();

    [SerializeField] private ParticleSystem mainEffect;
    [SerializeField] private ParticleSystem trailEffect;
    [SerializeField] private ParticleSystem impactEffect;

    private bool isInitialized = false;

    public void Initialize(int damage, float chargeRatio, Vector3 direction, Chronofracture weapon)
    {
        this.damage = damage;
        this.chargeRatio = chargeRatio;
        this.moveDirection = direction;
        this.parentWeapon = weapon;
        this.speed = 15f + 10f * chargeRatio; // 차지 비율에 따라 속도 조정

        // 파티클 효과 색상 및 크기 조정
        UpdateVisuals();

        isInitialized = true;

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, 3f);
    }

    private void UpdateVisuals()
    {
        if (mainEffect == null)
        {
            mainEffect = GetComponent<ParticleSystem>();
        }

        // 메인 이펙트 조정
        if (mainEffect != null)
        {
            // 파티클 시스템 정지 후 설정 변경
            mainEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = mainEffect.main;
            // 보라색 계열로 설정 (차지 비율에 따라 밝기 변화)
            Color baseColor = new Color(0.8f, 0.2f, 1f, 0.8f);
            Color chargedColor = new Color(1f, 0.3f, 1f, 0.9f);
            main.startColor = Color.Lerp(baseColor, chargedColor, chargeRatio);

            // 크기도 차지 비율에 따라 조정
            main.startSize = main.startSize.constant * (1f + chargeRatio * 0.5f);

            // 재생
            mainEffect.Play();
        }

        // 트레일 이펙트 조정
        if (trailEffect != null)
        {
            trailEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var trailMain = trailEffect.main;
            Color trailColor = new Color(0.6f, 0.2f, 0.9f, 0.7f);
            trailMain.startColor = trailColor;

            // 재생
            trailEffect.Play();
        }

        // 충돌 이펙트는 충돌 시 재생할 예정이므로 여기서는 설정만 조정
        if (impactEffect != null)
        {
            impactEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var impactMain = impactEffect.main;
            Color impactColor = new Color(1f, 0.4f, 1f, 0.9f);
            impactMain.startColor = impactColor;
        }
    }

    private void Update()
    {
        if (!isInitialized) return;

        // 수동으로 이동시키는 경우 (Rigidbody를 사용하지 않는 경우)
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 더미 확인
        if (other.CompareTag("Dummy"))
        {
            IDamageable dummyTarget = other.GetComponent<IDamageable>();
            if (dummyTarget != null)
            {
                int finalDamage_ = Mathf.RoundToInt(damage);
                dummyTarget.TakeDamage(finalDamage_);

                // 충돌 효과 재생
                PlayImpactEffect(other.transform.position);

                Debug.Log($"검기가 더미에게 {finalDamage_} 데미지를 입혔습니다.");
            }
            return;
        }

        // 몬스터 히트박스 확인
        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox == null) return;

        ICreatureStatus monster = hitBox.GetMonsterStatus();
        if (monster == null) return;

        // 이미 때린 몬스터면 리턴
        if (hitTargets.Contains(monster)) return;

        // 데미지 계산 및 적용
        float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
        int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // 보스와 일반 몬스터 모두 처리
        if (monster is IDamageable damageable)
        {
            if (damageMultiplier > 1f)
            {
                Debug.Log("검기 백어택!");
            }

            damageable.TakeDamage(finalDamage);

            // 충돌 효과 재생
            PlayImpactEffect(other.transform.position);

            string monsterType = monster is BossStatus ? "보스" : "일반몹";
            Debug.Log($"검기가 {monsterType}에게 {finalDamage} 데미지를 입혔습니다.");

            hitTargets.Add(monster);

            // 시간 메아리 1스택 부여
            if (parentWeapon != null)
            {
                parentWeapon.AddEchoStackToTarget(damageable);
                Debug.Log("검기에 의한 시간 메아리 1스택 부여");
            }
        }
    }

    private void PlayImpactEffect(Vector3 position)
    {
        // 충돌 이펙트 재생 (prefab에 있는 경우)
        if (impactEffect != null)
        {
            impactEffect.transform.position = position;
            impactEffect.Play();
        }
        else
        {
            // 충돌 이펙트가 없으면 간단한 파티클 생성
            GameObject impactObj = new GameObject("ImpactEffect");
            impactObj.transform.position = position;

            ParticleSystem ps = impactObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.5f, 1f, 0.8f);
            main.duration = 0.5f;
            main.startLifetime = 0.5f;
            main.startSize = 1f + chargeRatio * 0.5f;

            // 자동 파괴
            Destroy(impactObj, 1f);
        }

        // 소리 재생 (있는 경우)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            AudioSource.PlayClipAtPoint(audioSource.clip, position);
        }
    }

    // 충돌 시 검기 제거 (선택적)
    private void OnCollisionEnter(Collision collision)
    {
        // 벽이나 장애물과 충돌하면 파괴
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            PlayImpactEffect(collision.contacts[0].point);
            Destroy(gameObject);
        }
    }
}