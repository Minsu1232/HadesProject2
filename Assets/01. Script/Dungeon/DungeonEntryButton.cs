using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class DungeonEntryButton : MonoBehaviour
{
    [SerializeField] private string dungeonSceneName = "DungeonScene";
    [SerializeField] private string startingStageID = "1_1"; // ��: Stage1_1, Stage2_1

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        PlayerPrefs.SetString("CurrentStageID", startingStageID);
        PlayerPrefs.Save();

        // �ε� ȭ�鸸 ��� (���̵� ȿ�� ����)
        LoadingScreen.Instance.ShowLoading(dungeonSceneName);
    }
}