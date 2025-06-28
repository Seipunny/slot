using UnityEngine;
using System.Collections;

public class ReelController : MonoBehaviour
{
    [Header("Настройки барабана")]
    public RectTransform reelTransform; // RectTransform барабана
    
    [Header("Скорость вращения")]
    public float minSpinSpeed = 1200f; // Минимальная скорость
    public float maxSpinSpeed = 1800f; // Максимальная скорость
    
    [Header("Настройки остановки")]
    public float minSpinDistance = 3600f; // Минимальное расстояние полного спина
    public float maxSpinDistance = 5400f; // Максимальное расстояние полного спина
    public AnimationCurve slowdownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0); // Кривая замедления
    
    [Header("Позиции символов")]
    private float[] symbolPositions = { 900f, 600f, 300f, 0f, -300f, -600f, -900f };
    private const float REEL_CYCLE = 1800f; // Полный цикл барабана
    private const float SYMBOL_HEIGHT = 300f; // Высота одной картинки
    
    private bool isSpinning = false;
    private int currentSymbol = 1;
    private float currentSpeed = 0f; // Текущая скорость барабана
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (reelTransform == null)
            reelTransform = GetComponent<RectTransform>();
        
        // Устанавливаем начальную позицию (символ 1)
        SetPosition(symbolPositions[0]);
        currentSymbol = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    /// <summary>
    /// Запускает спин барабана с случайной скоростью
    /// </summary>
    public void StartSpin(int targetSymbol)
    {
        if (!isSpinning && targetSymbol >= 1 && targetSymbol <= 7)
        {
            StartCoroutine(SpinWithSmartSlowdown(targetSymbol));
        }
    }
    
    /// <summary>
    /// Спин с умным замедлением за одну картинку до цели
    /// </summary>
    private IEnumerator SpinWithSmartSlowdown(int targetSymbol)
    {
        isSpinning = true;
        currentSymbol = targetSymbol;
        
        // Случайная начальная скорость для каждого спина
        float initialSpeed = Random.Range(minSpinSpeed, maxSpinSpeed);
        currentSpeed = initialSpeed;
        
        // Случайное расстояние до начала проверки замедления
        float spinDistance = Random.Range(minSpinDistance, maxSpinDistance);
        float traveledDistance = 0f;
        
        float targetY = symbolPositions[targetSymbol - 1];
        
        // Основной спин с полной скоростью
        while (traveledDistance < spinDistance)
        {
            float deltaDistance = currentSpeed * Time.deltaTime;
            traveledDistance += deltaDistance;
            
            // Обновляем позицию
            Vector2 pos = reelTransform.anchoredPosition;
            pos.y -= deltaDistance;
            pos.y = WrapPosition(pos.y);
            reelTransform.anchoredPosition = pos;
            
            yield return null;
        }
        
        // Продолжаем с полной скоростью пока не останется одна картинка до цели
        while (isSpinning)
        {
            Vector2 pos = reelTransform.anchoredPosition;
            float distanceToTarget = CalculateDownwardDistance(pos.y, targetY);
            
            // Если осталась одна картинка (300px) - начинаем плавное замедление
            if (distanceToTarget <= SYMBOL_HEIGHT)
            {
                yield return StartCoroutine(SmoothSlowdownToTarget(targetY, distanceToTarget));
                break;
            }
            
            // Продолжаем с полной скоростью
            float deltaDistance = currentSpeed * Time.deltaTime;
            pos.y -= deltaDistance;
            pos.y = WrapPosition(pos.y);
            reelTransform.anchoredPosition = pos;
            
            yield return null;
        }
        
        currentSpeed = 0f;
        isSpinning = false;
    }
    
    /// <summary>
    /// Плавное замедление за одну картинку до точной остановки
    /// </summary>
    private IEnumerator SmoothSlowdownToTarget(float targetY, float initialDistance)
    {
        float slowdownStartTime = Time.time;
        float slowdownDuration = initialDistance / (currentSpeed * 0.5f); // Время замедления
        Vector2 startPos = reelTransform.anchoredPosition;
        
        while (Time.time - slowdownStartTime < slowdownDuration && isSpinning)
        {
            float elapsed = Time.time - slowdownStartTime;
            float progress = elapsed / slowdownDuration;
            
            // Применяем кривую замедления (от 1 до 0)
            float speedMultiplier = slowdownCurve.Evaluate(progress);
            float currentSpeedAdjusted = currentSpeed * speedMultiplier;
            
            // Обновляем позицию
            Vector2 pos = reelTransform.anchoredPosition;
            float deltaDistance = currentSpeedAdjusted * Time.deltaTime;
            pos.y -= deltaDistance;
            pos.y = WrapPosition(pos.y);
            reelTransform.anchoredPosition = pos;
            
            // Проверяем близость к цели
            float distanceToTarget = CalculateDownwardDistance(pos.y, targetY);
            if (distanceToTarget <= 10f)
            {
                break;
            }
            
            yield return null;
        }
        
        // Точная установка финальной позиции
        SetPosition(targetY);
    }
    
    /// <summary>
    /// Рассчитывает расстояние вниз до целевой позиции
    /// </summary>
    private float CalculateDownwardDistance(float fromY, float toY)
    {
        fromY = WrapPosition(fromY);
        toY = WrapPosition(toY);
        
        float distance;
        if (toY <= fromY)
        {
            distance = fromY - toY;
        }
        else
        {
            distance = fromY + 900f + (900f - toY);
        }
        
        return distance;
    }
    
    /// <summary>
    /// Зацикливание позиции барабана
    /// </summary>
    private float WrapPosition(float yPos)
    {
        while (yPos < -900f)
            yPos += REEL_CYCLE;
        while (yPos > 900f)
            yPos -= REEL_CYCLE;
        return yPos;
    }
    
    /// <summary>
    /// Устанавливает точную позицию барабана
    /// </summary>
    private void SetPosition(float yPosition)
    {
        Vector2 pos = reelTransform.anchoredPosition;
        pos.y = yPosition;
        reelTransform.anchoredPosition = pos;
    }
    
    /// <summary>
    /// Проверяет, крутится ли барабан
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    /// <summary>
    /// Получает текущий символ
    /// </summary>
    public int GetCurrentSymbol()
    {
        return currentSymbol;
    }
    
    /// <summary>
    /// Получает текущую скорость барабана
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Останавливает барабан немедленно
    /// </summary>
    public void StopAtSymbol(int symbol)
    {
        if (symbol >= 1 && symbol <= 7)
        {
            StopAllCoroutines();
            SetPosition(symbolPositions[symbol - 1]);
            currentSymbol = symbol;
            currentSpeed = 0f;
            isSpinning = false;
        }
    }
    
    /// <summary>
    /// Получает позицию символа по номеру
    /// </summary>
    public float GetSymbolPosition(int symbolNumber)
    {
        if (symbolNumber >= 1 && symbolNumber <= 7)
            return symbolPositions[symbolNumber - 1];
        return 0f;
    }
}
