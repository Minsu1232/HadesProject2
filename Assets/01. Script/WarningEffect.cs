using UnityEngine;
using DG.Tweening;

public class WarningEffect : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;  // Inspector�� SpriteRenderer �Ҵ�
    public float warningDuration = 1.0f;     // ��� ȿ�� ���� �ð�
    private MaterialPropertyBlock propertyBlock;
    private Material warningMat;

    void Awake()
    {
        // ��������Ʈ ������ �ڵ� �Ҵ� (���ٸ� ���� GameObject���� ã��)
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // MaterialPropertyBlock ���� �� ����
        propertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(propertyBlock);

        // ���� �ν��Ͻ� ���� (���� ������ ������ ���� �ʵ���)
        warningMat = Instantiate(spriteRenderer.sharedMaterial);
        spriteRenderer.material = warningMat;

        // �ʱ� _FillAmount ���� 0���� ����
        propertyBlock.SetFloat("_FillAmount", 0f);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    void Start()
    {
        // DOTween�� ����� _FillAmount�� 0���� 1�� �ִϸ��̼� ó��
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
