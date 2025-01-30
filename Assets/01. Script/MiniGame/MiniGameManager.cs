// 미니게임 매니저
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

public class MiniGameManager : MonoBehaviour
{
    public DodgeMiniGameUI dodgeUI;  // Inspector에서 할당
    public event Action<MiniGameType, MiniGameResult> OnMiniGameComplete;

    private IMiniGame currentMiniGame;
    private Dictionary<MiniGameType, IMiniGame> miniGameCache = new Dictionary<MiniGameType, IMiniGame>();
    public Dictionary<MiniGameType, MonoBehaviour> miniGameUIs = new Dictionary<MiniGameType, MonoBehaviour>();
    private void Awake()
    {
        if (dodgeUI != null)
        {
            RegisterUI(MiniGameType.Dodge, dodgeUI);
            Debug.Log("DodgeGameUI registered from inspector reference");
        }
        foreach (var kvp in miniGameUIs)
        {
            Debug.Log($"Registered UI - Type: {kvp.Key} value : {kvp.Value}");
        }
    }
    private void CreateMiniGame(MiniGameType type)
    {
        if (!miniGameCache.ContainsKey(type))
        {
            miniGameCache[type] = type switch
            {
                MiniGameType.Dodge => new DodgeMiniGameWrapper(),
                //MiniGameType.Parry => new ParryMiniGame(),
                //MiniGameType.QuickTime => new QuickTimeMiniGame(),
                _ => throw new ArgumentException($"Unknown mini game type: {type}")
            };
        }
        currentMiniGame = miniGameCache[type];
    }

    public void StartMiniGame(MiniGameType type, float difficulty)
    {
        
        Debug.Log($"StartMiniGame called - Type: {type}, Difficulty: {difficulty}");
        Debug.Log($"miniGameUIs count after StartMiniGame: {miniGameUIs.Count}");
        Debug.Log($"Registering UI for type: {type}, Type value: {(int)type}");
        Debug.Log($"Trying to fetch UI for type: {type}, Type value: {(int)type}");
        foreach (var pair in miniGameUIs)
        {
            Debug.Log($"Key: {pair.Key}, UI Type: {pair.Value.GetType().Name}");
        }
        CreateMiniGame(type);
        currentMiniGame.Initialize(difficulty);
        Time.timeScale = 0.5f; // 슬로우 모션 효과
        Debug.Log(miniGameUIs.TryGetValue(type, out var uii));
        // UI 활성화
        if (miniGameUIs.TryGetValue(type, out var ui))
        {

            Debug.Log($"Found UI for type {type}, activating...");
            ui.gameObject.SetActive(true);  // UI GameObject 자체를 활성화
            if (ui is DodgeMiniGameUI dodgeUI && currentMiniGame is DodgeMiniGameWrapper dodgeGame)
            {
                Debug.Log("Initializing DodgeMiniGameUI");
                dodgeUI.gameObject.SetActive(true);  // UI를 먼저 활성화
                dodgeUI.Initialize(dodgeGame);       // 그 다음 초기화
            }
        }
        else
        {
            Debug.LogError($"No UI found for game type: {type}");
        }

        currentMiniGame.Start();
    }

    private void Update()
    {
        if (currentMiniGame != null && !currentMiniGame.IsComplete)
        {
            currentMiniGame.Update();
            if (currentMiniGame.IsComplete)
            {
                CompleteMiniGame();
            }
        }
    }

    private void CompleteMiniGame()
    {
        Time.timeScale = 1f;
        var result = currentMiniGame.GetResult();
        OnMiniGameComplete?.Invoke(currentMiniGame.Type, result);

        // UI 비활성화
        if (miniGameUIs.TryGetValue(currentMiniGame.Type, out var ui))
        {
            ui.gameObject.SetActive(false);
        }

        currentMiniGame = null;
    }

    public void RegisterUI(MiniGameType type, MonoBehaviour ui)
    {
        Debug.Log("RegisterUI Called!");
        Debug.Log($"Dictionary count before: {miniGameUIs.Count}");
        miniGameUIs[type] = ui;
        ui.gameObject.SetActive(false);
        Debug.Log($"Dictionary count after: {miniGameUIs.Count}");
        Debug.Log($"UI registered? {miniGameUIs.ContainsKey(type)}");
    }

    public void HandleDodgeReward(MiniGameResult result)
    {
        var player = GameInitializer.Instance.GetPlayerClass();
        float invincibleDuration = 2f; // 무적 시간을 위한 코루틴 필요

        switch (result)
        {
            case MiniGameResult.Perfect:
                // 완벽 회피
                Debug.Log(player.GetStats().AttackPower);
                StartCoroutine(GrantInvincibility(player, invincibleDuration));
                player.ModifyPower(attackAmount: 30);
                Debug.Log(player.GetStats().AttackPower);
                break;

            case MiniGameResult.Good:
                // 일반 회피
                Debug.Log(player.GetStats().Speed);
                StartCoroutine(GrantInvincibility(player, invincibleDuration * 0.5f));
                player.ModifyPower(speedAmount: 2);
                Debug.Log(player.GetStats().Speed);
                break;

            case MiniGameResult.Miss:
                // 실패 - 아무 보상 없음, 보스 공격에 맞음
                break;
        }
    }

    private IEnumerator GrantInvincibility(PlayerClass player, float duration)
    {
        player.isInvicible = true;
        Debug.Log($"{duration}동안 무적");
        yield return new WaitForSeconds(duration);
        player.isInvicible = false;
    }
}