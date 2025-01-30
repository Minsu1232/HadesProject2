using UnityEngine;

public class DodgeMiniGameWrapper : IMiniGame
{
    private DodgeMiniGame dodgeGame;
    private MiniGameResult currentResult;
    private float currentDifficulty = 1f;  // ���̵� ����� �ʵ� �߰�
    public MiniGameType Type => MiniGameType.Dodge;
    public bool IsComplete { get; private set; }

    public DodgeMiniGameWrapper()
    {
        dodgeGame = new DodgeMiniGame();
        dodgeGame.OnDodgeResultReceived += HandleDodgeResult;
        dodgeGame.OnMiniGameEnded += HandleMiniGameEnded;
    }

    public void Initialize(float difficulty)
    {
        IsComplete = false;
        currentResult = MiniGameResult.Miss;
        currentDifficulty = difficulty;  // ���̵� ����
    }

    public void Start()
    {
        dodgeGame.StartDodgeMiniGame(currentDifficulty);  // ����� ���̵� ����
    }

    public void Update()
    {
        if (IsComplete) return;

        if (!dodgeGame.Update(Time.deltaTime))
        {
            IsComplete = true;
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dodgeGame.ProcessInput(dodgeGame.GetCurrentProgress());
        }
    }

    public void Cancel()
    {
        dodgeGame.EndMiniGame(false);
        IsComplete = true;
        currentResult = MiniGameResult.Cancel;
    }

    public MiniGameResult GetResult() => currentResult;

    private void HandleDodgeResult(DodgeMiniGame.DodgeResult result)
    {
        currentResult = result switch
        {
            DodgeMiniGame.DodgeResult.Perfect => MiniGameResult.Perfect,
            DodgeMiniGame.DodgeResult.Good => MiniGameResult.Good,
            _ => MiniGameResult.Miss
        };
    }

    private void HandleMiniGameEnded()
    {
        IsComplete = true;
    }

    // �߰� ���� getter
    public float GetCurrentProgress() => dodgeGame.GetCurrentProgress();
    public float GetSuccessWindowStart() => dodgeGame.GetSuccessWindowStart();
    public float GetSuccessWindowEnd() => dodgeGame.GetSuccessWindowEnd();
    public float GetRemainingTimeNormalized() => dodgeGame.GetRemainingTimeNormalized();
}
