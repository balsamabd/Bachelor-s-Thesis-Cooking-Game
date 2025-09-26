using System;
using UnityEngine;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;

    public enum GameMode { Tutorial, Gameplay }
    public GameMode CurrentMode { get; private set; }

    private enum State { WaitingToStart, CountdownToStart, GamePlaying, GameOver }
    private State state;

    private float waitingToStartTimer = 1f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        state = State.WaitingToStart;
        CurrentMode = GameMode.Tutorial;
    }

    private void Update()
    {
        switch (state)
        {
            case State.WaitingToStart:
                waitingToStartTimer -= Time.deltaTime;
                if (waitingToStartTimer <= 0f)
                {
                    ChangeState(State.CountdownToStart);
                }
                break;

            case State.CountdownToStart:
                countdownToStartTimer -= Time.deltaTime;
                OnStateChanged?.Invoke(this, EventArgs.Empty);

                if (countdownToStartTimer <= 0f)
                {
                    //Actual Game Timer
                    gamePlayingTimerMax = (CurrentMode == GameMode.Tutorial) ? 120f : 600f;
                    gamePlayingTimer = gamePlayingTimerMax;
                    ChangeState(State.GamePlaying);
                }
                break;

            case State.GamePlaying:
                gamePlayingTimer -= Time.deltaTime;
                if (gamePlayingTimer <= 0f)
                {
                    HandleGameOver();
                }
                break;

            case State.GameOver:
                // Do nothing; wait for scene change or user
                break;
        }
    }

    private void ChangeState(State newState)
    {
        state = newState;
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void HandleGameOver()
    {
        if (state == State.GameOver) return; // prevent multiple triggers

        ChangeState(State.GameOver);
        Debug.Log($"Game Over! Mode: {CurrentMode}, NPCFlow: {FeedbackSelectionUI.IsNPCFlow}");

        // Tutorial delay
        if (CurrentMode == GameMode.Tutorial && !FeedbackSelectionUI.IsNPCFlow)
        {
            Invoke(nameof(GoToFeedbackSelection), 5f); // 5s delay so UI shows
        }
        else if (CurrentMode == GameMode.Tutorial && FeedbackSelectionUI.IsNPCFlow)
        {
            // NPC tutorial: go back to GamePlaying
            ChangeState(State.GamePlaying);
        }
        // Gameplay: stay in GameOver until user action
    }

    private void GoToFeedbackSelection()
    {
        Loader.Load(Loader.Scene.FeedbackSelectionScene);
    }

    // Public helpers
    public bool IsGamePlaying() => state == State.GamePlaying;
    public bool IsCountdownToStartActive() => state == State.CountdownToStart;
    public float GetCountdownToStartTimer() => countdownToStartTimer;
    public bool IsGameOver() => state == State.GameOver;
    public float GetGamePlayingTimerNormalized() => 1 - (gamePlayingTimer / gamePlayingTimerMax);

    public void StartGameplay(GameMode mode)
    {
        CurrentMode = mode;
        state = State.WaitingToStart;
        waitingToStartTimer = 1f;
        countdownToStartTimer = 3f;
        gamePlayingTimer = gamePlayingTimerMax;

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetGameMode(GameMode mode)
    {
        CurrentMode = mode;
    }
}
