using UnityEngine;
using DG.Tweening;

public class WarningEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;  // Inspector에 SpriteRenderer 할당
    public float warningDuration = 1.0f;     // 경고 효과 지속 시간
    private MaterialPropertyBlock propertyBlock;
    private Material warningMat;

    void Awake()
    {
        // 스프라이트 렌더러 자동 할당 (없다면 현재 GameObject에서 찾기)
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // MaterialPropertyBlock 생성 및 적용
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(propertyBlock);

        // 재질 인스턴스 생성 (공유 재질에 영향을 주지 않도록)
        warningMat = Instantiate(spriteRenderer.sharedMaterial);
        spriteRenderer.material = warningMat;

        // 초기 _FillAmount 값을 0으로 설정
        propertyBlock.SetFloat("_FillAmount", 0f);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    void Start()
    {
        // DOTween을 사용해 _FillAmount를 0에서 1로 애니메이션 처리
        DOTween.To(
            () => propertyBlock.GetFloat("_FillAmount"),
            x => {
                propertyBlock.SetFloat("_FillAmount", x);
                spriteRenderer.SetPropertyBlock(propertyBlock);
            },
            1f, warningDuration
        );
    }
}
