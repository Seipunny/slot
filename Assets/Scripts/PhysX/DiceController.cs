using UnityEngine;

public class DiceController : MonoBehaviour
{
    [Header("Настройки кубика")]
    public float maxVelocity = 15f;
    public float rotationSpeed = 360f;
    public Color normalColor = Color.white;
    public Color fallingColor = Color.red;
    
    [Header("Физические настройки")]
    public float mass = 1f;
    public float linearDrag = 0.5f;
    public float angularDrag = 0.5f;
    public float gravityScale = 1f;
    public bool freezeRotation = false;
    
    [Header("Эффекты")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isFalling = false;
    private float fallThreshold = -5f;
    private bool isDestroyed = false;
    private PhysxGameManager gameManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Используем синглтон вместо FindObjectOfType
        gameManager = PhysxGameManager.Instance;
        
        // Настраиваем физику для лучшей симуляции
        if (rb != null)
        {
            rb.mass = mass;
            rb.linearDamping = linearDrag;
            rb.angularDamping = angularDrag;
            rb.gravityScale = gravityScale;
            rb.freezeRotation = freezeRotation;
            
            // Устанавливаем тип коллизии для лучшей производительности
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Добавляем случайное вращение при создании
            if (!freezeRotation)
            {
                rb.angularVelocity = Random.Range(-rotationSpeed, rotationSpeed);
            }
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }
    
    void Update()
    {
        if (isDestroyed) return;
        
        // Ограничиваем максимальную скорость
        if (rb != null && rb.linearVelocity.magnitude > maxVelocity)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxVelocity;
        }
        
        // Проверяем, падает ли кубик
        CheckFallingState();
        
        // Вращаем кубик только если не заморожено
        if (!freezeRotation && rb != null)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
    
    void CheckFallingState()
    {
        bool wasFalling = isFalling;
        isFalling = transform.position.y < fallThreshold;
        
        // Изменяем цвет при падении
        if (isFalling != wasFalling && spriteRenderer != null)
        {
            spriteRenderer.color = isFalling ? fallingColor : normalColor;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return;
        
        // Воспроизводим звук удара
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Создаем эффект удара
        if (hitEffect != null)
        {
            Vector2 hitPoint = collision.contacts[0].point;
            Instantiate(hitEffect, hitPoint, Quaternion.identity);
        }
        
        // Добавляем небольшой случайный импульс для более реалистичного поведения
        if (rb != null)
        {
            Vector2 randomImpulse = new Vector2(
                Random.Range(-0.5f, 0.5f),
                Random.Range(0.1f, 0.3f)
            );
            rb.AddForce(randomImpulse, ForceMode2D.Impulse);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;
        
        // Если кубик попал в пропасть
        if (other.CompareTag("Gap"))
        {
            Debug.Log("Кубик попал в пропасть через триггер: " + gameObject.name);
            // Можно добавить специальные эффекты для пропасти
        }
    }
    
    // Метод для принудительного уничтожения кубика
    public void DestroyDice()
    {
        if (isDestroyed) return;
        
        isDestroyed = true;
        Debug.Log("Уничтожаем кубик: " + gameObject.name);
        
        // Создаем эффект уничтожения
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // Уведомляем менеджер игры об уничтожении
        if (gameManager != null)
        {
            gameManager.OnDiceFellInGap(gameObject);
        }
        
        // Уничтожаем объект
        Destroy(gameObject);
    }
    
    void OnDestroy()
    {
        if (!isDestroyed)
        {
            Debug.Log("Кубик уничтожен без вызова DestroyDice: " + gameObject.name);
        }
    }
    
    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>().size);
        
        // Показываем порог падения
        Gizmos.color = Color.red;
        Vector3 fallLine = new Vector3(transform.position.x, fallThreshold, transform.position.z);
        Gizmos.DrawLine(fallLine + Vector3.left * 2, fallLine + Vector3.right * 2);
    }
} 