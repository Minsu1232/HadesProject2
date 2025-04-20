using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class WeaponDataManager : Singleton<WeaponDataManager>
{
    public List<WeaponScriptableObject> weaponScriptableObjects; // ScriptableObject ����Ʈ
    private string persistentFilePath; // ��� �ʱ�ȭ�� Awake���� ����
    private string streamingFilePath;  // StreamingAssets ���

    protected override void Awake()
    {
        base.Awake();
        // ��� �ʱ�ȭ
        persistentFilePath = Path.Combine(Application.persistentDataPath, "Weapons.csv");
        streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Weapons.csv");

        // CSV ���� ���� ���� Ȯ��
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogWarning($"CSV ������ �����ϴ�. StreamingAssets���� �����մϴ�: {persistentFilePath}");
            CopyCSVFromStreamingAssets();
        }

        // CSV �ε�
        LoadWeaponDataFromCSV();
    }

 
    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            // StreamingAssets�� ������ persistentDataPath�� ����
            File.Copy(streamingFilePath, persistentFilePath);
            Debug.Log($"StreamingAssets���� CSV ���� ���� �Ϸ�: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets���� Weapons.csv ������ ã�� �� �����ϴ�.");
        }
    }

    private void LoadWeaponDataFromCSV()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"CSV ������ ã�� �� �����ϴ�: {persistentFilePath}");
            return;
        }

        string[] csvLines = File.ReadAllLines(persistentFilePath);

        for (int i = 1; i < csvLines.Length; i++) // ù ���� ���
        {
            string[] values = csvLines[i].Split(',');

            string weaponName = values[0];
            int baseDamage = int.Parse(values[1]);
            int baseGagePerHit = int.Parse(values[2]);
            float maxChargeTime  = float.Parse(values[3]);
            float chargeMultiplier = float.Parse(values[4]);
            Vector3 defaultPosition = ParseVector3(values[5]);
            Vector3 defaultRotation = ParseVector3(values[6]);
            int damageUpgradeCount = int.Parse(values[7]);
            int gageUpgradeCount = int.Parse(values[8]);
            int additionalDamage = int.Parse(values[9]);
            int additionalGagePerHit = int.Parse(values[10]);
            Debug.Log($"�����̸� {weaponName}");
            WeaponScriptableObject weaponSO = weaponScriptableObjects.Find(w => w.weaponName == weaponName);
            if (weaponSO != null)
            {
                weaponSO.weaponName = weaponName;
                weaponSO.baseDamage = baseDamage;
                weaponSO.baseGagePerHit = baseGagePerHit;
                weaponSO.defaultPosition = defaultPosition;
                weaponSO.defaultRotation = defaultRotation;
                weaponSO.maxChargeTime = maxChargeTime;
                weaponSO.chargeMultiplier = chargeMultiplier;
                weaponSO.damageUpgradeCount = damageUpgradeCount;
                weaponSO.gageUpgradeCount = gageUpgradeCount;
                weaponSO.additionalDamage = additionalDamage;
                weaponSO.additionalGagePerHit = additionalGagePerHit;

                Debug.Log($"CSV ������ �ε� �Ϸ�: {weaponName} ���ݷ�{baseDamage}");
            }
            else
            {
                Debug.LogWarning($"CSV �����Ϳ� ��Ī�Ǵ� ScriptableObject�� ã�� �� �����ϴ�: {weaponName}");
            }
        }
    }
    public void SaveWeaponDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // ��� �ۼ�
            writer.WriteLine("weaponName,baseDamage,baseGagePerHit,maxChargeTime,chargeMultiplier,defaultPosition,defaultRotation,damageUpgradeCount,gageUpgradeCount,additionalDamage,additionalGagePerHit");

            // ScriptableObject �����͸� CSV�� ����
            foreach (var weaponSO in weaponScriptableObjects)
            {
                string line = $"{weaponSO.weaponName}," +
                              $"{weaponSO.baseDamage}," +
                              $"{weaponSO.baseGagePerHit}," +
                              $"{weaponSO.maxChargeTime}," +
                              $"{weaponSO.chargeMultiplier}," +
                              $"{FormatVector3(weaponSO.defaultPosition)}," +
                              $"{FormatVector3(weaponSO.defaultRotation)}," +
                              $"{weaponSO.damageUpgradeCount}," +
                              $"{weaponSO.gageUpgradeCount}," +
                              $"{weaponSO.additionalDamage}," +
                              $"{weaponSO.additionalGagePerHit}";

                writer.WriteLine(line);
            }

            Debug.Log("CSV ������ ���� �Ϸ�");
        }
    }

    private Vector3 ParseVector3(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Debug.LogWarning("ParseVector3: �Է� ���� ��� �ֽ��ϴ�. �⺻�� Vector3.zero ��ȯ");
            return Vector3.zero;
        }

        // ��ǥ�� �������� ���ڿ� ������
        string[] parts = value.Split('/');

        // �迭 ���� Ȯ��
        if (parts.Length != 3)
        {
            Debug.LogError($"ParseVector3: ���� ������ �߸��Ǿ����ϴ�. �Է°�: '{value}', �迭 ����: {parts.Length}. �⺻�� Vector3.zero ��ȯ");
            return Vector3.zero;
        }

        try
        {
            // ������ ���� �Ľ�
            float x = float.Parse(parts[0].Trim()); // ���� ���� �� �Ľ�
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());
            return new Vector3(x, y, z);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ParseVector3: �� ��ȯ �� ���� �߻�. �Է°�: '{value}' - {ex.Message}. �⺻�� Vector3.zero ��ȯ");
            return Vector3.zero;
        }
    }

    private string FormatVector3(Vector3 vector)
    {
        // ��ǥ ��� ������(`/`)�� �����ڷ� ���
        return $"{vector.x}/{vector.y}/{vector.z}";
    }
}
