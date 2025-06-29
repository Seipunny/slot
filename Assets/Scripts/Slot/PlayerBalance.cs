using UnityEngine;
using TMPro;

public class PlayerBalance : MonoBehaviour
{
    [Header("Balance Settings")]
    public int startingBalance = 1000; // Стартовый баланс
    public int betAmount = 100; // Ставка за игру
    public int bossReward = 1000; // Награда за победу над боссом
    
    [Header("UI Elements")]
    public TextMeshProUGUI balanceText; // Отображение баланса
    public TextMeshProUGUI winningsText; // Отображение текущего выигрыша и множителя
    
    [Header("Game References")]
    public MobManager mobManager; // Ссылка на MobManager
    
    private int currentBalance;
    private int currentWinnings;
    private int totalMobs = 8; // Общее количество мобов
    private float maxMultiplier = 10f; // Максимальный множитель (x10)
    
    void Start()
    {
        // Всегда начинаем со стартовым балансом
        currentBalance = startingBalance;
        currentWinnings = 0;
        
        UpdateBalanceUI();
        UpdateWinningsUI();
    }
    
    /// <summary>
    /// Начинает новую игру, списывая ставку
    /// </summary>
    public bool StartGame()
    {
        if (currentBalance >= betAmount)
        {
            currentBalance -= betAmount;
            currentWinnings = 0;
            
            UpdateBalanceUI();
            UpdateWinningsUI();
            
            Debug.Log($"Игра начата. Списано {betAmount}$. Баланс: {currentBalance}$");
            return true;
        }
        else
        {
            Debug.Log("Недостаточно средств для начала игры!");
            return false;
        }
    }
    
    /// <summary>
    /// Обрабатывает убийство моба и обновляет множитель
    /// </summary>
    /// <param name="mobsKilled">Количество убитых мобов</param>
    public void OnMobKilled(int mobsKilled)
    {
        // Рассчитываем текущий множитель на основе убитых мобов
        float currentMultiplier = Mathf.Lerp(1f, maxMultiplier, (float)mobsKilled / totalMobs);
        currentWinnings = Mathf.RoundToInt(betAmount * currentMultiplier);
        
        UpdateWinningsUI();
        
        Debug.Log($"Мобов убито: {mobsKilled}/{totalMobs}. Множитель: x{currentMultiplier:F1}. Потенциальный выигрыш: {currentWinnings}$");
    }
    
    /// <summary>
    /// Обновляет отображение выигрыша в реальном времени (вызывается при каждом изменении игры)
    /// </summary>
    public void UpdateWinningsDisplay()
    {
        UpdateWinningsUI();
    }
    
    /// <summary>
    /// Обрабатывает победу над боссом
    /// </summary>
    public void OnBossDefeated()
    {
        currentBalance += bossReward;
        int totalWin = bossReward;
        
        UpdateBalanceUI();
        
        Debug.Log($"Босс побежден! Получено {totalWin}$. Новый баланс: {currentBalance}$");
        
        // Обнуляем текущий выигрыш после победы
        currentWinnings = 0;
        UpdateWinningsUI();
    }
    
    /// <summary>
    /// Обрабатывает поражение в игре
    /// </summary>
    public void OnGameLost()
    {
        currentWinnings = 0;
        UpdateWinningsUI();
        
        Debug.Log("Игра проиграна. Выигрыш обнулен.");
    }
    
    /// <summary>
    /// Обновляет UI баланса
    /// </summary>
    private void UpdateBalanceUI()
    {
        if (balanceText != null)
        {
            balanceText.text = $"Balance: {currentBalance}$";
        }
    }
    
    /// <summary>
    /// Обновляет UI текущего выигрыша и множителя
    /// </summary>
    private void UpdateWinningsUI()
    {
        if (winningsText != null)
        {
            if (mobManager != null && mobManager.IsGameActive())
            {
                // Получаем количество убитых мобов из MobManager
                int mobsKilled = GetKilledMobsCount();
                float currentMultiplier = Mathf.Lerp(1f, maxMultiplier, (float)mobsKilled / totalMobs);
                
                // Показываем прогресс накопления выигрыша
                string progressText = "";
                if (mobsKilled > 0)
                {
                    progressText = $"Potential WIN: {currentWinnings}$\nMult: x{currentMultiplier:F1}\nProgress: {mobsKilled}/{totalMobs} mobs";
                }
                else
                {
                    progressText = $"Current WIN: 0$\nMult: x{currentMultiplier:F1}\nProgress: {mobsKilled}/{totalMobs} mobs";
                }
                
                winningsText.text = progressText;
            }
            else
            {
                winningsText.text = ""; // Очищаем когда игра не активна
            }
        }
    }
    
    /// <summary>
    /// Получает количество убитых мобов
    /// </summary>
    private int GetKilledMobsCount()
    {
        if (mobManager != null)
        {
            // curMobId представляет индекс текущего моба
            // Количество убитых мобов = curMobId (так как мобы убиваются по порядку)
            // Если curMobId = 0, то убито 0 мобов (бой с первым мобом)
            // Если curMobId = 1, то убит 1 моб (бой со вторым мобом)
            int currentMobId = mobManager.GetCurrentMobId();
            return Mathf.Max(0, currentMobId);
        }
        return 0;
    }
    
    /// <summary>
    /// Сбрасывает баланс к стартовому значению
    /// </summary>
    [ContextMenu("Reset Balance")]
    public void ResetBalance()
    {
        currentBalance = startingBalance;
        currentWinnings = 0;
        UpdateBalanceUI();
        UpdateWinningsUI();
    }
    
    /// <summary>
    /// Получить текущий баланс
    /// </summary>
    public int GetCurrentBalance()
    {
        return currentBalance;
    }
    
    /// <summary>
    /// Проверить, достаточно ли средств для игры
    /// </summary>
    public bool CanAffordGame()
    {
        return currentBalance >= betAmount;
    }
} 