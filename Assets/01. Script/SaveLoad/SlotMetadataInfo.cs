using System;

// 세이브 슬롯 메타데이터 정보 (UI 표시용)
public class SlotMetadataInfo
{
    public int slotIndex;                // 슬롯 인덱스 (0, 1, 2)
    public bool hasData;                 // 슬롯에 데이터가 있는지 여부
    public string playerName;            // 플레이어 이름
    public int chapterProgress;          // 챕터 진행도
    public DateTime lastSaveTime;        // 마지막 저장 시간
    public int totalPlayTime;            // 총 플레이 시간 (초)

    // 마지막 저장 시간을 문자열로 반환
    public string GetLastSaveTimeString()
    {
        if (lastSaveTime == DateTime.MinValue)
            return "없음";

        return lastSaveTime.ToString("yyyy-MM-dd HH:mm:ss");
    }

    // 총 플레이 시간을 형식화된 문자열로 반환
    public string GetFormattedPlayTime()
    {
        if (totalPlayTime <= 0)
            return "0분";

        int hours = totalPlayTime / 3600;
        int minutes = (totalPlayTime % 3600) / 60;

        if (hours > 0)
            return $"{hours}시간 {minutes}분";
        else
            return $"{minutes}분";
    }
}