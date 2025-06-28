using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class PiggyBankLevel
{
    public int capacity;           // Количество монет за полный цикл
    public float cycleTime;        // Время цикла в секундах
    public Sprite piggySprite;     // Картинка свиньи для этого уровня
    public int upgradeCost;        // Стоимость прокачки
}

public class PiggyBankController : MonoBehaviour
{
    [SerializeField] private Image piggyImage;       // Изображение свиньи
    [SerializeField] private TMP_Text statusText;    // Текст состояния
    [SerializeField] private Image farmingProgress;  // Image для прогресса фарминга
    [SerializeField] private List<PiggyBankLevel> levelsDatabase; // База данных уровней

    private int currentLevel = 0;                    // Текущий уровень свиньи
    private PiggyState currentState;                 // Текущее состояние
    private float timeRemaining;                     // Оставшееся время цикла
    private float totalCycleTime;                    // Полное время цикла для расчета прогресса
    private int collectedCoins;                      // Накопленные монеты

    private enum PiggyState
    {
        Idle,           // "Start Farming"
        Farming,        // "Farming... 8:00:00"
        ReadyToCollect  // "Collect X coins"
    }

    void Start()
    {
        UpdatePiggyVisuals();
        SetState(PiggyState.Idle);
        farmingProgress.fillAmount = 0; // Начальный прогресс = 0
    }

    void Update()
    {
        if (currentState == PiggyState.Farming)
        {
            timeRemaining -= Time.deltaTime;
            UpdateStatusText();

            float progress = 1 - (timeRemaining / totalCycleTime);
            farmingProgress.fillAmount = progress;

            // Расчет накопленных монет на основе прогресса (от 0 до capacity)
            int currentCoins = (int)(levelsDatabase[currentLevel].capacity * progress);
            collectedCoins = currentCoins;

            if (timeRemaining <= 0)
            {
                collectedCoins = levelsDatabase[currentLevel].capacity; // Полный capacity при завершении
                SetState(PiggyState.ReadyToCollect);
            }
        }
    }

    public void OnButtonClick()
    {
        switch (currentState)
        {
            case PiggyState.Idle:
                StartFarming();
                break;
            case PiggyState.ReadyToCollect:
                CollectCoins();
                break;
            case PiggyState.Farming:
                break;
        }
    }

    void StartFarming()
    {
        totalCycleTime = levelsDatabase[currentLevel].cycleTime;
        timeRemaining = totalCycleTime;
        collectedCoins = 0; // Сбрасываем накопленные монеты в начале цикла
        SetState(PiggyState.Farming);
    }

    void CollectCoins()
    {
        Debug.Log($"Collected {collectedCoins} coins!");
        collectedCoins = 0; // Сбрасываем монеты после сбора
        farmingProgress.fillAmount = 0; // Обнуляем прогресс-бар
        SetState(PiggyState.Idle);
    }

    void SetState(PiggyState newState)
    {
        currentState = newState;
        UpdateStatusText();
    }

    void UpdateStatusText()
    {
        switch (currentState)
        {
            case PiggyState.Idle:
                statusText.text = "Start Farming";
                break;
            case PiggyState.ReadyToCollect:
                statusText.text = $"Collect {collectedCoins} coins";
                farmingProgress.fillAmount = 1;
                break;
            case PiggyState.Farming:
                int currentCoins = collectedCoins;
                int totalCapacity = levelsDatabase[currentLevel].capacity;
                statusText.text = $"Farming... {FormatTime(timeRemaining)}\n{currentCoins}/{totalCapacity}";
                break;
        }
    }

    string FormatTime(float seconds)
    {
        int hours = (int)(seconds / 3600);
        int minutes = (int)((seconds % 3600) / 60);
        int secs = (int)(seconds % 60);
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }

    void UpdatePiggyVisuals()
    {
        piggyImage.sprite = levelsDatabase[currentLevel].piggySprite;
    }

    // Обновленная функция прокачки
    public void UpgradePiggy()
    {
        if (currentLevel < levelsDatabase.Count - 1) // Проверяем, не максимальный ли уровень
        {
            // Здесь будет логика проверки монет, когда добавишь свою систему баланса
            // Например: if (playerCoins >= levelsDatabase[currentLevel].upgradeCost)
            int previousLevel = currentLevel;
            currentLevel++;             // Увеличиваем уровень

            UpdatePiggyVisuals();       // Обновляем спрайт

            if (currentState == PiggyState.Farming)
            {
                // Обновляем цикл с учетом нового уровня
                float progress = 1 - (timeRemaining / totalCycleTime);
                totalCycleTime = levelsDatabase[currentLevel].cycleTime;
                timeRemaining = totalCycleTime * (1 - progress); // Пересчитываем оставшееся время
                collectedCoins = (int)(levelsDatabase[currentLevel].capacity * progress); // Обновляем монеты
                Debug.Log($"Piggy upgraded to level {currentLevel}! Cycle updated.");
            }
            else
            {
                Debug.Log($"Piggy upgraded to level {currentLevel}!");
            }
        }
        else
        {
            Debug.Log("Max level reached!");
        }
    }

    // Публичный метод для добавления уровня
    public void AddLevel(int capacity, float cycleTime, Sprite piggySprite, int upgradeCost)
    {
        levelsDatabase.Add(new PiggyBankLevel
        {
            capacity = capacity,
            cycleTime = cycleTime,
            piggySprite = piggySprite,
            upgradeCost = upgradeCost
        });
    }

    // Публичный метод для получения уровня
    public PiggyBankLevel GetLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelsDatabase.Count)
            return levelsDatabase[levelIndex];
        return null;
    }
}