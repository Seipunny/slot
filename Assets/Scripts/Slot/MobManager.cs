using UnityEngine;
using TMPro; // Добавляем для TextMeshProUGUI

public class MobManager : MonoBehaviour
{
    [Header("Mobs Obj")]
    public GameObject[] mobs = new GameObject[8];
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
    
    [Header("Slot Machine")]
    public SlotMahineManager slotMachine; // Ссылка на слот-машину
    
    [Header("Spins System")]
    public int maxSpins = 25; // Максимальное количество спинов
    private int currentSpins = 0; // Текущее количество спинов
    public TextMeshProUGUI spinsText; // UI элемент для отображения спинов
    
    [Header("Audio Sources")]
    public AudioSource damageAudio; // Звук получения урона
    public AudioSource deadAudio; // Звук смерти моба
    public AudioSource swordAudio; // Звук нанесения урона
    public AudioSource gameWinAudio; // Звук победы в игре
    public AudioSource gameLoseAudio; // Звук поражения в игре

    void Start()
    {
        for (int i = 0; i < mobs.Length; i++) {
            mobAnimators[i] = mobs[i].GetComponent<Animator>();
        }

        curMobId = -1;
        curHp = -1;
        mobsObj.SetActive(false);
        hpObj.SetActive(false);
    }

    public void StartGame()
    {
        curMobId = 0;
        curHp = 0;
        currentSpins = 0; // Сбрасываем счетчик спинов
        Init();
        mobsObj.SetActive(true);
        hpObj.SetActive(true);
        startGameButton.SetActive(false);
        
        UpdateSpinsUI(); // Обновляем UI спинов
        
        // Запускаем автоматические спины с небольшой задержкой
        if (slotMachine != null)
        {
            Invoke("StartAutoSpins", 0.5f);
        }
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
        
        // Проверяем, не закончились ли спины
        if (currentSpins >= maxSpins && curMobId >= 0)
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
            int remainingSpins = maxSpins - currentSpins;
            spinsText.text = $"{remainingSpins}/{maxSpins}";
        }
    }
    
    /// <summary>
    /// Обработка поражения в игре
    /// </summary>
    private void GameLose()
    {
        Debug.Log("Поражение! Закончились спины.");
        
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
        
        UpdateSpinsUI();
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
        if (curMobId >= mobs.Length)
        {
            Debug.Log("Все мобы побеждены!");
            curMobId = -1;
            curHp = -1;
            mobsObj.SetActive(false);
            hpObj.SetActive(false);
            startGameButton.SetActive(true);
            
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
            
            // Сбрасываем счетчик спинов
            currentSpins = 0;
            UpdateSpinsUI();
        }
        else
        {
            Init();
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
}
