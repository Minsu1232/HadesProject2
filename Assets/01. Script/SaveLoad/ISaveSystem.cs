// 저장 시스템 인터페이스
public interface ISaveSystem
{
    void SaveData<T>(T data, string dataId) where T : class;
    T LoadData<T>(string dataId) where T : class, new();
    bool HasData(string dataId);
    void DeleteData(string dataId);

    // 새로 추가된 슬롯 관련 메서드
    void SetCurrentSlot(int slot);
    int GetCurrentSlot();
    void DeleteSlot();
    bool SlotExists();
}