using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDissolveController : MonoBehaviour
{
    public Material dissolveMaterial; // �ν����Ϳ��� ���� (���� ���͸���)
    public float dissolveTime = 2.0f;
    public bool destroyAfterDissolve = false;

    private Renderer[] renderers;
    private Material[] originalMaterials;
    private Material[] dissolveMatInstances; // �� �������� ���� ���� ���͸��� �ν��Ͻ�

    void Start()
    {
        InitializeRenderers();
    }

    // �������� ���͸��� �ʱ�ȭ
    private void InitializeRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>();

        // ���� ���׸��� ���� �� dissolve ���͸��� �ν��Ͻ� ����
        int totalMaterials = 0;
        foreach (Renderer renderer in renderers)
        {
            totalMaterials += renderer.materials.Length;
            Debug.Log($"{renderer.name} ������ �ʱ�ȭ");
        }

        originalMaterials = new Material[totalMaterials];
        dissolveMatInstances = new Material[totalMaterials]; // ���� �ν��Ͻ� �迭

        int index = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] mats = renderer.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                originalMaterials[index] = mats[i];
                // �� ���͸��󸶴� ������ ������ ���͸��� �ν��Ͻ� ����
                dissolveMatInstances[index] = new Material(dissolveMaterial);
                index++;
            }
        }
    }

    public void OnMonsterDeath()
    {
        // �������� ���ų� �ʱ�ȭ���� �ʾ��� ��� �ٽ� �ʱ�ȭ
        if (renderers == null || renderers.Length == 0)
        {
            Debug.Log("������ �ʱ�ȭ ����, �ٽ� �ʱ�ȭ�մϴ�");
            InitializeRenderers();
        }

        // ��� �������� ���׸����� ������ ���׸���� ��ü
        int materialIndex = 0;
        foreach (Renderer renderer in renderers)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];

            for (int i = 0; i < newMaterials.Length; i++)
            {
                // �� �������� ������ ���͸��� �ν��Ͻ� �Ҵ�
                newMaterials[i] = dissolveMatInstances[materialIndex];
                // �ʱ� ������ �� ����
                newMaterials[i].SetFloat("_DissolveAmount", 0);
                materialIndex++;
            }

            renderer.materials = newMaterials;
        }

        // ������ ȿ�� ����
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float elapsedTime = 0;

        while (elapsedTime < dissolveTime)
        {
            // ������ ���� 0���� 1�� ������ ����
            float dissolveAmount = Mathf.Clamp01(elapsedTime / dissolveTime);

            // ��� ������ ���͸��� �ν��Ͻ� ������Ʈ
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
        // ������ �Ϸ� �� ���� ������Ʈ ����
        Destroy(gameObject);
    }

    public void RefreshRenderers()
    {
        Debug.Log("������ ���� ȣ���");

        // ���͸��� �ν��Ͻ� ���� (�޸� ���� ����)
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

        // �ʱ�ȭ �ٽ� ����
        InitializeRenderers();
    }

    private void OnDestroy()
    {
        // ���͸��� �ν��Ͻ� ����
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