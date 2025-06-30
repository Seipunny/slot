using UnityEngine;

public class WallController : MonoBehaviour
{
    [Header("Настройки стены")]
    public float bounceForce = 5f;
    public float friction = 0.8f;
    public bool isDestructive = false;
    
    [Header("Физические настройки")]
    public float restitution = 0.7f;
    public float wallFriction = 0.4f;
    public bool isStatic = true;
    
    [Header("Эффекты")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private AudioSource audioSource;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        // Настраиваем физический материал для стены
        if (boxCollider != null)
        {
            PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D();
            physicsMaterial.friction = wallFriction;
            physicsMaterial.bounciness = restitution;
            boxCollider.sharedMaterial = physicsMaterial;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("dice"))
        {
            HandleDiceCollision(collision);
        }
    }
    
    void HandleDiceCollision(Collision2D collision)
    {
        Rigidbody2D diceRb = collision.gameObject.GetComponent<Rigidbody2D>();
        
        if (diceRb != null)
        {
            // Вычисляем нормаль столкновения
            Vector2 normal = collision.contacts[0].normal;
            Vector2 contactPoint = collision.contacts[0].point;
            
            // Применяем отскок с учетом физического материала
            Vector2 bounceVelocity = Vector2.Reflect(diceRb.linearVelocity, normal);
            diceRb.linearVelocity = bounceVelocity * friction;
            
            // Добавляем дополнительный импульс отскока
            diceRb.AddForce(normal * bounceForce, ForceMode2D.Impulse);
            
            // Добавляем небольшой случайный импульс для разнообразия
            Vector2 randomImpulse = new Vector2(
                Random.Range(-0.3f, 0.3f),
                Random.Range(0.1f, 0.5f)
            );
            diceRb.AddForce(randomImpulse, ForceMode2D.Impulse);
            
            // Предотвращаем застревание кубиков в стенах
            if (diceRb.linearVelocity.magnitude < 1f)
            {
                diceRb.linearVelocity = normal * 2f;
            }
        }
        
        // Воспроизводим звук
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
        
        // Если стена разрушительная, уничтожаем кубик
        if (isDestructive)
        {
            DiceController diceController = collision.gameObject.GetComponent<DiceController>();
            if (diceController != null)
            {
                diceController.DestroyDice();
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }
    
    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = isDestructive ? Color.red : Color.blue;
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
    
    void OnDrawGizmos()
    {
        // Показываем стены всегда для удобства
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
            Gizmos.DrawCube(transform.position, boxCollider.size);
        }
    }
} 