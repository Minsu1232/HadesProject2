// ItemPickupObject.cs - ���忡 ������ ������ ȹ�� ó�� ������Ʈ (���� ����)
using System.Collections;
using UnityEngine;
using TMPro;

public class ItemPickupObject : MonoBehaviour
{
    [Header("������ ����")]
    private Item item;                      // ������ ����
    private int quantity = 1;               // ������ ����
    private bool isBossItem = false;        // ���� ������ ����

    [Header("�ð��� ȿ��")]
    [SerializeField] private ParticleSystem itemParticle;    // ������ ��ƼŬ ȿ��
    [SerializeField] private TextMeshProUGUI quantityText;   // ���� �ؽ�Ʈ
    [SerializeField] private Light pointLight;               // ����Ʈ ����Ʈ

    [Header("��͵� ����")]
    [SerializeField]
    private Color[] rarityColors = new Color[] {
        new Color(0.7f, 0.7f, 0.7f), // Common - ȸ��
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - ���
        new Color(0.3f, 0.3f, 0.9f), // Rare - �Ķ���
        new Color(0.7f, 0.3f, 0.9f), // Epic - �����
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - �ݻ�
    };
    [SerializeField] private GameObject[] raritySpawnEffects;   // ��޺� ���� ����Ʈ
    [SerializeField] private GameObject[] rarityPickupEffects;  // ��޺� ȹ�� ����Ʈ

    [Header("������ ����")]
    [SerializeField] private float floatHeight = 0.2f;    // �յ� ���ٴϴ� ����
    [SerializeField] private float floatSpeed = 1.0f;     // �յ� ���ٴϴ� �ӵ�
    [SerializeField] private float rotateSpeed = 30f;     // ȸ�� �ӵ�

    [Header("�ڵ� ȹ�� ����")]
    [SerializeField] private bool enableAutoPickup = true;    // �ڵ� ȹ�� Ȱ��ȭ
    [SerializeField] private float autoPickupDelay = 1.5f;    // �ڵ� ȹ�� ������
    [SerializeField] private float attractionRadius = 3.0f;   // �÷��̾� ���� �ݰ�
    [SerializeField] private float attractionSpeed = 5.0f;    // �������� �ӵ�
    [SerializeField] private AnimationCurve attractionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // �������� �ӵ� Ŀ��

    [Header("ȿ�� ����")]
    [SerializeField] private AudioClip pickupSound;     // ȹ�� ����
    [SerializeField] private AudioClip spawnSound;      // ���� ����

    [Header("��ȣ�ۿ� ����")]
    [SerializeField] private LayerMask playerLayer;        // �÷��̾� ���̾�
    [SerializeField] private float interactionRadius = 1.5f; // ��ȣ�ۿ� �ݰ�

    // ������Ʈ ����
    private Rigidbody rb;
    private Collider col;
    private AudioSource audioSource;
    private Transform playerTransform;
    private Vector3 startPosition;
    private Coroutine attractionRoutine;
    private bool isPickedUp = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D ����
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 20.0f;
        }

        // ����Ʈ ����Ʈ�� �������� �ʾҴٸ� ã��
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();
        }
    }

    // ������ ���� �ʱ�ȭ
    public void Initialize(Item itemData, int itemQuantity, bool bossItem = false)
    {
        this.item = itemData;
        this.quantity = itemQuantity;
        this.isBossItem = bossItem;

        // ��ƼŬ ���� ����
        if (itemParticle != null)
        {
            // ��޿� ���� ���� ����
            Color particleColor = GetRarityColor(item.rarity);

            // ��ƼŬ �ý��� ���� ��� ���� ����
            ParticleSystem.MainModule main = itemParticle.main;
            main.startColor = particleColor;

            // ���� �������̸� ��ƼŬ ũ�� ����
            if (bossItem)
            {
                main.startSize = main.startSize.constant * 1.5f;
            }
        }

        // ����Ʈ ����Ʈ ����
        if (pointLight != null)
        {
            // ��͵��� ���� ����Ʈ Ȱ��ȭ (�Ϲ� ����� ����Ʈ ����)
            pointLight.enabled = item.rarity > Item.ItemRarity.Common;

            if (pointLight.enabled)
            {
                // ���� ����
                pointLight.color = GetRarityColor(item.rarity);

                // ��͵��� ���� ���� ����
                float intensity = 0.5f;
                switch (item.rarity)
                {
                    case Item.ItemRarity.Uncommon:
                        intensity = 0.5f;
                        break;
                    case Item.ItemRarity.Rare:
                        intensity = 0.7f;
                        break;
                    case Item.ItemRarity.Epic:
                        intensity = 1.0f;
                        break;
                    case Item.ItemRarity.Legendary:
                        intensity = 1.5f;
                        break;
                }
                pointLight.intensity = intensity;

                // ���� �������̸� ����Ʈ ��ȭ
                if (bossItem)
                {
                    pointLight.intensity *= 1.5f;
                    pointLight.range *= 1.3f;
                }
            }
        }

        // ���� �ؽ�Ʈ ����
        if (quantityText != null)
        {
            if (quantity > 1)
            {
                quantityText.text = quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        // ���� �������̸� ũ�� ����
        if (bossItem)
        {
            transform.localScale *= 1.2f;
        }

        // ���� ��ġ ����
        startPosition = transform.position;

        // ���� ����Ʈ �� ���� ���
        PlaySpawnEffects();

        // �ڵ� ȹ�� ����
        if (enableAutoPickup)
        {
            StartCoroutine(EnableAutoPickupAfterDelay());
        }

        // ���� ������Ʈ �̸� ����
        gameObject.name = $"Item_{item.itemName}_{item.rarity}";
    }

    // ��޺� ���� ��ȯ
    private Color GetRarityColor(Item.ItemRarity rarity)
    {
        int index = Mathf.Min((int)rarity, rarityColors.Length - 1);
        return rarityColors[index];
    }

    // ���� ����Ʈ �� ���� ���
    private void PlaySpawnEffects()
    {
        // ��޿� �´� ����Ʈ ����
        if (raritySpawnEffects != null && raritySpawnEffects.Length > 0)
        {
            int index = Mathf.Min((int)item.rarity, raritySpawnEffects.Length - 1);

            if (raritySpawnEffects[index] != null)
            {
                // ������ ����� ����Ʈ ����
                Instantiate(raritySpawnEffects[index], transform.position, Quaternion.identity);
            }
        }

        // ���� ���
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    // �ڵ� ȹ�� ������ �� Ȱ��ȭ
    private IEnumerator EnableAutoPickupAfterDelay()
    {
        // ���� �ùķ��̼��� ����ȭ�ǵ��� ���� �ð� ���
        yield return new WaitForSeconds(autoPickupDelay);

        // �÷��̾� ã��
        playerTransform = FindPlayerTransform();

        if (playerTransform != null)
        {
            // �÷��̾� ���� �� �η� �ۿ�
            StartCoroutine(DetectPlayerProximity());
        }
    }

    // �÷��̾� Ʈ������ ã��
    private Transform FindPlayerTransform()
    {
        // �÷��̾� �±׷� ã��
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player?.transform;
    }

    // �÷��̾� ���� ����
    private IEnumerator DetectPlayerProximity()
    {
        while (!isPickedUp && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // ��ȣ�ۿ� ���� ���� ������ ȹ��
            if (distance <= interactionRadius)
            {
                PickupItem();
                yield break;
            }
            // �������� ���� ���� ������ �÷��̾�� �̵�
            else if (distance <= attractionRadius)
            {
                if (attractionRoutine == null)
                {
                    attractionRoutine = StartCoroutine(MoveTowardsPlayer());
                }
            }

            yield return null;
        }
    }

    // �÷��̾� �������� �̵�
    private IEnumerator MoveTowardsPlayer()
    {
        // ���� �ùķ��̼� ��Ȱ��ȭ
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        float startTime = Time.time;

        while (!isPickedUp && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // ��ȣ�ۿ� ������ �����ϸ� ȹ��
            if (distanceToPlayer <= interactionRadius)
            {
                PickupItem();
                yield break;
            }

            // ��� �ð��� ���� �ӵ� ���� (Ŀ�� ����)
            float elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / 1.0f); // 1�ʵ��� ����
            float speedMultiplier = attractionCurve.Evaluate(t);

            // �÷��̾� ������ �̵�
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attractionSpeed * speedMultiplier * Time.deltaTime;

            yield return null;
        }
    }

    private void Update()
    {
        if (!isPickedUp)
        {
            // �յ� ���ٴϴ� ȿ��
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // ȸ�� ȿ��
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    // Ʈ���� �浹 ���� (�÷��̾ ���� �����ۿ� ����� ��)
    private void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && IsPlayerCollider(other))
        {
            PickupItem();
        }
    }

    // �÷��̾� �ݶ��̴����� Ȯ��
    private bool IsPlayerCollider(Collider other)
    {
        return ((1 << other.gameObject.layer) & playerLayer) != 0 || other.CompareTag("Player");
    }

    // ������ ȹ�� ó��
    private void PickupItem()
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // �κ��丮�� ������ �߰�
        if (InventorySystem.Instance != null)
        {
            bool added = InventorySystem.Instance.AddItem(item, quantity);

            if (added)
            {
                // ȹ�� ����Ʈ �� ���� ���
                PlayPickupEffects();

                // ������Ʈ ����
                StartCoroutine(DestroyAfterEffects());
            }
            else
            {
                Debug.LogWarning("�κ��丮�� ���� ���� �������� ȹ���� �� �����ϴ�.");
                isPickedUp = false; // �ٽ� ȹ�� �����ϵ��� ����
            }
        }
        else
        {
            Debug.LogError("InventorySystem �ν��Ͻ��� ã�� �� �����ϴ�.");
            isPickedUp = false;
        }
    }

    // ȹ�� ����Ʈ �� ���� ���
    private void PlayPickupEffects()
    {
        // ��޿� �´� ����Ʈ ����
        if (rarityPickupEffects != null && rarityPickupEffects.Length > 0)
        {
            int index = Mathf.Min((int)item.rarity, rarityPickupEffects.Length - 1);

            if (rarityPickupEffects[index] != null)
            {
                // ������ ����� ����Ʈ ����
                Instantiate(rarityPickupEffects[index], transform.position, Quaternion.identity);
            }
        }

        // ���� ���
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    // ����Ʈ ��� �� ������Ʈ ����
    private IEnumerator DestroyAfterEffects()
    {
        // �ݶ��̴� ��Ȱ��ȭ
        if (col != null) col.enabled = false;

        // ������ ��Ȱ��ȭ
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // ����Ʈ ��Ȱ��ȭ
        if (pointLight != null)
        {
            pointLight.enabled = false;
        }

        // ��ƼŬ �ý��� ����
        if (itemParticle != null)
        {
            itemParticle.Stop(true);
        }

        // ����Ʈ �� ���� ��� ���
        float delay = pickupSound != null ? pickupSound.length : 0.5f;
        yield return new WaitForSeconds(delay);

        // ���� ������Ʈ ����
        Destroy(gameObject);
    }

    // �����⿡�� ����� ǥ�� (������)
    private void OnDrawGizmosSelected()
    {
        // ��ȣ�ۿ� ���� ǥ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // �ڵ� ȹ�� ���� ���� ǥ��
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}