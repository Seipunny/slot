using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BotDiceLogic : DiceLogicBase
{
    private readonly float decisionDelaySpin = 2f;
    private readonly float decisionDelayAfterSpin = 3f;
    private readonly float decisionDelayReSpin = 0.5f;
    private readonly float decisionDelayEnd = 2f;
    public GameObject[] playersButtons;
    public GameObject lockButton;

    private Coroutine botGameLoopCoroutine;

    protected override void Start()
    {
        base.Start();
        if (gameObject.activeInHierarchy)
        {
            StartBotGameLoop();
        }
    }

    // Вызывается, когда объект становится активным
    private void OnEnable()
    {
        if (botGameLoopCoroutine == null && gameManager != null)
        {
            StartBotGameLoop();
        }
    }

    // Вызывается, когда объект отключается
    private void OnDisable()
    {
        if (botGameLoopCoroutine != null)
        {
            StopCoroutine(botGameLoopCoroutine);
            botGameLoopCoroutine = null;
            Debug.Log("BotGameLoop stopped due to OnDisable");
        }
    }

    public override bool CanTakeTurn()
    {
        bool canTakeTurn = gameManager.CanBotTakeTurn();
        //Debug.Log($"Bot CanTakeTurn: {canTakeTurn}, isPlayerTurn: {gameManager.isPlayerTurn}, isGameOver: {gameManager.isGameOver}");
        if (canTakeTurn)
        {
            playersButtons[0].SetActive(false);
            playersButtons[1].SetActive(false);
            playersButtons[2].SetActive(false);
            lockButton.SetActive(true);
            blink_sprite.SetParent(gameManager.gameObject.transform);
        }
        return canTakeTurn;
    }

    protected override void OnTurnEnd(int balance)
    {
        playersButtons[0].SetActive(true);
        playersButtons[1].SetActive(false);
        playersButtons[2].SetActive(false);
        lockButton.SetActive(false);
        blink_sprite.SetParent(playersButtons[0].transform);
        gameManager.OnBotTurnEnd(balance);
    }

    public override void RestartGame()
    {
        base.RestartGame();
        if (botGameLoopCoroutine != null)
        {
            StopCoroutine(botGameLoopCoroutine);
            botGameLoopCoroutine = null;
        }
        if (gameObject.activeInHierarchy)
        {
            StartBotGameLoop();
        }
    }

    private void StartBotGameLoop()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("Cannot start BotGameLoop: GameObject is inactive");
            return;
        }
        if (botGameLoopCoroutine != null)
        {
            StopCoroutine(botGameLoopCoroutine);
        }
        botGameLoopCoroutine = StartCoroutine(BotGameLoop());
        Debug.Log("BotGameLoop started!");
    }

    private IEnumerator BotGameLoop()
    {
        while (true)
        {
            while (!CanTakeTurn())
            {
                Debug.Log($"BotGameLoop: Waiting for CanTakeTurn, CurrentState: {currentState}");
                yield return null; // Ждём следующий кадр
            }

            Debug.Log("Bot is taking turn!");

            switch (currentState)
            {
                case GameState.Spin:
                    yield return new WaitForSeconds(decisionDelaySpin);
                    currentTurnPoints = 0;
                    yield return StartCoroutine(SpinCoroutine(false));
                    break;

                case GameState.AfterSpin:
                    yield return new WaitForSeconds(decisionDelayAfterSpin);
                    int[] currentResults = GetCurrentResults();
                    if (ShouldLockDice(currentResults))
                    {
                        LockDice(currentResults);
                        currentState = GameState.ReSpin;
                    }
                    else
                    {
                        totalBalance += currentTurnPoints;
                        currentTurnPoints = 0;
                        UpdateBalanceText();
                        currentState = GameState.Spin;
                        ResetLocks();
                        OnTurnEnd(totalBalance);
                    }
                    break;

                case GameState.ReSpin:
                    yield return new WaitForSeconds(decisionDelayReSpin);
                    yield return StartCoroutine(SpinCoroutine(true));
                    break;

                case GameState.End:
                    yield return new WaitForSeconds(decisionDelayEnd);
                    totalBalance += currentTurnPoints;
                    currentTurnPoints = 0;
                    UpdateBalanceText();
                    currentState = GameState.Spin;
                    ResetLocks();
                    OnTurnEnd(totalBalance);
                    break;
            }
        }
    }

    private int[] GetCurrentResults()
    {
        int[] results = new int[diceAnimators.Length];
        for (int i = 0; i < diceAnimators.Length; i++)
        {
            results[i] = GetCurrentFace(i);
        }
        return results;
    }

    private bool ShouldLockDice(int[] results)
    {
        if (IsStraight(results, 0, 4) || IsStraight(results, 1, 5))
        {
            return false;
        }

        Dictionary<int, int> faceCount = GetFaceCount(results);
        Dictionary<int, int> potentialRewards = new Dictionary<int, int>();

        foreach (var pair in faceCount)
        {
            int face = pair.Key;
            int count = pair.Value;

            int currentReward = GetRewardForCountAndFace(count, face);
            int potentialCount = count + 1;
            int potentialReward = GetRewardForCountAndFace(potentialCount, face);

            if (potentialReward > currentReward && potentialCount <= 5)
            {
                potentialRewards[face] = potentialReward - currentReward;
            }
        }

        return potentialRewards.Count > 0;
    }

    private void LockDice(int[] results)
    {
        Dictionary<int, int> faceCount = GetFaceCount(results);
        Dictionary<int, int> potentialRewards = new Dictionary<int, int>();

        foreach (var pair in faceCount)
        {
            int face = pair.Key;
            int count = pair.Value;

            int currentReward = GetRewardForCountAndFace(count, face);
            int potentialCount = count + 1;
            int potentialReward = GetRewardForCountAndFace(potentialCount, face);

            if (potentialReward > currentReward && potentialCount <= 5)
            {
                potentialRewards[face] = potentialReward - currentReward;
            }
        }

        if (potentialRewards.Count == 0)
            return;

        int bestFace = potentialRewards.OrderByDescending(x => x.Value).First().Key;

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] == bestFace)
            {
                isDiceLocked[i] = true;
                lockerImages[i].SetActive(true);
            }
        }
    }

    private int GetRewardForCountAndFace(int count, int face)
    {
        switch (count)
        {
            case 1:
                if (face == 1) return 100;
                if (face == 5) return 50;
                return 0;
            case 2:
                if (face == 1) return 200;
                if (face == 5) return 100;
                return 0;
            case 3:
                if (face == 1) return 1000;
                if (face == 2) return 200;
                if (face == 3) return 300;
                if (face == 4) return 400;
                if (face == 5) return 500;
                if (face == 6) return 600;
                return 0;
            case 4:
                if (face == 1) return 2000;
                if (face == 2) return 400;
                if (face == 3) return 600;
                if (face == 4) return 800;
                if (face == 5) return 1000;
                if (face == 6) return 1200;
                return 0;
            case 5:
                if (face == 1) return 4000;
                if (face == 2) return 800;
                if (face == 3) return 1200;
                if (face == 4) return 1600;
                if (face == 5) return 2000;
                if (face == 6) return 2400;
                return 0;
            default:
                return 0;
        }
    }

    private Dictionary<int, int> GetFaceCount(int[] results)
    {
        Dictionary<int, int> faceCount = new Dictionary<int, int>();
        for (int i = 1; i <= 6; i++)
        {
            faceCount[i] = 0;
        }
        foreach (int face in results)
        {
            faceCount[face]++;
        }
        return faceCount;
    }
}