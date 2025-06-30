using UnityEngine;
using System.Collections.Generic;

public class GapController : MonoBehaviour
{
    [Header("Настройки пропасти")]
    public bool isActive = true;
    public float fallSpeed = 2f;
    public Color gapColor = Color.black;
    
    [Header("Эффекты")]
    public GameObject fallEffect;
    public AudioClip fallSound;
    public ParticleSystem fallParticles;
    
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private PhysxGameManager gameManager;
    private HashSet<GameObject> processedDice = new HashSet<GameObject>();
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Используем синглтон вместо FindObjectOfType
        gameManager = PhysxGameManager.Instance;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = gapColor;
        }
        
        // Настраиваем частицы
        if (fallParticles != null)
        {
            fallParticles.Stop();
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive) return;
        
        if (other.CompareTag("dice") && !processedDice.Contains(other.gameObject))
        {
            HandleDiceFall(other.gameObject);
        }
    }
    
    void HandleDiceFall(GameObject dice)
    {
        // Предотвращаем повторную обработку одного кубика
        if (processedDice.Contains(dice)) return;
        
        processedDice.Add(dice);
        Debug.Log("Кубик попал в пропасть! Уничтожаем: " + dice.name);
        
        // Воспроизводим звук падения
        if (audioSource != null && fallSound != null)
        {
            audioSource.PlayOneShot(fallSound);
        }
        
        // Создаем эффект падения
        if (fallEffect != null)
        {
            Instantiate(fallEffect, dice.transform.position, Quaternion.identity);
        }
        
        // Запускаем частицы
        if (fallParticles != null)
        {
            fallParticles.transform.position = dice.transform.position;
            fallParticles.Play();
        }
        
        // Уведомляем менеджер игры о проигрыше НЕМЕДЛЕННО
        if (gameManager != null)
        {
            gameManager.OnDiceFellInGap(dice);
        }
        
        // Уничтожаем кубик
        DiceController diceController = dice.GetComponent<DiceController>();
        if (diceController != null)
        {
            diceController.DestroyDice();
        }
        else
        {
            Destroy(dice);
        }
        
        // Очищаем из списка обработанных через некоторое время
        StartCoroutine(CleanupProcessedDice(dice));
    }
    
    System.Collections.IEnumerator CleanupProcessedDice(GameObject dice)
    {
        yield return new WaitForSeconds(1f);
        processedDice.Remove(dice);
    }
    
    // Метод для активации/деактивации пропасти
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (spriteRenderer != null)
        {
            Color color = gapColor;
            color.a = active ? 1f : 0.3f;
            spriteRenderer.color = color;
        }
    }
    
    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = isActive ? Color.red : Color.gray;
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
    
    void OnDrawGizmos()
    {
        // Показываем область пропасти всегда
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(transform.position, boxCollider.size);
        }
    }
} 