using UnityEngine;

public class PlinkoTest : MonoBehaviour
{
    [Header("Тестовые настройки")]
    public bool enableDebugLogs = true;
    public bool showGizmos = true;
    public KeyCode testSpawnKey = KeyCode.Space;
    public KeyCode testGameOverKey = KeyCode.G;
    public KeyCode testStartGameKey = KeyCode.S;
    public KeyCode testCashoutKey = KeyCode.C;
    
    private PhysxGameManager gameManager;
    
    void Start()
    {
        // Используем синглтон
        gameManager = PhysxGameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("PhysxGameManager.Instance не найден!");
        }
        else
        {
            Debug.Log("PlinkoTest: Менеджер игры найден через синглтон");
        }
    }
    
    void Update()
    {
        if (gameManager == null) return;
        
        // Тестовый спавн кубика
        if (Input.GetKeyDown(testSpawnKey))
        {
            TestSpawnDice();
        }
        
        // Тестовый проигрыш
        if (Input.GetKeyDown(testGameOverKey))
        {
            TestGameOver();
        }
        
        // Тестовый старт игры
        if (Input.GetKeyDown(testStartGameKey))
        {
            TestStartGame();
        }
        
        // Тестовый cashout
        if (Input.GetKeyDown(testCashoutKey))
        {
            TestCashout();
        }
        
        // Показываем информацию о кубиках
        if (enableDebugLogs && Input.GetKey(KeyCode.I))
        {
            ShowDiceInfo();
        }
        
        // Показываем информацию о игре
        if (enableDebugLogs && Input.GetKey(KeyCode.O))
        {
            ShowGameInfo();
        }
    }
    
    void TestSpawnDice()
    {
        if (gameManager != null)
        {
            Debug.Log("=== ТЕСТ: Принудительный спавн кубика ===");
            // Вызываем приватный метод через рефлексию
            var method = typeof(PhysxGameManager).GetMethod("SpawnDice", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(gameManager, null);
            }
        }
    }
    
    void TestGameOver()
    {
        if (gameManager != null)
        {
            Debug.Log("=== ТЕСТ: Принудительный проигрыш ===");
            // Вызываем приватный метод через рефлексию
            var method = typeof(PhysxGameManager).GetMethod("GameOver", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(gameManager, null);
            }
        }
    }
    
    void TestStartGame()
    {
        if (gameManager != null)
        {
            Debug.Log("=== ТЕСТ: Запуск игры ===");
            gameManager.StartGame();
        }
    }
    
    void TestCashout()
    {
        if (gameManager != null)
        {
            Debug.Log("=== ТЕСТ: Cashout ===");
            gameManager.Cashout();
        }
    }
    
    void ShowDiceInfo()
    {
        GameObject[] dice = GameObject.FindGameObjectsWithTag("dice");
        Debug.Log($"=== ИНФОРМАЦИЯ О КУБИКАХ ===\nВсего кубиков на сцене: {dice.Length}");
        
        for (int i = 0; i < dice.Length; i++)
        {
            if (dice[i] != null)
            {
                Rigidbody2D rb = dice[i].GetComponent<Rigidbody2D>();
                string velocity = rb != null ? rb.linearVelocity.ToString() : "Нет Rigidbody2D";
                Debug.Log($"Кубик {i}: {dice[i].name} | Позиция: {dice[i].transform.position} | Скорость: {velocity}");
            }
        }
    }
    
    void ShowGameInfo()
    {
        if (gameManager == null) return;
        
        Debug.Log($"=== ИНФОРМАЦИЯ О ИГРЕ ===\n" +
                  $"Баланс: ${gameManager.GetCurrentBalance():F2}\n" +
                  $"Мультипликатор: x{gameManager.GetCurrentMultiplier():F2}\n" +
                  $"Игра активна: {gameManager.IsGameActive()}\n" +
                  $"Игра окончена: {gameManager.IsGameOver()}\n" +
                  $"Потеряно кубиков: {gameManager.GetDiceLost()}\n" +
                  $"Всего попаданий: {gameManager.GetTotalHits()}");
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        // Показываем область пропасти
        GameObject[] gaps = GameObject.FindGameObjectsWithTag("Gap");
        foreach (GameObject gap in gaps)
        {
            if (gap != null)
            {
                BoxCollider2D collider = gap.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                    Gizmos.DrawCube(gap.transform.position, collider.size);
                    
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(gap.transform.position, collider.size);
                }
            }
        }
        
        // Показываем круги
        GameObject[] circles = GameObject.FindGameObjectsWithTag("circle");
        foreach (GameObject circle in circles)
        {
            if (circle != null)
            {
                CircleCollider2D collider = circle.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    Gizmos.color = new Color(0f, 0f, 1f, 0.3f);
                    Gizmos.DrawSphere(circle.transform.position, collider.radius);
                    
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(circle.transform.position, collider.radius);
                }
            }
        }
        
        // Показываем стены
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject wall in walls)
        {
            if (wall != null)
            {
                BoxCollider2D collider = wall.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                    Gizmos.DrawCube(wall.transform.position, collider.size);
                    
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(wall.transform.position, collider.size);
                }
            }
        }
    }
} 