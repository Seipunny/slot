using UnityEngine;
using TMPro; // Добавляем для TextMeshProUGUI
using System.Collections; // Добавляем для корутин

public class MobManager : MonoBehaviour
{
    [Header("Mobs Obj")]
    public GameObject[] mobs = new GameObject[8];
    public GameObject[] previewMobs = new GameObject[8];
    [Header("Health obj")]
    public GameObject[] hp = new GameObject[6];
    [Header("Black Health obj")]
    public GameObject[] blackHp = new GameObject[6];
    [Header("HP Mobs")]
    public int[] health = new int[6];

    Animator[] mobAnimators = new Animator[8];
    int curMobId = 0;
    int curHp = 0;

    public GameObject mobsObj;
    public GameObject hpObj;
    public GameObject startGameButton;
    public GameObject previewMobsObj;
    
    [Header("Slot Machine")]
    public SlotMahineManager slotMachine; // Ссылка на слот-машину
    
    [Header("Spins System")]
    public int maxSpins = 25; // Максимальное количество спинов
    private int currentSpins = 0; // Текущее количество спинов
    public TextMeshProUGUI spinsText; // UI элемент для отображения спинов
    
    [Header("Mobs Counter UI")]
    public TextMeshProUGUI mobsCounterText; // UI элемент для отображения количества мобов до босса
    
    [Header("Preview Settings")]
    public float previewMoveSpeed = 2f; // Скорость движения превью
    private Vector3[] originalPreviewPositions; // Оригинальные позиции превью мобов
    
    [Header("Audio Sources")]
    public AudioSource damageAudio; // Звук получения урона
    public AudioSource deadAudio; // Звук смерти моба
    public AudioSource swordAudio; // Звук нанесения урона
    public AudioSource gameWinAudio; // Звук победы в игре
    public AudioSource gameLoseAudio; // Звук поражения в игре
    
    [Header("Balance System")]
    public PlayerBalance playerBalance; // Ссылка на систему баланса

    void Start()
    {
        for (int i = 0; i < mobs.Length; i++) {
            mobAnimators[i] = mobs[i].GetComponent<Animator>();
        }

        // Сохраняем оригинальные позиции превью мобов
        SaveOriginalPreviewPositions();
        previewMobsObj.SetActive(false);
        curMobId = -1;
        curHp = -1;
        mobsObj.SetActive(false);
        hpObj.SetActive(false);
    }
    
    /// <summary>
    /// Сохраняет оригинальные позиции превью мобов
    /// </summary>
    private void SaveOriginalPreviewPositions()
    {
        if (previewMobs != null && previewMobs.Length > 0)
        {
            originalPreviewPositions = new Vector3[previewMobs.Length];
            for (int i = 0; i < previewMobs.Length; i++)
            {
                if (previewMobs[i] != null)
                {
                    originalPreviewPositions[i] = previewMobs[i].transform.position;
                }
            }
        }
    }

    public void StartGame()
    {
        // Проверяем баланс перед началом игры
        if (playerBalance != null && !playerBalance.CanAffordGame())
        {
            Debug.Log("Недостаточно средств для начала игры!");
            return;
        }
        
        // Списываем ставку
        if (playerBalance != null && !playerBalance.StartGame())
        {
            return; // Не удалось списать ставку
        }
        
        // Шафлим мобов (кроме последнего - босса)
        ShuffleMobs();
        
        curMobId = 0;
        curHp = 0;
        currentSpins = 0; // Сбрасываем счетчик спинов
        Init();
        mobsObj.SetActive(true);
        hpObj.SetActive(true);
        previewMobsObj.SetActive(true); // Показываем превью объект
        startGameButton.SetActive(false);
        
        UpdateSpinsUI(); // Обновляем UI спинов
        UpdateMobsCounterUI(); // Обновляем UI счетчика мобов
        UpdatePreviewMobs(); // Обновляем превью мобов
        
        // Обновляем отображение выигрыша в начале игры
        if (playerBalance != null)
        {
            playerBalance.UpdateWinningsDisplay();
        }
        
        // Запускаем автоматические спины с небольшой задержкой
        if (slotMachine != null)
        {
            Invoke("StartAutoSpins", 0.5f);
        }
    }
    
    /// <summary>
    /// Шафлит порядок мобов, кроме последнего (босс)
    /// </summary>
    private void ShuffleMobs()
    {
        if (mobs.Length <= 1) return;
        
        // Шафлим только первые mobs.Length-1 элементов (оставляем босса в конце)
        int mobsToShuffle = mobs.Length - 1;
        
        for (int i = 0; i < mobsToShuffle; i++)
        {
            int randomIndex = Random.Range(i, mobsToShuffle);
            
            // Меняем местами только GameObject'ы мобов
            GameObject temp = mobs[i];
            mobs[i] = mobs[randomIndex];
            mobs[randomIndex] = temp;
            
            // Также меняем местами соответствующие аниматоры
            Animator tempAnimator = mobAnimators[i];
            mobAnimators[i] = mobAnimators[randomIndex];
            mobAnimators[randomIndex] = tempAnimator;
        }
        
        // Применяем тот же шафл к превью мобам
        ApplyShuffleToPreviewMobs();
        
        Debug.Log("Мобы перемешаны (кроме босса)");
    }
    
    /// <summary>
    /// Применяет результат шафла к превью мобам, меняя только их позиции X
    /// </summary>
    private void ApplyShuffleToPreviewMobs()
    {
        if (previewMobs == null || originalPreviewPositions == null) return;
        
        // Применяем тот же порядок что и у основных мобов
        for (int i = 0; i < mobs.Length; i++)
        {
            // Находим какой оригинальный моб сейчас на позиции i
            for (int j = 0; j < mobs.Length; j++)
            {
                // Проверяем по имени GameObject'а
                if (mobs[i].name == previewMobs[j].name.Replace("Preview", ""))
                {
                    if (previewMobs[j] != null)
                    {
                        Vector3 currentPos = previewMobs[j].transform.position;
                        previewMobs[j].transform.position = new Vector3(
                            originalPreviewPositions[i].x, 
                            currentPos.y, 
                            currentPos.z  // Сохраняем текущую Z позицию
                        );
                    }
                    break;
                }
            }
        }
    }
    
    /// <summary>
    /// Обновляет позиции и видимость превью мобов
    /// </summary>
    private void UpdatePreviewMobs()
    {
        if (previewMobs == null || curMobId < 0) return;
        
        // Управляем видимостью превью мобов: показываем только текущего + 3 следующих
        for (int i = 0; i < previewMobs.Length; i++)
        {
            if (previewMobs[i] != null)
            {
                // Находим соответствующий индекс в массиве mobs по имени
                int mobIndex = FindMobIndexByPreviewName(previewMobs[i].name);
                
                // Показываем превью если соответствующий моб находится в диапазоне текущий + 3 следующих
                bool shouldBeVisible = (mobIndex >= curMobId && mobIndex <= curMobId + 3);
                previewMobs[i].SetActive(shouldBeVisible);
            }
        }
        
        // Плавно двигаем previewMobsObj на -0.5 по X
        if (previewMobsObj != null)
        {
            Vector3 targetPos = previewMobsObj.transform.position;
            targetPos.x = targetPos.x - 0.5f;
            
            StartCoroutine(MovePreviewMobsObj(targetPos));
        }
    }
    
    /// <summary>
    /// Плавно перемещает previewMobsObj к целевой позиции
    /// </summary>
    private IEnumerator MovePreviewMobsObj(Vector3 targetPosition)
    {
        if (previewMobsObj == null) yield break;
        
        Vector3 startPosition = previewMobsObj.transform.position;
        float elapsedTime = 0f;
        float duration = 1f / previewMoveSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            previewMobsObj.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }
        
        previewMobsObj.transform.position = targetPosition;
    }
    
    /// <summary>
    /// Отложенный запуск автоспинов
    /// </summary>
    private void StartAutoSpins()
    {
        if (slotMachine != null && curMobId >= 0)
        {
            slotMachine.StartAutoSpin();
        }
    }
    
    /// <summary>
    /// Вызывается при завершении каждого спина
    /// </summary>
    public void OnSpinCompleted()
    {
        currentSpins++;
        UpdateSpinsUI();
        
        // Проверяем, не закончились ли спины с небольшой задержкой
        // чтобы дать время обработать урон от последнего спина
        if (currentSpins >= maxSpins)
        {
            Invoke("CheckForGameLose", 0.5f);
        }
    }
    
    /// <summary>
    /// Проверяет поражение с задержкой после последнего спина
    /// </summary>
    private void CheckForGameLose()
    {
        // Проверяем только если игра все еще активна (моб не умер)
        if (curMobId >= 0 && currentSpins >= maxSpins)
        {
            // Поражение - закончились спины, но мобы еще живы
            GameLose();
        }
    }
    
    /// <summary>
    /// Обновляет UI отображение спинов
    /// </summary>
    private void UpdateSpinsUI()
    {
        if (spinsText != null)
        {
            if (curMobId < 0) // Игра не активна
            {
                spinsText.text = ""; // Очищаем текст
            }
            else
            {
                int remainingSpins = maxSpins - currentSpins;
                spinsText.text = $"{remainingSpins}/{maxSpins}";
            }
        }
    }
    
    /// <summary>
    /// Показывает результат завершенной игры
    /// </summary>
    /// <param name="isWin">true для победы, false для поражения</param>
    private void ShowGameResult(bool isWin)
    {
        if (mobsCounterText != null)
        {
            if (isWin)
            {
                mobsCounterText.text = "Ставочка зашла!";
            }
            else
            {
                mobsCounterText.text = "Ставочка не зашла";
            }
        }
    }
    
    /// <summary>
    /// Обновляет UI отображение количества мобов до босса
    /// </summary>
    private void UpdateMobsCounterUI()
    {
        if (mobsCounterText != null && curMobId >= 0)
        {
            if (curMobId == mobs.Length - 1)
            {
                // Это босс
                mobsCounterText.text = "BOSS FIGHT!";
            }
            else
            {
                // Показываем сколько мобов осталось до босса
                int mobsLeftToBoss = (mobs.Length - 1) - curMobId;
                mobsCounterText.text = $"Boss fight in:\n{mobsLeftToBoss} mobs";
            }
        }
        else if (mobsCounterText != null)
        {
            mobsCounterText.text = "";
        }
    }
    
    /// <summary>
    /// Обработка поражения в игре
    /// </summary>
    private void GameLose()
    {
        Debug.Log("Поражение! Закончились спины.");
        
        // Уведомляем PlayerBalance о поражении
        if (playerBalance != null)
        {
            playerBalance.OnGameLost();
        }
        
        // Воспроизводим звук поражения
        if (gameLoseAudio != null)
        {
            gameLoseAudio.Play();
        }
        
        // Останавливаем автоматические спины
        if (slotMachine != null)
        {
            slotMachine.StopAutoSpin();
        }
        
        // Сбрасываем игру
        curMobId = -1;
        curHp = -1;
        currentSpins = 0;
        mobsObj.SetActive(false);
        hpObj.SetActive(false);
        startGameButton.SetActive(true);
        previewMobsObj.SetActive(false);
        
        UpdateSpinsUI(); // Очищаем текст спинов
        ShowGameResult(false); // Показываем статус поражения
    }

    void Init()
    {
        // Проверяем корректность curMobId
        if (curMobId < 0 || curMobId >= mobs.Length)
        {
            Debug.LogError($"Init: curMobId ({curMobId}) выходит за границы массива mobs (длина: {mobs.Length})");
            return;
        }
        
        if (curMobId >= health.Length)
        {
            Debug.LogError($"Init: curMobId ({curMobId}) выходит за границы массива health (длина: {health.Length})");
            return;
        }
        
        for (int i = 0; i < mobs.Length; i++) { 
            mobs[i].SetActive(false);
        }
        mobs[curMobId].SetActive(true);
        curHp = health[curMobId];
        RefreshUI();
        UpdatePreviewMobs(); // Обновляем превью при инициализации
        
        // Обновляем отображение выигрыша при переходе к новому мобу
        if (playerBalance != null)
        {
            playerBalance.UpdateWinningsDisplay();
        }
    }


    // Update is called once per frame
    public void GetDamage(int amount)
    {
        // Проверяем корректность curMobId
        if (curMobId < 0 || curMobId >= health.Length)
        {
            Debug.LogError($"curMobId ({curMobId}) выходит за границы массива health (длина: {health.Length})");
            return;
        }
        
        curHp = curHp - amount;
        if (curHp < 0)
        {
            curHp = 0;
        }

        // Вызываем анимацию удара только если моб еще жив (hp > 0)
        if (curHp > 0)
        {
            if (curMobId < mobAnimators.Length && mobAnimators[curMobId] != null)
            {
                mobAnimators[curMobId].SetTrigger("Hit");
                Debug.Log($"Анимация Hit для моба {curMobId}");
            }
            else
            {
                Debug.LogWarning($"Аниматор для моба {curMobId} не назначен или индекс выходит за границы");
            }
            
            // Воспроизводим звук получения урона (только если моб жив)
            if (damageAudio != null)
            {
                damageAudio.Play();
                swordAudio.Play();
            }
        }

        RefreshUI();
        
        // Обновляем отображение выигрыша при каждом ударе
        if (playerBalance != null)
        {
            playerBalance.UpdateWinningsDisplay();
        }
    }

    void RefreshUI() {
        
        // Проверяем корректность curMobId
        if (curMobId < 0 || curMobId >= health.Length)
        {
            Debug.LogError($"RefreshUI: curMobId ({curMobId}) выходит за границы массива health (длина: {health.Length})");
            return;
        }
        
        for (int i = 0; i < hp.Length; i++)
        {
            hp[i].SetActive(false);
            blackHp[i].SetActive(true);
        }
        for (int i = 0; i < health[curMobId]; i++)
        {
            hp[i].SetActive(true);
        }
        for (int i = 0; i < curHp; i++)
        {
            blackHp[i].SetActive(false);
        }
        if (curHp == 0)
        {
            // Вызываем анимацию смерти
            if (curMobId < mobAnimators.Length && mobAnimators[curMobId] != null)
            {
                mobAnimators[curMobId].SetTrigger("Dead");
                Debug.Log($"Анимация Dead для моба {curMobId}");
            }
            else
            {
                Debug.LogWarning($"Аниматор для моба {curMobId} не назначен для анимации Dead");
            }
            
            // Воспроизводим звук смерти моба
            if (deadAudio != null)
            {
                deadAudio.Play();
                swordAudio.Play();
            }
            
            Invoke("Death", 0.3f);
            // Скрываем превью текущего моба с той же задержкой
            Invoke("HideCurrentPreviewMob", 0.3f);
        }
    }

    /// <summary>
    /// Находит индекс моба в массиве mobs[] по имени превью
    /// </summary>
    private int FindMobIndexByPreviewName(string previewName)
    {
        // Получаем имя моба, убирая "Preview" из имени превью
        string mobName = previewName.Replace("Preview", "");
        
        // Ищем моба с таким именем в массиве mobs
        for (int i = 0; i < mobs.Length; i++)
        {
            if (mobs[i] != null && mobs[i].name == mobName)
            {
                return i;
            }
        }
        
        return -1; // Не найден
    }
    
    /// <summary>
    /// Находит превью моба по индексу текущего моба
    /// </summary>
    private GameObject FindPreviewByMobIndex(int mobIndex)
    {
        if (mobIndex < 0 || mobIndex >= mobs.Length || mobs[mobIndex] == null)
            return null;
            
        string mobName = mobs[mobIndex].name;
        
        // Ищем превью с соответствующим именем
        for (int i = 0; i < previewMobs.Length; i++)
        {
            if (previewMobs[i] != null && previewMobs[i].name == mobName + "Preview")
            {
                return previewMobs[i];
            }
        }
        
        return null;
    }

    /// <summary>
    /// Скрывает превью текущего моба с задержкой
    /// </summary>
    void HideCurrentPreviewMob()
    {
        // Находим превью текущего моба по его индексу
        GameObject currentPreview = FindPreviewByMobIndex(curMobId);
        
        if (currentPreview != null)
        {
            currentPreview.SetActive(false);
            Debug.Log($"Скрыт превью моба {curMobId} ({currentPreview.name})");
        }
    }

    void Death()
    {
        // Сбрасываем состояние аниматора умершего моба пока он еще активен
        if (curMobId >= 0 && curMobId < mobAnimators.Length && mobAnimators[curMobId] != null)
        {
            mobAnimators[curMobId].Rebind();
            Debug.Log($"Сброшено состояние аниматора для умершего моба {curMobId}");
        }
        
        curMobId++;
        
        // Уведомляем PlayerBalance об убитом мобе
        if (playerBalance != null)
        {
            playerBalance.OnMobKilled(curMobId);
        }
        
        if (curMobId >= mobs.Length)
        {
            Debug.Log("Все мобы побеждены!");
            
            // Уведомляем PlayerBalance о победе над боссом
            if (playerBalance != null)
            {
                playerBalance.OnBossDefeated();
            }
            
            curMobId = -1;
            curHp = -1;
            mobsObj.SetActive(false);
            hpObj.SetActive(false);
            startGameButton.SetActive(true);
            previewMobsObj.SetActive(false);
            
            // Воспроизводим звук победы в игре
            if (gameWinAudio != null)
            {
                gameWinAudio.Play();
            }
            
            // Останавливаем автоматические спины
            if (slotMachine != null)
            {
                slotMachine.StopAutoSpin();
            }
            
            // Сбрасываем счетчик спинов и отображаем статус победы
            currentSpins = 0;
            UpdateSpinsUI(); // Очищаем текст спинов
            ShowGameResult(true); // Показываем статус победы
        }
        else
        {
            Init();
            UpdateMobsCounterUI(); // Обновляем счетчик мобов после смерти моба
        }
        
    }
    
    /// <summary>
    /// Получить текущее количество оставшихся спинов
    /// </summary>
    public int GetRemainingSpins()
    {
        return maxSpins - currentSpins;
    }
    
    /// <summary>
    /// Проверить, активна ли игра
    /// </summary>
    public bool IsGameActive()
    {
        return curMobId >= 0 && currentSpins < maxSpins;
    }
    
    /// <summary>
    /// Получить текущий ID моба (используется для подсчета убитых мобов)
    /// </summary>
    public int GetCurrentMobId()
    {
        return curMobId;
    }
}
