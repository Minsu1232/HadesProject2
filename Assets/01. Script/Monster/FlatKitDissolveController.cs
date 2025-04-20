using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// FlatKit ��Ÿ�� ���̴��� ������ ��Ʈ�ѷ�
/// </summary>
public class FlatKitDissolveController : MonoBehaviour
{
    [Header("������ ����")]
    [Tooltip("������ ȿ���� ���� �ð�")]
    public float dissolveTime = 2.0f;

    [Tooltip("������ ȿ�� ���� ���� �ð�")]
    public float startDelay = 0.0f;

    [Tooltip("�����갡 �Ϸ�� �� ������Ʈ ���� ����")]
    public bool destroyAfterDissolve = true;

    // ������ ������Ʈ
    private Renderer[] renderers;

    // ��Ƽ���� �迭
    private Material[] materials;

    // �����갡 Ȱ��ȭ�Ǿ����� ����
    private bool isDissolving = false;

    // �ʱ�ȭ
    void Awake()
    {
        // �� ���� ������Ʈ�� ���Ե� ��� �������� ������
        renderers = GetComponentsInChildren<Renderer>();

        // ��� �������� ��� ��Ƽ������ ������ ����Ʈ
        List<Material> materialsList = new List<Material>();

        // �� ���������� ��Ƽ���� ����
        foreach (Renderer renderer in renderers)
        {
            // ���� ��Ƽ������ �ν��Ͻ��� ��ȯ�Ͽ� �ٸ� ������Ʈ�� ������ ���� �ʵ��� ��
            Material[] instanceMaterials = renderer.materials;

            for (int i = 0; i < instanceMaterials.Length; i++)
            {
                materialsList.Add(instanceMaterials[i]);
            }

            // �ν��Ͻ�ȭ�� ��Ƽ������ �������� �ٽ� �Ҵ�
            renderer.materials = instanceMaterials;
        }

        // ����Ʈ�� �迭�� ��ȯ
        materials = materialsList.ToArray();

        // �ʱ� ������ Ȱ��ȭ ���� ����
        SetDissolveEnabled(false);
    }

    // ������ Ȱ��ȭ ���� ����
    private void SetDissolveEnabled(bool enabled)
    {
        foreach (Material mat in materials)
        {
            // ���̴� Ű���� ����
            if (enabled)
            {
                mat.EnableKeyword("DR_DISSOLVE_ON");
            }
            else
            {
                mat.DisableKeyword("DR_DISSOLVE_ON");
            }

            // ���̴� �Ӽ� ���� (���̴����� Toggle ������Ƽ�� ����ϴ� ���)
            if (mat.HasProperty("_DissolveEnabled"))
            {
                mat.SetInt("_DissolveEnabled", enabled ? 1 : 0);
            }
        }
    }

    // ������ ���� �ʱ�ȭ
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

    // ������ ȿ�� ����
    public void StartDissolve()
    {
        if (!isDissolving)
        {
            isDissolving = true;
            SetDissolveEnabled(true);
            StartCoroutine(DissolveCoroutine());
        }
    }

    // ���Ͱ� ���� �� ȣ���ϴ� �Լ�
    public void OnMonsterDeath()
    {
        StartDissolve();
    }

    // ������ ȿ���� ó���ϴ� �ڷ�ƾ
    private IEnumerator DissolveCoroutine()
    {
        // ���� ���� �ð�
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float elapsedTime = 0;

        while (elapsedTime < dissolveTime)
        {
            // ���� ������ �� ��� (0���� 1��)
            float dissolveValue = Mathf.Clamp01(elapsedTime / dissolveTime);

            // ��� ��Ƽ������ ������ �� ����
            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_DissolveAmount"))
                {
                    mat.SetFloat("_DissolveAmount", dissolveValue);
                }
            }

            // �ð� ������Ʈ
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // ������ �Ϸ� �� ������Ʈ ó��
        if (destroyAfterDissolve)
        {
            Destroy(gameObject);
        }
        else
        {
            // ������� �Ϸ������� ������Ʈ�� ����
            SetDissolveEnabled(false);
        }
    }
}