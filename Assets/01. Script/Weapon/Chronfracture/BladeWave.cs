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
        this.speed = 15f + 10f * chargeRatio; // ���� ������ ���� �ӵ� ����

        // ��ƼŬ ȿ�� ���� �� ũ�� ����
        UpdateVisuals();

        isInitialized = true;

        // ���� �ð� �� �ڵ� �ı�
        Destroy(gameObject, 3f);
    }

    private void UpdateVisuals()
    {
        if (mainEffect == null)
        {
            mainEffect = GetComponent<ParticleSystem>();
        }

        // ���� ����Ʈ ����
        if (mainEffect != null)
        {
            // ��ƼŬ �ý��� ���� �� ���� ����
            mainEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = mainEffect.main;
            // ����� �迭�� ���� (���� ������ ���� ��� ��ȭ)
            Color baseColor = new Color(0.8f, 0.2f, 1f, 0.8f);
            Color chargedColor = new Color(1f, 0.3f, 1f, 0.9f);
            main.startColor = Color.Lerp(baseColor, chargedColor, chargeRatio);

            // ũ�⵵ ���� ������ ���� ����
            main.startSize = main.startSize.constant * (1f + chargeRatio * 0.5f);

            // ���
            mainEffect.Play();
        }

        // Ʈ���� ����Ʈ ����
        if (trailEffect != null)
        {
            trailEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var trailMain = trailEffect.main;
            Color trailColor = new Color(0.6f, 0.2f, 0.9f, 0.7f);
            trailMain.startColor = trailColor;

            // ���
            trailEffect.Play();
        }

        // �浹 ����Ʈ�� �浹 �� ����� �����̹Ƿ� ���⼭�� ������ ����
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

        // �������� �̵���Ű�� ��� (Rigidbody�� ������� �ʴ� ���)
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ���� Ȯ��
        if (other.CompareTag("Dummy"))
        {
            IDamageable dummyTarget = other.GetComponent<IDamageable>();
            if (dummyTarget != null)
            {
                int finalDamage_ = Mathf.RoundToInt(damage);
                dummyTarget.TakeDamage(finalDamage_);

                // �浹 ȿ�� ���
                PlayImpactEffect(other.transform.position);

                Debug.Log($"�˱Ⱑ ���̿��� {finalDamage_} �������� �������ϴ�.");
            }
            return;
        }

        // ���� ��Ʈ�ڽ� Ȯ��
        MonsterHitBox hitBox = other.GetComponent<MonsterHitBox>();
        if (hitBox == null) return;

        ICreatureStatus monster = hitBox.GetMonsterStatus();
        if (monster == null) return;

        // �̹� ���� ���͸� ����
        if (hitTargets.Contains(monster)) return;

        // ������ ��� �� ����
        float damageMultiplier = hitBox.GetDamageMultiplier(transform.position);
        int finalDamage = Mathf.RoundToInt(damage * damageMultiplier);

        // ������ �Ϲ� ���� ��� ó��
        if (monster is IDamageable damageable)
        {
            if (damageMultiplier > 1f)
            {
                Debug.Log("�˱� �����!");
            }

            damageable.TakeDamage(finalDamage);

            // �浹 ȿ�� ���
            PlayImpactEffect(other.transform.position);

            string monsterType = monster is BossStatus ? "����" : "�Ϲݸ�";
            Debug.Log($"�˱Ⱑ {monsterType}���� {finalDamage} �������� �������ϴ�.");

            hitTargets.Add(monster);

            // �ð� �޾Ƹ� 1���� �ο�
            if (parentWeapon != null)
            {
                parentWeapon.AddEchoStackToTarget(damageable);
                Debug.Log("�˱⿡ ���� �ð� �޾Ƹ� 1���� �ο�");
            }
        }
    }

    private void PlayImpactEffect(Vector3 position)
    {
        // �浹 ����Ʈ ��� (prefab�� �ִ� ���)
        if (impactEffect != null)
        {
            impactEffect.transform.position = position;
            impactEffect.Play();
        }
        else
        {
            // �浹 ����Ʈ�� ������ ������ ��ƼŬ ����
            GameObject impactObj = new GameObject("ImpactEffect");
            impactObj.transform.position = position;

            ParticleSystem ps = impactObj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startColor = new Color(1f, 0.5f, 1f, 0.8f);
            main.duration = 0.5f;
            main.startLifetime = 0.5f;
            main.startSize = 1f + chargeRatio * 0.5f;

            // �ڵ� �ı�
            Destroy(impactObj, 1f);
        }

        // �Ҹ� ��� (�ִ� ���)
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            AudioSource.PlayClipAtPoint(audioSource.clip, position);
        }
    }

    // �浹 �� �˱� ���� (������)
    private void OnCollisionEnter(Collision collision)
    {
        // ���̳� ��ֹ��� �浹�ϸ� �ı�
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            PlayImpactEffect(collision.contacts[0].point);
            Destroy(gameObject);
        }
    }
}