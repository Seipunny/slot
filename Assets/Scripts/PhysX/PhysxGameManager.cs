using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class PhysxGameManager : MonoBehaviour
{
    [Header("Синглтон")]
    public static PhysxGameManager Instance { get; private set; }
    
    [Header("Настройки игры")]
    public GameObject dicePrefab;
    public Transform spawnPoint;
    public float spawnInterval = 2f;
    public int maxDiceCount = 2;
    public float startBalance = 1000f;
    public float gameCost = 100f; // Стоимость одной игры
    
    [Header("Настройки кругов")]
    public float circleImpulseForce = 10f;
    public float circleImpulseRadius = 1f;
    
    [Header("Настройки пропасти")]
    public Transform gapBottom;
    public float loseYPosition = -10f;
    
    [Header("UI элементы")]
    public TextMeshProUGUI balanceText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI potentialWinText;
    public TextMeshProUGUI gameStatusText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI transactionText;
    public UnityEngine.UI.Button startButton;
    public UnityEngine.UI.Button cashoutButton;
    
    [Header("Менеджеры")]
    public HapticManager hapticManager; // Перетащите сюда HapticManager со сцены
    
    [Header("Игровая логика")]
    public float baseReward = 10f;
    public float multiplierPerHit = 0.25f;
    public float multiplierPenaltyOnDiceLoss = 0.5f;
    
    private List<GameObject> activeDice = new List<GameObject>();
    private HashSet<GameObject> processedDice = new HashSet<GameObject>(); // Для предотвращения двойной обработки
    private float nextSpawnTime;
    private float currentBalance;
    private float currentMultiplier = 1f;
    private bool gameActive = false;
    private bool gameOver = false;
    private int diceLost = 0;
    private int totalHits = 0;
    
    void Awake()
    {
        // Синглтон паттерн
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Принудительно устанавливаем правильные значения
        multiplierPerHit = 0.25f;
        
        InitializeGame();
    }

    void Update()
    {
        if (!gameActive || gameOver) return;
        
        // Спавн новых кубиков только если нет потерянных кубиков
        // Общее количество кубиков = активные + потерянные
        int totalDiceSpawned = activeDice.Count + diceLost;
        if (Time.time >= nextSpawnTime && totalDiceSpawned < maxDiceCount)
        {
            SpawnDice();
            nextSpawnTime = Time.time + spawnInterval;
        }
        
        // Проверка проигрыша
        CheckLoseCondition();
        
        // Очистка уничтоженных кубиков из списка
        activeDice.RemoveAll(dice => dice == null);
    }
    
    void InitializeGame()
    {
        currentBalance = startBalance;
        ResetGameSession();
        
        UpdateUI();
        UpdateGameStatus();
        ClearTransaction();
        
        // Настраиваем кнопки
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
            startButton.gameObject.SetActive(true);
        }
        
        if (cashoutButton != null)
        {
            cashoutButton.onClick.RemoveAllListeners();
            cashoutButton.onClick.AddListener(Cashout);
            cashoutButton.gameObject.SetActive(false);
        }
    }
    
    void ResetGameSession()
    {
        currentMultiplier = 1f;
        gameActive = false;
        gameOver = false;
        diceLost = 0;
        totalHits = 0;
        processedDice.Clear(); // Очищаем список обработанных кубиков
    }
    
    // ФУНКЦИЯ ДЛЯ КНОПКИ "НАЧАТЬ ИГРУ"
    public void StartGame()
    {
        if (gameActive) return;
        
        // Проверяем, достаточно ли средств
        if (currentBalance < gameCost)
        {
            Debug.Log("Недостаточно средств для начала игры!");
            if (gameStatusText != null)
            {
                gameStatusText.text = "Недостаточно средств!";
            }
            return;
        }
        
        // Вычитаем стоимость игры из баланса
        currentBalance -= gameCost;
        
        // Обнуляем все параметры сессии
        ResetGameSession();
        gameActive = true;
        
        // Спавним первый кубик
        nextSpawnTime = Time.time + 1f;
        
        // Обновляем UI
        if (startButton != null) startButton.gameObject.SetActive(false);
        if (cashoutButton != null) cashoutButton.gameObject.SetActive(false); // Скрываем до достижения x2
        
        // Показываем транзакцию списания
        ShowTransaction($"-${gameCost:F0}", Color.red);
        
        UpdateUI();
        UpdateGameStatus();
        Debug.Log($"Игра запущена! Списано: ${gameCost}, Баланс: ${currentBalance:F2}");
    }
    
    // ФУНКЦИЯ ДЛЯ КНОПКИ "CASHOUT"
    public void Cashout()
    {
        if (!gameActive) return;
        
        // Награда = ставка × мультипликатор
        float finalReward = gameCost * currentMultiplier;
        currentBalance += finalReward;
        
        Debug.Log($"Cashout! Мультипликатор: {currentMultiplier:F2}, Ставка: ${gameCost:F0}, Награда: ${finalReward:F2}");
        
        // Показываем транзакцию награды
        ShowTransaction($"+${finalReward:F2}", Color.green);
        
        // Автоматически завершаем игру и обнуляем все
        EndGameSession();
    }
    
    // ФУНКЦИЯ ДЛЯ КНОПКИ "НАЧАТЬ ЗАНОВО" (после проигрыша)
    public void RestartGame()
    {
        Debug.Log("Перезапуск игры...");
        
        // Уничтожаем все кубики
        foreach (GameObject dice in activeDice)
        {
            if (dice != null)
            {
                Debug.Log("Уничтожаем кубик при перезапуске: " + dice.name);
                Destroy(dice);
            }
        }
        activeDice.Clear();
        
        // Скрываем UI проигрыша
        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);
        
        InitializeGame();
    }
    
    // Внутренний метод для завершения игровой сессии
    void EndGameSession()
    {
        gameActive = false;
        
        // Уничтожаем все кубики
        foreach (GameObject dice in activeDice)
        {
            if (dice != null)
            {
                Destroy(dice);
            }
        }
        activeDice.Clear();
        processedDice.Clear(); // Очищаем список обработанных кубиков
        
        // Обнуляем все параметры сессии
        ResetGameSession();
        
        // Обновляем UI
        if (cashoutButton != null) cashoutButton.gameObject.SetActive(false);
        if (startButton != null) startButton.gameObject.SetActive(true);
        
        UpdateUI();
        UpdateGameStatus();
        
        Debug.Log($"Сессия завершена. Баланс: ${currentBalance:F2}");
    }
    
    void SpawnDice()
    {
        if (dicePrefab != null && spawnPoint != null)
        {
            GameObject newDice = Instantiate(dicePrefab, spawnPoint.position, Quaternion.identity);
            activeDice.Add(newDice);
            Debug.Log("Спавн нового кубика. Всего кубиков: " + activeDice.Count);
        }
    }
    
    void CheckLoseCondition()
    {
        foreach (GameObject dice in activeDice.ToArray())
        {
            if (dice != null && dice.transform.position.y < loseYPosition)
            {
                // Проверяем, не был ли этот кубик уже обработан
                if (!processedDice.Contains(dice))
                {
                    Debug.Log("Кубик упал ниже порога проигрыша: " + dice.name);
                    OnDiceLost(dice);
                }
                return;
            }
        }
    }
    
    public void OnDiceFellInGap(GameObject dice)
    {
        Debug.Log("Менеджер получил уведомление о падении кубика в пропасть: " + dice.name);
        
        // Проверяем, не был ли этот кубик уже обработан
        if (processedDice.Contains(dice))
        {
            Debug.Log("Кубик уже был обработан, игнорируем: " + dice.name);
            return;
        }
        
        OnDiceLost(dice);
    }
    
    void OnDiceLost(GameObject dice)
    {
        // Добавляем кубик в список обработанных
        processedDice.Add(dice);
        
        // Вызываем тактильную обратную связь при потере кубика
        if (hapticManager != null)
        {
            hapticManager.HapticTriggerMedium();
        }
        
        diceLost++;
        
        // Уменьшаем мультипликатор
        currentMultiplier *= multiplierPenaltyOnDiceLoss;
        
        Debug.Log($"Кубик потерян! Всего потеряно: {diceLost}, Мультипликатор: {currentMultiplier:F2}");
        
        // Удаляем кубик из списка активных если он там еще есть
        if (activeDice.Contains(dice))
        {
            activeDice.Remove(dice);
            Debug.Log("Кубик удален из списка активных в OnDiceLost. Осталось: " + activeDice.Count);
        }
        
        // Уничтожаем кубик
        if (dice != null)
        {
            Destroy(dice);
        }
        
        // Игра заканчивается только когда ВСЕ кубики потеряны
        if (diceLost >= maxDiceCount)
        {
            // Все кубики потеряны - игра окончена
            GameOver();
        }
        else
        {
            // Если потерян только один кубик - продолжаем игру
            Debug.Log($"Потерян {diceLost} из {maxDiceCount} кубиков. Игра продолжается.");
            UpdateUI();
            UpdateGameStatus();
        }
    }
    
    void GameOver()
    {
        if (gameOver) return;
        
        gameOver = true;
        gameActive = false;
        
        Debug.Log("ИГРА ОКОНЧЕНА! Все кубики потеряны!");
        
        if (gameOverText != null)
        {
            gameOverText.text = $"Игра окончена!\nВсе кубики потеряны ({diceLost}/{maxDiceCount})\nФинальный мультипликатор: {currentMultiplier:F2}\nНажмите 'Начать заново'";
            gameOverText.gameObject.SetActive(true);
        }
        
        if (cashoutButton != null) cashoutButton.gameObject.SetActive(false);
        if (startButton != null) startButton.gameObject.SetActive(true);
        
        UpdateGameStatus();
    }
    
    public void OnCircleHit(GameObject circle, GameObject dice)
    {
        if (!gameActive) return;
        
        // Вызываем тактильную обратную связь
        if (hapticManager != null)
        {
            hapticManager.HapticTriggerMedium();
        }
        
        // Увеличиваем мультипликатор
        float oldMultiplier = currentMultiplier;
        currentMultiplier += multiplierPerHit;
        totalHits++;
        
        Debug.Log($"Попадание в круг! Мультипликатор: {oldMultiplier:F2} + {multiplierPerHit:F2} = {currentMultiplier:F2}, Всего попаданий: {totalHits}");
        
        // Применяем импульс от центра круга
        Rigidbody2D diceRb = dice.GetComponent<Rigidbody2D>();
        if (diceRb != null)
        {
            Vector2 direction = (dice.transform.position - circle.transform.position).normalized;
            diceRb.AddForce(direction * circleImpulseForce, ForceMode2D.Impulse);
        }
        
        UpdateUI();
        UpdateGameStatus();
    }
    
    void UpdateUI()
    {
        if (balanceText != null)
            balanceText.text = $"Баланс: ${currentBalance:F2}";
        
        if (multiplierText != null)
            multiplierText.text = $"Мультипликатор: x{currentMultiplier:F2}";
        
        // Обновляем потенциальный выигрыш (ставка × мультипликатор)
        if (potentialWinText != null)
        {
            float potentialWin = gameCost * currentMultiplier;
            potentialWinText.text = $"Потенциальный выигрыш: ${potentialWin:F2}";
        }
        
        // Обновляем видимость кнопки Cashout
        UpdateCashoutButtonVisibility();
    }
    
    void UpdateCashoutButtonVisibility()
    {
        if (cashoutButton != null && gameActive && !gameOver)
        {
            // Показываем кнопку Cashout только при мультипликаторе >= 2.0
            bool shouldShowCashout = currentMultiplier >= 2.0f;
            cashoutButton.gameObject.SetActive(shouldShowCashout);
            
            if (shouldShowCashout)
            {
                Debug.Log($"Кнопка Cashout теперь доступна! Мультипликатор: {currentMultiplier:F2}");
            }
        }
    }
    
    void UpdateGameStatus()
    {
        if (gameStatusText != null)
        {
            if (!gameActive)
            {
                if (currentBalance < gameCost)
                {
                    gameStatusText.text = "Недостаточно средств для игры";
                }
                else
                {
                    gameStatusText.text = "Нажмите 'Начать игру'";
                }
            }
            else if (gameOver)
            {
                gameStatusText.text = "Игра окончена";
            }
            else
            {
                string lostStatus = diceLost > 0 ? $" | ПОТЕРЯНО: {diceLost}/{maxDiceCount}" : "";
                gameStatusText.text = $"Игра активна | Кубиков: {activeDice.Count}/{maxDiceCount}{lostStatus} | Попаданий: {totalHits}";
            }
        }
    }
    
    void ShowTransaction(string text, Color color)
    {
        if (transactionText != null)
        {
            transactionText.text = text;
            transactionText.color = color;
            transactionText.gameObject.SetActive(true);
            
            // Скрываем транзакцию через 3 секунды
            Invoke("ClearTransaction", 3f);
        }
    }
    
    void ClearTransaction()
    {
        if (transactionText != null)
        {
            transactionText.gameObject.SetActive(false);
        }
    }
    
    // Публичные методы для получения информации
    public float GetCurrentBalance() => currentBalance;
    public float GetCurrentMultiplier() => currentMultiplier;
    public bool IsGameActive() => gameActive;
    public bool IsGameOver() => gameOver;
    public int GetDiceLost() => diceLost;
    public int GetTotalHits() => totalHits;
    public float GetGameCost() => gameCost;
    public bool CanStartGame() => currentBalance >= gameCost;
} 