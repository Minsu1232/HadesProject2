using GSpawn_Pro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDissolveController : MonoBehaviour
{
    public Material dissolveMaterial; // 인스펙터에서 지정
    public float dissolveTime = 2.0f;
    public bool destroyAfterDissolve = false;
    private Renderer[] renderers;
    private Material[] originalMaterials;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // 원본 메테리얼 저장
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            totalMaterials += renderer.materials.Length;
            Debug.Log($"{renderer.name}이다"); 
        }

        originalMaterials = new Material[totalMaterials];

        int index = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                originalMaterials[index] = mats[i];
                index++;
            }
        }
    }

    public void OnMonsterDeath()
    {
        // 모든 렌더러의 메테리얼을 디졸브 메테리얼로 교체
        foreach (Renderer renderer in renderers)
        {
            
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = dissolveMaterial;
            }
            renderer.materials = materials;
        }

        // 디졸브 효과 시작
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float elapsedTime = 0;

        // 디졸브 초기값 설정
        dissolveMaterial.SetFloat("_DissolveAmount", 0);

        while (elapsedTime < dissolveTime)
        {
            // 디졸브 값을 0에서 1로 서서히 증가
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveTime);
            dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        destroyAfterDissolve = true;
        // 디졸브 완료 후 게임 오브젝트 제거
        Destroy(gameObject);
    }

    public void RefreshRenderers()
    {
        Debug.Log("호출");
        // 모든 자식 렌더러를 다시 검색 (비활성화된 오브젝트도 포함)
        renderers = GetComponentsInChildren<Renderer>(true);
        
        // 원본 메테리얼 저장 다시 수행
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            Debug.Log("호출2");
            totalMaterials += renderer.materials.Length;
        }

        originalMaterials = new Material[totalMaterials];

        int index = 0;
        foreach (Renderer renderer in renderers)
        {

            Debug.Log("호출3");
            Debug.Log(renderer.name);
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                originalMaterials[index] = mats[i];
                index++;
            }
        }
    }
}