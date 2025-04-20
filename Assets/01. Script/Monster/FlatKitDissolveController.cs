using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlatKit 스타일 셰이더용 디졸브 컨트롤러
/// </summary>
public class FlatKitDissolveController : MonoBehaviour
{
    [Header("디졸브 설정")]
    [Tooltip("디졸브 효과의 지속 시간")]
    public float dissolveTime = 2.0f;

    [Tooltip("디졸브 효과 시작 지연 시간")]
    public float startDelay = 0.0f;

    [Tooltip("디졸브가 완료된 후 오브젝트 제거 여부")]
    public bool destroyAfterDissolve = true;

    // 렌더러 컴포넌트
    private Renderer[] renderers;

    // 머티리얼 배열
    private Material[] materials;

    // 디졸브가 활성화되었는지 여부
    private bool isDissolving = false;

    // 초기화
    void Awake()
    {
        // 이 게임 오브젝트에 포함된 모든 렌더러를 가져옴
        renderers = GetComponentsInChildren<Renderer>();

        // 모든 렌더러의 모든 머티리얼을 저장할 리스트
        List<Material> materialsList = new List<Material>();

        // 각 렌더러에서 머티리얼 수집
        foreach (Renderer renderer in renderers)
        {
            // 공유 머티리얼을 인스턴스로 변환하여 다른 오브젝트에 영향을 주지 않도록 함
            Material[] instanceMaterials = renderer.materials;

            for (int i = 0; i < instanceMaterials.Length; i++)
            {
                materialsList.Add(instanceMaterials[i]);
            }

            // 인스턴스화된 머티리얼을 렌더러에 다시 할당
            renderer.materials = instanceMaterials;
        }

        // 리스트를 배열로 변환
        materials = materialsList.ToArray();

        // 초기 디졸브 활성화 상태 설정
        SetDissolveEnabled(false);
    }

    // 디졸브 활성화 상태 설정
    private void SetDissolveEnabled(bool enabled)
    {
        foreach (Material mat in materials)
        {
            // 셰이더 키워드 설정
            if (enabled)
            {
                mat.EnableKeyword("DR_DISSOLVE_ON");
            }
            else
            {
                mat.DisableKeyword("DR_DISSOLVE_ON");
            }

            // 셰이더 속성 설정 (셰이더에서 Toggle 프로퍼티를 사용하는 경우)
            if (mat.HasProperty("_DissolveEnabled"))
            {
                mat.SetInt("_DissolveEnabled", enabled ? 1 : 0);
            }
        }
    }

    // 디졸브 값을 초기화
    public void ResetDissolve()
    {
        foreach (Material mat in materials)
        {
            if (mat.HasProperty("_DissolveAmount"))
            {
                mat.SetFloat("_DissolveAmount", 0);
            }
        }

        isDissolving = false;
        SetDissolveEnabled(false);
    }

    // 디졸브 효과 시작
    public void StartDissolve()
    {
        if (!isDissolving)
        {
            isDissolving = true;
            SetDissolveEnabled(true);
            StartCoroutine(DissolveCoroutine());
        }
    }

    // 몬스터가 죽을 때 호출하는 함수
    public void OnMonsterDeath()
    {
        StartDissolve();
    }

    // 디졸브 효과를 처리하는 코루틴
    private IEnumerator DissolveCoroutine()
    {
        // 시작 지연 시간
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float elapsedTime = 0;

        while (elapsedTime < dissolveTime)
        {
            // 현재 디졸브 값 계산 (0에서 1로)
            float dissolveValue = Mathf.Clamp01(elapsedTime / dissolveTime);

            // 모든 머티리얼의 디졸브 값 설정
            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_DissolveAmount"))
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                }
            }

            // 시간 업데이트
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 디졸브 완료 후 오브젝트 처리
        if (destroyAfterDissolve)
        {
            Destroy(gameObject);
        }
        else
        {
            // 디졸브는 완료했지만 오브젝트는 유지
            SetDissolveEnabled(false);
        }
    }
}