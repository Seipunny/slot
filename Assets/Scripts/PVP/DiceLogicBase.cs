using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public abstract class DiceLogicBase : MonoBehaviour
{
    [SerializeField] protected float spinDurationMin, spinDurationMax, stopDelay;
    [SerializeField] protected Animator[] diceAnimators = new Animator[5];
    [SerializeField] protected Image[] diceResultImages = new Image[5];
    [SerializeField] protected Button[] lockButtons;
    [SerializeField] protected GameObject[] arrowHintImages;
    [SerializeField] protected GameObject[] lockerImages;
    // ������� ������ ������ � �����, �������� �� 4 ��������� ������
    [SerializeField] protected Button spin_button;
    [SerializeField] protected Button end_button;
    [SerializeField] protected Button respin_button;
    [SerializeField] protected Button locked_button;
    [SerializeField] protected Transform blink_sprite;

    [SerializeField] protected Image[] combinationImages = new Image[5];
    [SerializeField] protected Sprite[] faceSprites = new Sprite[7];
    [SerializeField] protected Sprite[] diceSprites = new Sprite[6];
    [SerializeField] protected TextMeshProUGUI rewardText, balanceText;
    [SerializeField] protected HapticManager hapticManager;
    [SerializeField] protected GameManager gameManager;

    protected bool[] isDiceSpinning = new bool[5];
    protected float[] diceSpinTimeLeft = new float[5];
    protected bool[] isDiceLocked = new bool[5];
    protected int[] currentFaces = new int[5];

    protected int currentTurnPoints, totalBalance;
    protected GameState currentState = GameState.Spin;

    protected readonly Color winningColor = Color.white;
    protected readonly Color nonWinningColor = new Color(0.78f, 0.41f, 0.41f);

    // ����, ������� ������������� ��������� ������ ��������
    protected bool isSpinCoroutineActive = false;

    public enum GameState { Spin, AfterSpin, ReSpin, End }

    protected virtual void Start()
    {
        ValidateComponents();
        InitializeUI();
    }

    protected virtual void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            if (isDiceSpinning[i] && !isDiceLocked[i] && (diceSpinTimeLeft[i] -= Time.deltaTime) <= 0)
                StopDice(i);
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� ������ Spin (����� ����� ������� ������)
    /// </summary>
    public virtual void OnSpinButtonClick()
    {
        // ������ ��� ��������� Spin, ���� �������� �� �������� � ����� ����� ������
        if (currentState == GameState.Spin && !isSpinCoroutineActive && CanTakeTurn())
        {
            hapticManager.TriggerHaptic("light");
            currentTurnPoints = 0;
            isSpinCoroutineActive = true; // ����� ������������� ����
            StartCoroutine(SpinCoroutine(false));
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� ������ End (����� ����� ��������� ��� ��������)
    /// </summary>
    public virtual void OnEndButtonClick()
    {
        if (isSpinCoroutineActive) return; // �� ���������, ���� �������� ��������
        if (!CanTakeTurn()) return; // ���������, ����� �� ����� ������� ���
        if (currentState == GameState.Spin || currentState == GameState.AfterSpin || currentState == GameState.End)
        {
            EndTurn();
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� ������ Re-spin (�������� �������, ���� ������� �����-����)
    /// </summary>
    public virtual void OnRespinButtonClick()
    {
        if (!CanTakeTurn()) return;
        if (isSpinCoroutineActive) return; // ���� �������� ��� ��������, ������ �� ������

        if (currentState == GameState.AfterSpin && IsAnyDiceLocked())
        {
            for (int i = 0; i < lockButtons.Length && i < 5; i++)
            {
                if (arrowHintImages[i] != null)
                    arrowHintImages[i].SetActive(false);
            }
            hapticManager.TriggerHaptic("light");
            currentState = GameState.ReSpin;
            isSpinCoroutineActive = true; // ����� ������������� ����
            StartCoroutine(SpinCoroutine(true));
        }
        else if (currentState == GameState.ReSpin)
        {
            EndTurn();
        }
    }

    /// <summary>
    /// ���������� ��� ������� �� ������ Locked (��� ���� � ������ ����������)
    /// </summary>
    public virtual void OnLockedButtonClick()
    {
        // ������ �� ������ � ������ ���� ����������� ������, ��� ������ ��� ����.
    }

    protected virtual IEnumerator SpinCoroutine(bool isReSpin)
    {
        ToggleInteractables(false);

        for (int i = 0; i < 5; i++)
        {
            if (!isDiceLocked[i])
            {
                isDiceSpinning[i] = true;
                diceSpinTimeLeft[i] = Random.Range(spinDurationMin, spinDurationMax) + (i * stopDelay);
                diceResultImages[i].enabled = false;
                yield return new WaitForSeconds(Random.value * 0.3f);
                diceAnimators[i].SetBool("Spin", true);
            }
            else
            {
                isDiceSpinning[i] = false;
            }
        }

        yield return new WaitUntil(() => !IsAnyDiceSpinning());

        var (reward, winningFaces) = CalculateReward(currentFaces);
        currentTurnPoints = reward;
        UpdateInfoPanel(currentFaces, reward, winningFaces);

        currentState = isReSpin ? GameState.End : GameState.AfterSpin;
        ToggleInteractables(true);
        UpdateButtonState();

        isSpinCoroutineActive = false; // �������� ��������� � ������� ����
    }

    protected virtual void StopDice(int diceIndex)
    {
        isDiceSpinning[diceIndex] = false;
        diceAnimators[diceIndex].SetBool("Spin", false);
        currentFaces[diceIndex] = Random.Range(1, 7);
        diceResultImages[diceIndex].sprite = diceSprites[currentFaces[diceIndex] - 1];
        diceResultImages[diceIndex].enabled = true;
        hapticManager?.TriggerHaptic("light");
    }

    protected (int reward, HashSet<int> winningFaces) CalculateReward(int[] results)
    {
        int[] faceCount = new int[6];
        foreach (int face in results)
            faceCount[face - 1]++;

        int reward = 0;
        var winningFaces = new HashSet<int>();

        if (IsStraight(results, 0, 4))
            return (1000, new HashSet<int> { 1, 2, 3, 4, 5 });
        if (IsStraight(results, 1, 5))
            return (1500, new HashSet<int> { 2, 3, 4, 5, 6 });

        for (int face = 0; face < 6; face++)
        {
            int count = faceCount[face], points = face switch
            {
                0 => count switch { 1 => 100, 2 => 200, 3 => 1000, 4 => 2000, 5 => 4000, _ => 0 },
                4 => count switch { 1 => 50, 2 => 100, 3 => 500, 4 => 1000, 5 => 2000, _ => 0 },
                _ => count switch { 3 => (face + 1) * 100, 4 => (face + 1) * 200, 5 => (face + 1) * 400, _ => 0 }
            };
            if (points > 0)
            {
                reward += points;
                winningFaces.Add(face + 1);
            }
        }

        return (reward, winningFaces);
    }

    protected bool IsStraight(int[] results, int minIdx, int maxIdx)
    {
        var uniqueFaces = new HashSet<int>(results);
        return uniqueFaces.Count == 5 && uniqueFaces.Min() == minIdx + 1 && uniqueFaces.Max() == maxIdx + 1;
    }

    protected void UpdateInfoPanel(int[] results, int reward, HashSet<int> winningFaces)
    {
        for (int i = 0; i < 5; i++)
        {
            combinationImages[i].sprite = faceSprites[results[i] - 1];
            combinationImages[i].color = winningFaces.Contains(results[i]) ? winningColor : nonWinningColor;
        }
        rewardText.text = reward.ToString();
    }

    protected void ClearInfoPanel()
    {
        for (int i = 0; i < 5; i++)
        {
            combinationImages[i].color = winningColor;
            combinationImages[i].sprite = faceSprites[6];
        }
            
        rewardText.text = "***";
    }

    protected void UpdateBalanceText()
    {
        hapticManager?.TriggerHaptic("medium");
        balanceText.text = $"{totalBalance}";
        for (int i = 0; i < lockButtons.Length && i < 5; i++)
        {
            if (lockerImages[i] != null)
            {
                lockerImages[i].SetActive(false);
                if (arrowHintImages[i] != null)
                {
                    arrowHintImages[i].SetActive(false);
                }    
            }
        }
    }

    public void ToggleLock(int diceIndex)
    {
        if (currentState != GameState.AfterSpin || IsAnyDiceSpinning() || diceIndex < 0 || diceIndex >= 5)
            return;

        isDiceLocked[diceIndex] = !isDiceLocked[diceIndex];
        if (lockButtons != null && diceIndex < lockButtons.Length && lockButtons[diceIndex] != null)
        {
            lockButtons[diceIndex].interactable = !isDiceLocked[diceIndex];
            lockerImages[diceIndex].SetActive(true);
            if (arrowHintImages[diceIndex] != null)
            {
                arrowHintImages[diceIndex].SetActive(false);
            }
            hapticManager?.TriggerHaptic("light");
        }

        UpdateButtonState();
    }

    protected void UpdateButtonState()
    {
        // ������� �������� ��� ������
        spin_button.gameObject.SetActive(false);
        end_button.gameObject.SetActive(false);
        respin_button.gameObject.SetActive(false);
        locked_button.gameObject.SetActive(false);

        // ���� ��� ������ ����������, ���������� ������ Locked
        if (!CanTakeTurn())
        {
            locked_button.gameObject.SetActive(true);
            return;
        }

        // � ����������� �� ��������� ���� ���������� ��������������� ������
        switch (currentState)
        {
            case GameState.Spin:
                blink_sprite.SetParent(spin_button.gameObject.transform);
                spin_button.gameObject.SetActive(true);
                break;
            case GameState.AfterSpin:
                if (IsAnyDiceLocked())
                {
                    respin_button.gameObject.SetActive(true);
                    blink_sprite.SetParent(respin_button.gameObject.transform);
                }
                else
                {
                    end_button.gameObject.SetActive(true);
                    blink_sprite.SetParent(end_button.gameObject.transform);
                    for (int i = 0; i < lockButtons.Length && i < 5; i++)
                    {
                        if (arrowHintImages[i] != null)
                            arrowHintImages[i].SetActive(true);
                    }
                }
                break;
            case GameState.ReSpin:
                respin_button.gameObject.SetActive(true);
                blink_sprite.SetParent(respin_button.gameObject.transform);
                break;
            case GameState.End:
                end_button.gameObject.SetActive(true);
                blink_sprite.SetParent(end_button.gameObject.transform);
                break;
            default:
                break;
        }
    }

    protected bool IsAnyDiceLocked() => System.Array.Exists(isDiceLocked, locked => locked);

    protected void ResetLocks() => System.Array.Clear(isDiceLocked, 0, 5);

    protected bool IsAnyDiceSpinning() => System.Array.Exists(isDiceSpinning, spinning => spinning);

    public int GetCurrentFace(int diceIndex) => (diceIndex >= 0 && diceIndex < 5) ? currentFaces[diceIndex] : -1;

    public int GetTotalBalance() => totalBalance;

    public GameState GetCurrentState() => currentState;

    public abstract bool CanTakeTurn();

    protected abstract void OnTurnEnd(int balance);

    private void ValidateComponents()
    {
        if (diceAnimators.Length != 5 || diceResultImages.Length != 5 || combinationImages.Length != 5 ||
            faceSprites.Length != 7 || diceSprites.Length != 6 || rewardText == null || balanceText == null || gameManager == null)
            Debug.LogError($"{GetType().Name}: Please assign all required components in the inspector!");
    }

    private void InitializeUI()
    {
        for (int i = 0; i < 5; i++)
            diceResultImages[i].enabled = false;
        UpdateButtonState();
        UpdateBalanceText();
        ClearInfoPanel();
    }

    private void ToggleInteractables(bool enabled)
    {
        if (lockButtons != null)
            for (int i = 0; i < lockButtons.Length && i < 5; i++)
            {
                if (lockButtons[i] != null)
                {
                    lockButtons[i].interactable = enabled;
                }
            }
               
    }

    public void EndTurn()
    {
        for (int i = 0; i < lockButtons.Length && i < 5; i++)
        {
            if (arrowHintImages[i] != null)
                arrowHintImages[i].SetActive(false);
        }
        hapticManager.TriggerHaptic("light");
        totalBalance += currentTurnPoints;
        currentTurnPoints = 0;
        UpdateBalanceText();
        currentState = GameState.Spin;
        ResetLocks();
        UpdateButtonState();
        Debug.Log("EndTurn called, invoking OnTurnEnd");
        OnTurnEnd(totalBalance);
    }

    public virtual void RestartGame()
    {
        hapticManager.TriggerHaptic("heavy");
        StopAllCoroutines(); // ������������� ��� ��������
        isSpinCoroutineActive = false;

        // ���������� ��������� �������
        System.Array.Clear(isDiceSpinning, 0, 5);
        System.Array.Clear(diceSpinTimeLeft, 0, 5);
        System.Array.Clear(isDiceLocked, 0, 5);
        System.Array.Clear(currentFaces, 0, 5);

        // ���������� ���� � ���������
        currentTurnPoints = 0;
        totalBalance = 0; // ����������, ��� ������ ������������
        currentState = GameState.Spin;

        // ������������������ UI � ������
        InitializeUI();
    }
}
