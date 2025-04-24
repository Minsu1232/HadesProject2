using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDissolveController : MonoBehaviour
{
    public Material dissolveMaterial; // 인스펙터에서 지정 (원본 메터리얼)
    public float dissolveTime = 2.0f;
    public bool destroyAfterDissolve = false;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] dissolveMatInstances; // 각 렌더러에 대한 고유 메터리얼 인스턴스

    void Start()
    {
        InitializeRenderers();
    }

    // 렌더러와 메터리얼 초기화
    private void InitializeRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // 원본 메테리얼 저장 및 dissolve 메터리얼 인스턴스 생성
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            totalMaterials += renderer.materials.Length;
            Debug.Log($"{renderer.name} 렌더러 초기화");
        }

        originalMaterials = new Material[totalMaterials];
        dissolveMatInstances = new Material[totalMaterials]; // 고유 인스턴스 배열

        int index = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                originalMaterials[index] = mats[i];
                // 각 메터리얼마다 고유한 디졸브 메터리얼 인스턴스 생성
                dissolveMatInstances[index] = new Material(dissolveMaterial);
                index++;
            }
        }
    }

    public void OnMonsterDeath()
    {
        // 렌더러가 없거나 초기화되지 않았을 경우 다시 초기화
        if (renderers == null || renderers.Length == 0)
        {
            Debug.Log("렌더러 초기화 누락, 다시 초기화합니다");
            InitializeRenderers();
        }

        // 모든 렌더러의 메테리얼을 디졸브 메테리얼로 교체
        int materialIndex = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                // 각 렌더러에 고유한 메터리얼 인스턴스 할당
                newMaterials[i] = dissolveMatInstances[materialIndex];
                // 초기 디졸브 값 설정
                newMaterials[i].SetFloat("_DissolveAmount", 0);
                materialIndex++;
            }

            renderer.materials = newMaterials;
        }

        // 디졸브 효과 시작
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float elapsedTime = 0;

        while (elapsedTime < dissolveTime)
        {
            // 디졸브 값을 0에서 1로 서서히 증가
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveTime);

            // 모든 디졸브 메터리얼 인스턴스 업데이트
            foreach (Material mat in dissolveMatInstances)
            {
                if (mat != null)
                {
                    mat.SetFloat("_DissolveAmount", dissolveAmount);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        destroyAfterDissolve = true;
        // 디졸브 완료 후 게임 오브젝트 제거
        Destroy(gameObject);
    }

    public void RefreshRenderers()
    {
        Debug.Log("렌더러 갱신 호출됨");

        // 메터리얼 인스턴스 해제 (메모리 누수 방지)
        if (dissolveMatInstances != null)
        {
            foreach (Material mat in dissolveMatInstances)
            {
                if (mat != null && Application.isPlaying)
                {
                    Destroy(mat);
                }
            }
        }

        // 초기화 다시 수행
        InitializeRenderers();
    }

    private void OnDestroy()
    {
        // 메터리얼 인스턴스 정리
        if (dissolveMatInstances != null)
        {
            foreach (Material mat in dissolveMatInstances)
            {
                if (mat != null && Application.isPlaying)
                {
                    Destroy(mat);
                }
            }
        }
    }
}