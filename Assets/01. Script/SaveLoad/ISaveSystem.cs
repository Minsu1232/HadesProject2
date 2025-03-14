// 저장 시스템 인터페이스
public interface ISaveSystem
{
    void SaveData<T>(T data, string dataId) where T : class;
    T LoadData<T>(string dataId) where T : class, new();
    bool HasData(string dataId);
    void DeleteData(string dataId);
}
