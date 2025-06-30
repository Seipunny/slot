using UnityEngine;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour
{
    [Header("Кнопки")]
    public Button startButton;
    public Button cashoutButton;
    public Button restartButton; // Новая кнопка для перезапуска после проигрыша
    
    private PhysxGameManager gameManager;
    
    void Start()
    {
        // Получаем ссылку на менеджер игры
        gameManager = PhysxGameManager.Instance;
        
        // Настраиваем обработчики кнопок
        SetupButtons();
    }
    
    void Update()
    {
        // Обновляем состояние кнопок каждый кадр
        UpdateButtonStates();
    }
    
    void SetupButtons()
    {
        // Кнопка "Начать игру"
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => {
                Debug.Log("Нажата кнопка Start");
                if (gameManager != null)
                {
                    gameManager.StartGame();
                }
            });
        }
        
        // Кнопка "Забрать выигрыш" (Cashout)
        if (cashoutButton != null)
        {
            cashoutButton.onClick.RemoveAllListeners();
            cashoutButton.onClick.AddListener(() => {
                Debug.Log("Нажата кнопка Cashout");
                if (gameManager != null)
                {
                    gameManager.Cashout();
                }
            });
        }
        
        // Кнопка "Начать заново" (после проигрыша)
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(() => {
                Debug.Log("Нажата кнопка Restart");
                if (gameManager != null)
                {
                    gameManager.RestartGame();
                }
            });
        }
    }
    
    void UpdateButtonStates()
    {
        if (gameManager == null) return;
        
        bool gameActive = gameManager.IsGameActive();
        bool gameOver = gameManager.IsGameOver();
        bool canStartGame = gameManager.CanStartGame();
        float currentMultiplier = gameManager.GetCurrentMultiplier();
        
        // Обновляем видимость и доступность кнопок
        if (startButton != null)
        {
            // Кнопка "Начать игру" видна только когда игра не активна
            startButton.gameObject.SetActive(!gameActive);
            // Доступна только если достаточно средств
            startButton.interactable = canStartGame && !gameActive;
        }
        
        if (cashoutButton != null)
        {
            // Кнопка "Забрать выигрыш" видна только во время активной игры И при мультипликаторе >= 2.0
            bool shouldShowCashout = gameActive && !gameOver && currentMultiplier >= 2.0f;
            cashoutButton.gameObject.SetActive(shouldShowCashout);
            cashoutButton.interactable = shouldShowCashout;
        }
        
        if (restartButton != null)
        {
            // Кнопка "Начать заново" видна только после проигрыша
            restartButton.gameObject.SetActive(gameOver);
            restartButton.interactable = gameOver;
        }
    }
    
    // Публичные методы для внешнего управления
    public void EnableAllButtons()
    {
        if (startButton != null) startButton.interactable = true;
        if (cashoutButton != null) cashoutButton.interactable = true;
        if (restartButton != null) restartButton.interactable = true;
    }
    
    public void DisableAllButtons()
    {
        if (startButton != null) startButton.interactable = false;
        if (cashoutButton != null) cashoutButton.interactable = false;
        if (restartButton != null) restartButton.interactable = false;
    }
    
    // Методы для отладки
    void OnGUI()
    {
        if (Debug.isDebugBuild && gameManager != null)
        {
            GUILayout.BeginArea(new Rect(10, 100, 250, 200));
            GUILayout.Label("=== Button Controller Debug ===");
            GUILayout.Label($"Game Active: {gameManager.IsGameActive()}");
            GUILayout.Label($"Game Over: {gameManager.IsGameOver()}");
            GUILayout.Label($"Can Start Game: {gameManager.CanStartGame()}");
            GUILayout.Label($"Balance: ${gameManager.GetCurrentBalance():F2}");
            GUILayout.Label($"Game Cost: ${gameManager.GetGameCost():F0}");
            GUILayout.EndArea();
        }
    }
} 