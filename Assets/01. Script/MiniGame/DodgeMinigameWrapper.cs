using System;
using UnityEngine;

public class DodgeMiniGameWrapper : IMiniGame
{
    private DodgeMiniGame dodgeGame;
    private MiniGameResult currentResult;
    private float currentDifficulty = 1f;  // 난이도 저장용 필드 추가
    public MiniGameType Type => MiniGameType.Dodge;
    public bool IsComplete { get; private set; }   

    private MiniGameManager miniGameManager;  // 매니저 참조 추가
    public DodgeMiniGameWrapper(MiniGameManager manager)
    {
        miniGameManager = manager;
        dodgeGame = new DodgeMiniGame();
        dodgeGame.OnDodgeResultReceived += HandleDodgeResult;  // 결과 이벤트 구독 추가
        dodgeGame.OnMiniGameEnded += HandleMiniGameEnded;
    }

    public void Initialize(float difficulty)
    {
        IsComplete = false;
        currentResult = MiniGameResult.Miss;
        currentDifficulty = difficulty;  // 난이도 저장
    }

    public void Start()
    {
        dodgeGame.StartDodgeMiniGame(currentDifficulty);  // 저장된 난이도 전달
    }

    public void Update()
    {
        if (IsComplete) return;

        // Time.deltaTime 대신 dodgeGame 내부에서 unscaledDeltaTime을 사용하므로 매개변수 없이 호출합니다.
        if (!dodgeGame.Update())
        {
            IsComplete = true;
            return;
        }

        // 입력 시점의 currentProgress 값을 기반으로 판정 처리
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

        // 결과 UI 표시
        miniGameManager.ShowMiniGameResult(result);
    }

    private void HandleMiniGameEnded()
    {
        IsComplete = true;
    }

    // 추가 정보 getter
    public float GetCurrentProgress() => dodgeGame.GetCurrentProgress();
    public float GetSuccessWindowStart() => dodgeGame.GetSuccessWindowStart();
    public float GetSuccessWindowEnd() => dodgeGame.GetSuccessWindowEnd();
    public float GetRemainingTimeNormalized() => dodgeGame.GetRemainingTimeNormalized();
}
