using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SlotMahineManager : MonoBehaviour
{
    [Header("Барабаны")]
    public ReelController[] reels = new ReelController[3]; // Массив из 3 барабанов
    
    [Header("Настройки спина")]
    public float delayBetweenReels = 0.3f; // Задержка между остановкой барабанов
    public bool randomResults = true; // Случайные результаты или заданные
    
    [Header("Заданные результаты (если randomResults = false)")]
    public int[] predefinedResults = { 1, 1, 1 }; // Результаты для каждого барабана
    
    [Header("Звуки")]
    public AudioSource audioSource; // Источник звука
    public AudioClip spinStartSound; // Звук старта спина
    public AudioClip spinEndSound; // Звук окончания всех барабанов
    public HapticManager hapticManager; 
    private bool isSpinning = false;
    private int[] currentResults = new int[3]; // Текущие результаты барабанов
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Проверяем, что все барабаны назначены
        ValidateReels();
        
        // Проверяем наличие AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Основная функция спина - запускает все барабаны
    /// </summary>
    public void Spin()
    {
        if (!isSpinning)
        {
            // Воспроизводим звук старта
            PlaySpinStartSound();
            
            StartCoroutine(SpinSequence());
        }
    }
    
    /// <summary>
    /// Корутина последовательности спина
    /// </summary>
    private IEnumerator SpinSequence()
    {
        isSpinning = true;
        
        // Определяем результаты для каждого барабана
        int[] targetResults = GetTargetResults();
        
        // Запускаем все барабаны одновременно
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
            {
                reels[i].StartSpin(targetResults[i]);
            }
        }
        
        // Отслеживаем остановку каждого барабана отдельно
        bool[] reelsStopped = new bool[reels.Length];
        
        while (!AllReelsStopped())
        {
            // Проверяем каждый барабан
            for (int i = 0; i < reels.Length; i++)
            {
                if (reels[i] != null && !reelsStopped[i] && !reels[i].IsSpinning())
                {
                    // Барабан только что остановился
                    reelsStopped[i] = true;
                    currentResults[i] = reels[i].GetCurrentSymbol();
                    
                    // Воспроизводим звук остановки барабана
                    PlayReelStopSound();
                    
                    Debug.Log($"Барабан {i + 1} остановился на символе {currentResults[i]}");
                }
            }
            
            yield return null;
        }
        
        isSpinning = false;
        
        // Вызываем событие окончания всего спина
        OnSpinComplete();
    }
    
    /// <summary>
    /// Получает целевые результаты для барабанов
    /// </summary>
    private int[] GetTargetResults()
    {
        int[] results = new int[3];
        
        if (randomResults)
        {
            // Генерируем случайные результаты (1-7)
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Random.Range(1, 8);
            }
        }
        else
        {
            // Используем заданные результаты
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = Mathf.Clamp(predefinedResults[i], 1, 7);
            }
        }
        
        return results;
    }
    
    /// <summary>
    /// Проверяет, остановились ли все барабаны
    /// </summary>
    private bool AllReelsStopped()
    {
        foreach (ReelController reel in reels)
        {
            if (reel != null && reel.IsSpinning())
            {
                return false;
            }
        }
        return true;
    }
    
    /// <summary>
    /// Вызывается при завершении спина
    /// </summary>
    private void OnSpinComplete()
    {
        // Воспроизводим звук окончания
        PlaySpinEndSound();
        
        Debug.Log($"Спин завершен! Результаты: {currentResults[0]}, {currentResults[1]}, {currentResults[2]}");
        
        // Проверяем на выигрышные комбинации
        CheckWinConditions();
    }
    
    /// <summary>
    /// Воспроизводит звук старта спина
    /// </summary>
    private void PlaySpinStartSound()
    {
        if (audioSource != null && spinStartSound != null)
        {
            audioSource.PlayOneShot(spinStartSound);
        }
    }
    
    /// <summary>
    /// Воспроизводит звук остановки одного барабана
    /// </summary>
    private void PlayReelStopSound()
    {
        if (hapticManager != null)
        {
            hapticManager.HapticTriggerMedium();
        }
        if (audioSource != null && spinEndSound != null)
        {
            audioSource.PlayOneShot(spinEndSound);
        }
    }
    
    /// <summary>
    /// Воспроизводит звук окончания всех барабанов (финальный)
    /// </summary>
    private void PlaySpinEndSound()
    {
        // Можно добавить отдельный звук для финала или оставить тишину
        // if (audioSource != null && spinCompleteSound != null)
        // {
        //     audioSource.PlayOneShot(spinCompleteSound);
        // }
    }
    
    /// <summary>
    /// Проверяет выигрышные комбинации
    /// </summary>
    private void CheckWinConditions()
    {
        // Проверка на три одинаковых символа
        if (currentResults[0] == currentResults[1] && currentResults[1] == currentResults[2])
        {
            Debug.Log($"ДЖЕКПОТ! Три одинаковых символа: {currentResults[0]}");
            OnJackpot(currentResults[0]);
        }
        // Проверка на два одинаковых символа
        else if (currentResults[0] == currentResults[1] || currentResults[1] == currentResults[2] || currentResults[0] == currentResults[2])
        {
            Debug.Log("Малый выигрыш! Два одинаковых символа.");
            OnSmallWin();
        }
        else
        {
            Debug.Log("Проигрыш. Попробуйте еще раз!");
            OnLose();
        }
    }
    
    /// <summary>
    /// Событие джекпота
    /// </summary>
    private void OnJackpot(int symbol)
    {
        // Здесь можно добавить эффекты, звуки, начисление очков и т.д.
    }
    
    /// <summary>
    /// Событие малого выигрыша
    /// </summary>
    private void OnSmallWin()
    {
        // Здесь можно добавить эффекты для малого выигрыша
    }
    
    /// <summary>
    /// Событие проигрыша
    /// </summary>
    private void OnLose()
    {
        // Здесь можно добавить эффекты для проигрыша
    }
    
    /// <summary>
    /// Проверяет корректность настройки барабанов
    /// </summary>
    private void ValidateReels()
    {
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] == null)
            {
                Debug.LogWarning($"Барабан {i + 1} не назначен в SlotMahineManager!");
            }
        }
    }
    
    /// <summary>
    /// Получает текущие результаты барабанов
    /// </summary>
    public int[] GetCurrentResults()
    {
        return currentResults;
    }
    
    /// <summary>
    /// Проверяет, идет ли спин
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }
    
    /// <summary>
    /// Принудительно останавливает все барабаны
    /// </summary>
    public void ForceStop()
    {
        StopAllCoroutines();
        
        for (int i = 0; i < reels.Length; i++)
        {
            if (reels[i] != null)
            {
                reels[i].StopAtSymbol(Random.Range(1, 8));
                currentResults[i] = reels[i].GetCurrentSymbol();
            }
        }
        
        isSpinning = false;
        OnSpinComplete();
    }
    
    /// <summary>
    /// Устанавливает заданные результаты для следующего спина
    /// </summary>
    public void SetPredefinedResults(int reel1, int reel2, int reel3)
    {
        randomResults = false;
        predefinedResults[0] = Mathf.Clamp(reel1, 1, 7);
        predefinedResults[1] = Mathf.Clamp(reel2, 1, 7);
        predefinedResults[2] = Mathf.Clamp(reel3, 1, 7);
    }
    
    /// <summary>
    /// Включает режим случайных результатов
    /// </summary>
    public void EnableRandomResults()
    {
        randomResults = true;
    }
}
