using GSpawn_Pro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDissolveController : MonoBehaviour
{
    public Material dissolveMaterial; // �ν����Ϳ��� ����
    public float dissolveTime = 2.0f;
    public bool destroyAfterDissolve = false;
    private Renderer[] renderers;
    private Material[] originalMaterials;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // ���� ���׸��� ����
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            totalMaterials += renderer.materials.Length;
            Debug.Log($"{renderer.name}�̴�"); 
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
        // ��� �������� ���׸����� ������ ���׸���� ��ü
        foreach (Renderer renderer in renderers)
        {
            
            Material[] materials = new Material[renderer.materials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = dissolveMaterial;
            }
            renderer.materials = materials;
        }

        // ������ ȿ�� ����
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float elapsedTime = 0;

        // ������ �ʱⰪ ����
        dissolveMaterial.SetFloat("_DissolveAmount", 0);

        while (elapsedTime < dissolveTime)
        {
            // ������ ���� 0���� 1�� ������ ����
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveTime);
            dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        destroyAfterDissolve = true;
        // ������ �Ϸ� �� ���� ������Ʈ ����
        Destroy(gameObject);
    }

    public void RefreshRenderers()
    {
        Debug.Log("ȣ��");
        // ��� �ڽ� �������� �ٽ� �˻� (��Ȱ��ȭ�� ������Ʈ�� ����)
        renderers = GetComponentsInChildren<Renderer>(true);
        
        // ���� ���׸��� ���� �ٽ� ����
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            Debug.Log("ȣ��2");
            totalMaterials += renderer.materials.Length;
        }

        originalMaterials = new Material[totalMaterials];

        int index = 0;
        foreach (Renderer renderer in renderers)
        {

            Debug.Log("ȣ��3");
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