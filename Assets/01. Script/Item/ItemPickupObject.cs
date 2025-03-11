// ItemPickupObject.cs - 월드에 생성된 아이템 획득 처리 컴포넌트 (최종 버전)
using System.Collections;
using UnityEngine;
using TMPro;

public class ItemPickupObject : MonoBehaviour
{
    [Header("아이템 정보")]
    private Item item;                      // 아이템 정보
    private int quantity = 1;               // 아이템 수량
    private bool isBossItem = false;        // 보스 아이템 여부

    [Header("시각적 효과")]
    [SerializeField] private ParticleSystem itemParticle;    // 아이템 파티클 효과
    [SerializeField] private TextMeshProUGUI quantityText;   // 수량 텍스트
    [SerializeField] private Light pointLight;               // 포인트 라이트

    [Header("희귀도 설정")]
    [SerializeField]
    private Color[] rarityColors = new Color[] {
        new Color(0.7f, 0.7f, 0.7f), // Common - 회색
        new Color(0.3f, 0.7f, 0.3f), // Uncommon - 녹색
        new Color(0.3f, 0.3f, 0.9f), // Rare - 파란색
        new Color(0.7f, 0.3f, 0.9f), // Epic - 보라색
        new Color(1.0f, 0.8f, 0.0f)  // Legendary - 금색
    };
    [SerializeField] private GameObject[] raritySpawnEffects;   // 등급별 등장 이펙트
    [SerializeField] private GameObject[] rarityPickupEffects;  // 등급별 획득 이펙트

    [Header("움직임 설정")]
    [SerializeField] private float floatHeight = 0.2f;    // 둥둥 떠다니는 높이
    [SerializeField] private float floatSpeed = 1.0f;     // 둥둥 떠다니는 속도
    [SerializeField] private float rotateSpeed = 30f;     // 회전 속도

    [Header("자동 획득 설정")]
    [SerializeField] private bool enableAutoPickup = true;    // 자동 획득 활성화
    [SerializeField] private float autoPickupDelay = 1.5f;    // 자동 획득 딜레이
    [SerializeField] private float attractionRadius = 3.0f;   // 플레이어 감지 반경
    [SerializeField] private float attractionSpeed = 5.0f;    // 끌려오는 속도
    [SerializeField] private AnimationCurve attractionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 끌려오는 속도 커브

    [Header("효과 설정")]
    [SerializeField] private AudioClip pickupSound;     // 획득 사운드
    [SerializeField] private AudioClip spawnSound;      // 생성 사운드

    [Header("상호작용 설정")]
    [SerializeField] private LayerMask playerLayer;        // 플레이어 레이어
    [SerializeField] private float interactionRadius = 1.5f; // 상호작용 반경

    // 컴포넌트 참조
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
            audioSource.spatialBlend = 1.0f; // 3D 사운드
            audioSource.minDistance = 1.0f;
            audioSource.maxDistance = 20.0f;
        }

        // 포인트 라이트가 지정되지 않았다면 찾기
        if (pointLight == null)
        {
            pointLight = GetComponentInChildren<Light>();
        }
    }

    // 아이템 정보 초기화
    public void Initialize(Item itemData, int itemQuantity, bool bossItem = false)
    {
        this.item = itemData;
        this.quantity = itemQuantity;
        this.isBossItem = bossItem;

        // 파티클 색상 설정
        if (itemParticle != null)
        {
            // 등급에 따른 색상 설정
            Color particleColor = GetRarityColor(item.rarity);

            // 파티클 시스템 메인 모듈 색상 변경
            ParticleSystem.MainModule main = itemParticle.main;
            main.startColor = particleColor;

            // 보스 아이템이면 파티클 크기 증가
            if (bossItem)
            {
                main.startSize = main.startSize.constant * 1.5f;
            }
        }

        // 포인트 라이트 설정
        if (pointLight != null)
        {
            // 희귀도에 따라 라이트 활성화 (일반 등급은 라이트 없음)
            pointLight.enabled = item.rarity > Item.ItemRarity.Common;

            if (pointLight.enabled)
            {
                // 색상 적용
                pointLight.color = GetRarityColor(item.rarity);

                // 희귀도에 따라 세기 조정
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

                // 보스 아이템이면 라이트 강화
                if (bossItem)
                {
                    pointLight.intensity *= 1.5f;
                    pointLight.range *= 1.3f;
                }
            }
        }

        // 수량 텍스트 설정
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

        // 보스 아이템이면 크기 증가
        if (bossItem)
        {
            transform.localScale *= 1.2f;
        }

        // 시작 위치 저장
        startPosition = transform.position;

        // 시작 이펙트 및 사운드 재생
        PlaySpawnEffects();

        // 자동 획득 설정
        if (enableAutoPickup)
        {
            StartCoroutine(EnableAutoPickupAfterDelay());
        }

        // 게임 오브젝트 이름 설정
        gameObject.name = $"Item_{item.itemName}_{item.rarity}";
    }

    // 등급별 색상 반환
    private Color GetRarityColor(Item.ItemRarity rarity)
    {
        int index = Mathf.Min((int)rarity, rarityColors.Length - 1);
        return rarityColors[index];
    }

    // 시작 이펙트 및 사운드 재생
    private void PlaySpawnEffects()
    {
        // 등급에 맞는 이펙트 선택
        if (raritySpawnEffects != null && raritySpawnEffects.Length > 0)
        {
            int index = Mathf.Min((int)item.rarity, raritySpawnEffects.Length - 1);

            if (raritySpawnEffects[index] != null)
            {
                // 선택한 등급의 이펙트 생성
                Instantiate(raritySpawnEffects[index], transform.position, Quaternion.identity);
            }
        }

        // 사운드 재생
        if (spawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }

    // 자동 획득 딜레이 후 활성화
    private IEnumerator EnableAutoPickupAfterDelay()
    {
        // 물리 시뮬레이션이 안정화되도록 일정 시간 대기
        yield return new WaitForSeconds(autoPickupDelay);

        // 플레이어 찾기
        playerTransform = FindPlayerTransform();

        if (playerTransform != null)
        {
            // 플레이어 감지 및 인력 작용
            StartCoroutine(DetectPlayerProximity());
        }
    }

    // 플레이어 트랜스폼 찾기
    private Transform FindPlayerTransform()
    {
        // 플레이어 태그로 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player?.transform;
    }

    // 플레이어 근접 감지
    private IEnumerator DetectPlayerProximity()
    {
        while (!isPickedUp && playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // 상호작용 범위 내에 있으면 획득
            if (distance <= interactionRadius)
            {
                PickupItem();
                yield break;
            }
            // 끌려오는 범위 내에 있으면 플레이어에게 이동
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

    // 플레이어 방향으로 이동
    private IEnumerator MoveTowardsPlayer()
    {
        // 물리 시뮬레이션 비활성화
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        float startTime = Time.time;

        while (!isPickedUp && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // 상호작용 범위에 도달하면 획득
            if (distanceToPlayer <= interactionRadius)
            {
                PickupItem();
                yield break;
            }

            // 경과 시간에 따라 속도 조절 (커브 적용)
            float elapsedTime = Time.time - startTime;
            float t = Mathf.Clamp01(elapsedTime / 1.0f); // 1초동안 가속
            float speedMultiplier = attractionCurve.Evaluate(t);

            // 플레이어 쪽으로 이동
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * attractionSpeed * speedMultiplier * Time.deltaTime;

            yield return null;
        }
    }

    private void Update()
    {
        if (!isPickedUp)
        {
            // 둥둥 떠다니는 효과
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 회전 효과
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        }
    }

    // 트리거 충돌 감지 (플레이어가 직접 아이템에 닿았을 때)
    private void OnTriggerEnter(Collider other)
    {
        if (!isPickedUp && IsPlayerCollider(other))
        {
            PickupItem();
        }
    }

    // 플레이어 콜라이더인지 확인
    private bool IsPlayerCollider(Collider other)
    {
        return ((1 << other.gameObject.layer) & playerLayer) != 0 || other.CompareTag("Player");
    }

    // 아이템 획득 처리
    private void PickupItem()
    {
        if (isPickedUp) return;

        isPickedUp = true;

        // 인벤토리에 아이템 추가
        if (InventorySystem.Instance != null)
        {
            bool added = InventorySystem.Instance.AddItem(item, quantity);

            if (added)
            {
                // 획득 이펙트 및 사운드 재생
                PlayPickupEffects();

                // 오브젝트 제거
                StartCoroutine(DestroyAfterEffects());
            }
            else
            {
                Debug.LogWarning("인벤토리가 가득 차서 아이템을 획득할 수 없습니다.");
                isPickedUp = false; // 다시 획득 가능하도록 설정
            }
        }
        else
        {
            Debug.LogError("InventorySystem 인스턴스를 찾을 수 없습니다.");
            isPickedUp = false;
        }
    }

    // 획득 이펙트 및 사운드 재생
    private void PlayPickupEffects()
    {
        // 등급에 맞는 이펙트 선택
        if (rarityPickupEffects != null && rarityPickupEffects.Length > 0)
        {
            int index = Mathf.Min((int)item.rarity, rarityPickupEffects.Length - 1);

            if (rarityPickupEffects[index] != null)
            {
                // 선택한 등급의 이펙트 생성
                Instantiate(rarityPickupEffects[index], transform.position, Quaternion.identity);
            }
        }

        // 사운드 재생
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    // 이펙트 재생 후 오브젝트 제거
    private IEnumerator DestroyAfterEffects()
    {
        // 콜라이더 비활성화
        if (col != null) col.enabled = false;

        // 렌더러 비활성화
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        // 라이트 비활성화
        if (pointLight != null)
        {
            pointLight.enabled = false;
        }

        // 파티클 시스템 정지
        if (itemParticle != null)
        {
            itemParticle.Stop(true);
        }

        // 이펙트 및 사운드 재생 대기
        float delay = pickupSound != null ? pickupSound.length : 0.5f;
        yield return new WaitForSeconds(delay);

        // 게임 오브젝트 제거
        Destroy(gameObject);
    }

    // 편집기에서 기즈모 표시 (디버깅용)
    private void OnDrawGizmosSelected()
    {
        // 상호작용 범위 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);

        // 자동 획득 감지 범위 표시
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}