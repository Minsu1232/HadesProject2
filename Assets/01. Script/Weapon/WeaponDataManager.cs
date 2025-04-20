using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class WeaponDataManager : Singleton<WeaponDataManager>
{
    public List<WeaponScriptableObject> weaponScriptableObjects; // ScriptableObject 리스트
    private string persistentFilePath; // 경로 초기화는 Awake에서 수행
    private string streamingFilePath;  // StreamingAssets 경로

    protected override void Awake()
    {
        base.Awake();
        // 경로 초기화
        persistentFilePath = Path.Combine(Application.persistentDataPath, "Weapons.csv");
        streamingFilePath = Path.Combine(Application.streamingAssetsPath, "Weapons.csv");

        // CSV 파일 존재 여부 확인
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogWarning($"CSV 파일이 없습니다. StreamingAssets에서 복사합니다: {persistentFilePath}");
            CopyCSVFromStreamingAssets();
        }

        // CSV 로드
        LoadWeaponDataFromCSV();
    }

 
    private void CopyCSVFromStreamingAssets()
    {
        if (File.Exists(streamingFilePath))
        {
            // StreamingAssets의 파일을 persistentDataPath로 복사
            File.Copy(streamingFilePath, persistentFilePath);
            Debug.Log($"StreamingAssets에서 CSV 파일 복사 완료: {persistentFilePath}");
        }
        else
        {
            Debug.LogError("StreamingAssets에서 Weapons.csv 파일을 찾을 수 없습니다.");
        }
    }

    private void LoadWeaponDataFromCSV()
    {
        if (!File.Exists(persistentFilePath))
        {
            Debug.LogError($"CSV 파일을 찾을 수 없습니다: {persistentFilePath}");
            return;
        }

        string[] csvLines = File.ReadAllLines(persistentFilePath);

        for (int i = 1; i < csvLines.Length; i++) // 첫 줄은 헤더
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
            Debug.Log($"무기이름 {weaponName}");
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

                Debug.Log($"CSV 데이터 로드 완료: {weaponName} 공격력{baseDamage}");
            }
            else
            {
                Debug.LogWarning($"CSV 데이터에 매칭되는 ScriptableObject를 찾을 수 없습니다: {weaponName}");
            }
        }
    }
    public void SaveWeaponDataToCSV()
    {
        using (StreamWriter writer = new StreamWriter(persistentFilePath))
        {
            // 헤더 작성
            writer.WriteLine("weaponName,baseDamage,baseGagePerHit,maxChargeTime,chargeMultiplier,defaultPosition,defaultRotation,damageUpgradeCount,gageUpgradeCount,additionalDamage,additionalGagePerHit");

            // ScriptableObject 데이터를 CSV로 저장
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

            Debug.Log("CSV 데이터 저장 완료");
        }
    }

    private Vector3 ParseVector3(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Debug.LogWarning("ParseVector3: 입력 값이 비어 있습니다. 기본값 Vector3.zero 반환");
            return Vector3.zero;
        }

        // 쉼표를 기준으로 문자열 나누기
        string[] parts = value.Split('/');

        // 배열 길이 확인
        if (parts.Length != 3)
        {
            Debug.LogError($"ParseVector3: 값의 형식이 잘못되었습니다. 입력값: '{value}', 배열 길이: {parts.Length}. 기본값 Vector3.zero 반환");
            return Vector3.zero;
        }

        try
        {
            // 각각의 값을 파싱
            float x = float.Parse(parts[0].Trim()); // 공백 제거 후 파싱
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());
            return new Vector3(x, y, z);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ParseVector3: 값 변환 중 오류 발생. 입력값: '{value}' - {ex.Message}. 기본값 Vector3.zero 반환");
            return Vector3.zero;
        }
    }

    private string FormatVector3(Vector3 vector)
    {
        // 쉼표 대신 슬래시(`/`)를 구분자로 사용
        return $"{vector.x}/{vector.y}/{vector.z}";
    }
}
