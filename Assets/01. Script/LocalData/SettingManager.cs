//using UnityEngine.Rendering;
//using UnityEngine;

//[System.Serializable]
//public class SettingsData
//{
//    public GraphicsSettings graphics = new GraphicsSettings();
//    public SoundSettings sound = new SoundSettings();
//    public ControlSettings controls = new ControlSettings();
//}

//public class SettingsManager : Singleton<SettingsManager>
//{
//    public void InitializeSettings()
//    {
//        string path = Path.Combine(Application.persistentDataPath, "SaveFiles", "settings.json");
//        if (!File.Exists(path))
//        {
//            SettingsData newData = new SettingsData();
//            // �⺻ ������ �ʱ�ȭ
//            newData.graphics.resolution = "1920x1080";
//            newData.graphics.quality = "high";
//            newData.sound.volume = 80;
//            newData.controls.moveUp = "W";
//            // ... ��Ÿ ������

//            string json = JsonUtility.ToJson(newData, true);
//            File.WriteAllText(path, json);
//        }
//    }
//}