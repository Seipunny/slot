using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerDiceLogic playerSlot;
    [SerializeField] private BotDiceLogic botSlot;
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private GameObject gameEndWindow;
    [SerializeField] private TextMeshProUGUI resultText;

    private bool isPlayerTurn = true;
    private const int WINNING_SCORE = 500;
    private Button playerSpinButton; // Ссылка на кнопку Spin игрока
    private bool isGameOver = false;

    void Start()
    {
        if (playerSlot == null || botSlot == null || gameStatusText == null)
        {
            Debug.LogError("Please assign all required components in the GameManager inspector!");
            return;
        }

        UpdateGameStatus();
    }

    void Update()
    {
        if (!isGameOver) CheckWinCondition();
    }

    public bool CanPlayerTakeTurn()
    {
        return isPlayerTurn && !isGameOver;
    }

    public bool CanBotTakeTurn()
    {
        return !isPlayerTurn && !isGameOver;
    }

    // В GameManager
    public void OnPlayerTurnEnd(int playerBalance)
    {
        Debug.Log($"OnPlayerTurnEnd called with balance: {playerBalance}, isGameOver: {isGameOver}");
        if (isGameOver) return;

        if (playerBalance >= WINNING_SCORE)
        {
            EndGame("Player wins!");
        }
        else
        {
            isPlayerTurn = false;
            Debug.Log($"Player turn ended, setting isPlayerTurn to: {isPlayerTurn}");
            if (playerSpinButton != null)
            {
                playerSpinButton.interactable = false;
            }
            UpdateGameStatus();
        }
    }

    public void OnBotTurnEnd(int botBalance)
    {
        if (isGameOver) return;

        if (botBalance >= WINNING_SCORE)
        {
            EndGame("Bot wins!");
        }
        else
        {
            isPlayerTurn = true;
            if (playerSpinButton != null)
            {
                playerSpinButton.interactable = true;
            }
            UpdateGameStatus();
        }
    }

    private void CheckWinCondition()
    {
        if (playerSlot.GetTotalBalance() >= WINNING_SCORE)
        {
            EndGame("Player wins!");
        }
        else if (botSlot.GetTotalBalance() >= WINNING_SCORE)
        {
            EndGame("Bot wins!");
        }
    }

    private void EndGame(string winner)
    {
        isGameOver = true; // Устанавливаем флаг окончания игры
        Debug.Log(winner);
        gameStatusText.text = $"{winner} Game Over!";
        playerSlot.enabled = false;
        botSlot.enabled = false;

        gameEndWindow.SetActive(true);
        resultText.text = $"{winner}";
    }

    private bool IsGameOver()
    {
        return isGameOver;
    }

    private void UpdateGameStatus()
    {
        if (isGameOver) return;
        string status = isPlayerTurn ? "Player's Turn" : "Bot's Turn";
        gameStatusText.text = status;
    }

    public void RestartGame()
    {
        // Сбрасываем флаги игры
        isGameOver = false;
        isPlayerTurn = true;

        // Останавливаем и перезапускаем состояние игрока и бота
        playerSlot.RestartGame();
        botSlot.RestartGame();

        // Убеждаемся, что компоненты активны
        playerSlot.enabled = true;
        botSlot.enabled = true;

        // Сбрасываем UI
        gameEndWindow.SetActive(false);

        UpdateGameStatus();
    }

}