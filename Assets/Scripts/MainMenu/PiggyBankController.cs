using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class PiggyBankLevel
{
    public int capacity;           // ���������� ����� �� ������ ����
    public float cycleTime;        // ����� ����� � ��������
    public Sprite piggySprite;     // �������� ������ ��� ����� ������
    public int upgradeCost;        // ��������� ��������
}

public class PiggyBankController : MonoBehaviour
{
    [SerializeField] private Image piggyImage;       // ����������� ������
    [SerializeField] private TMP_Text statusText;    // ����� ���������
    [SerializeField] private Image farmingProgress;  // Image ��� ��������� ��������
    [SerializeField] private List<PiggyBankLevel> levelsDatabase; // ���� ������ �������

    private int currentLevel = 0;                    // ������� ������� ������
    private PiggyState currentState;                 // ������� ���������
    private float timeRemaining;                     // ���������� ����� �����
    private float totalCycleTime;                    // ������ ����� ����� ��� ������� ���������
    private int collectedCoins;                      // ����������� ������

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
        farmingProgress.fillAmount = 0; // ��������� �������� = 0
    }

    void Update()
    {
        if (currentState == PiggyState.Farming)
        {
            timeRemaining -= Time.deltaTime;
            UpdateStatusText();

            float progress = 1 - (timeRemaining / totalCycleTime);
            farmingProgress.fillAmount = progress;

            // ������ ����������� ����� �� ������ ��������� (�� 0 �� capacity)
            int currentCoins = (int)(levelsDatabase[currentLevel].capacity * progress);
            collectedCoins = currentCoins;

            if (timeRemaining <= 0)
            {
                collectedCoins = levelsDatabase[currentLevel].capacity; // ������ capacity ��� ����������
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
        collectedCoins = 0; // ���������� ����������� ������ � ������ �����
        SetState(PiggyState.Farming);
    }

    void CollectCoins()
    {
        Debug.Log($"Collected {collectedCoins} coins!");
        collectedCoins = 0; // ���������� ������ ����� �����
        farmingProgress.fillAmount = 0; // �������� ��������-���
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

    // ����������� ������� ��������
    public void UpgradePiggy()
    {
        if (currentLevel < levelsDatabase.Count - 1) // ���������, �� ������������ �� �������
        {
            // ����� ����� ������ �������� �����, ����� �������� ���� ������� �������
            // ��������: if (playerCoins >= levelsDatabase[currentLevel].upgradeCost)
            int previousLevel = currentLevel;
            currentLevel++;             // ����������� �������

            UpdatePiggyVisuals();       // ��������� ������

            if (currentState == PiggyState.Farming)
            {
                // ��������� ���� � ������ ������ ������
                float progress = 1 - (timeRemaining / totalCycleTime);
                totalCycleTime = levelsDatabase[currentLevel].cycleTime;
                timeRemaining = totalCycleTime * (1 - progress); // ������������� ���������� �����
                collectedCoins = (int)(levelsDatabase[currentLevel].capacity * progress); // ��������� ������
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

    // ��������� ����� ��� ���������� ������
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

    // ��������� ����� ��� ��������� ������
    public PiggyBankLevel GetLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelsDatabase.Count)
            return levelsDatabase[levelIndex];
        return null;
    }
}