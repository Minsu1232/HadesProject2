// DashInvincibleComponent.cs - 대시 중 무적 기능 컴포넌트
using UnityEngine;
using DG.Tweening;  // DOTween 사용을 위해 추가

public class DashInvincibleComponent : MonoBehaviour
{
    private float invincibleDuration = 0f;    // 대시 중 무적 지속 시간
    private PlayerClass playerClass;
    private PlayerDashComponent dashComponent;
    private bool isInvincible = false;

    // 무적 효과 시각화를 위한 변수
    [SerializeField] private Color invincibleColor = new Color(1f, 1f, 1f, 0.5f);
    private Renderer[] renderers;
    private Color[] originalColors;

    private void Awake()
    {
        playerClass = GameInitializer.Instance.GetPlayerClass();
        dashComponent = GetComponent<PlayerDashComponent>();

        if (playerClass == null || dashComponent == null)
        {
            Debug.LogError("DashInvincibleComponent: 필요한 컴포넌트가 없습니다.");
            enabled = false;
            return;
        }

        // 렌더러 캐싱
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material.HasProperty("_Color"))
            {
                originalColors[i] = renderers[i].material.color;
            }
        }

        // 대시 이벤트 구독
        dashComponent.OnDashStart.AddListener(OnDashStart);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (dashComponent != null)
        {
            dashComponent.OnDashStart.RemoveListener(OnDashStart);
        }

        // 무적 상태 종료 (안전을 위해)
        if (isInvincible)
        {
            EndInvincibility();
        }
    }

    // 대시 시작 시 호출되는 이벤트 핸들러
    private void OnDashStart()
    {
        if (invincibleDuration > 0f)
        {
            StartInvincibility();
        }
    }

    // 무적 상태 시작
    private void StartInvincibility()
    {
        if (!isInvincible)
        {
            isInvincible = true;
            playerClass.isInvicible = true;

            // 시각적 피드백 (DOTween 사용)
            ApplyInvincibilityVisuals(true);

            // 무적 지속 시간 후 원래 상태로 복귀
            DOVirtual.DelayedCall(invincibleDuration, EndInvincibility);

            Debug.Log($"대시 무적 시작: {invincibleDuration}초");
        }
    }

    // 무적 상태 종료
    private void EndInvincibility()
    {
        if (isInvincible)
        {
            isInvincible = false;
            playerClass.isInvicible = false;

            // 시각적 피드백 제거
            ApplyInvincibilityVisuals(false);

            Debug.Log("대시 무적 종료");
        }
    }

    // 무적 시 시각적 피드백 적용/제거
    private void ApplyInvincibilityVisuals(bool active)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            if (renderers[i].material.HasProperty("_Color"))
            {
                if (active)
                {
                    // 무적 시작: 투명도 조절 (DOTween 사용)
                    renderers[i].material.DOColor(invincibleColor, 0.2f);
                }
                else
                {
                    // 무적 종료: 원래 색상으로 복원
                    renderers[i].material.DOColor(originalColors[i], 0.2f);
                }
            }
        }
    }

    // 무적 지속시간 설정
    public void SetInvincibleDuration(float duration)
    {
        invincibleDuration = Mathf.Max(0f, duration);
        Debug.Log($"대시 무적 지속시간 설정: {invincibleDuration}초");
    }

    // 현재 무적 지속시간 반환
    public float GetInvincibleDuration()
    {
        return invincibleDuration;
    }

    // 현재 무적 상태 반환
    public bool IsInvincible()
    {
        return isInvincible;
    }
}