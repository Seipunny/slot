using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameStatusText;
    [SerializeField] private GameObject gameEndWindow;
    [SerializeField] private TextMeshProUGUI resultText;

    private bool isPlayerTurn = true;
    private const int WINNING_SCORE = 500;
    private Button playerSpinButton; // ������ �� ������ Spin ������
    private bool isGameOver = false;

    void Start()
    {
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

    // � GameManager
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
        
    }

    private void EndGame(string winner)
    {
        isGameOver = true; // ������������� ���� ��������� ����
        Debug.Log(winner);
        gameStatusText.text = $"{winner} Game Over!";
        

        gameEndWindow.SetActive(true);
        resultText.text = $"{winner}";
    }

    private void UpdateGameStatus()
    {
        if (isGameOver) return;
        string status = isPlayerTurn ? "Player's Turn" : "Bot's Turn";
        gameStatusText.text = status;
    }

    public void RestartGame()
    {
        isGameOver = false;
        isPlayerTurn = true;
        gameEndWindow.SetActive(false);
        UpdateGameStatus();
    }

}