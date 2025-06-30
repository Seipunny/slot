using UnityEngine;
using System.Collections;

public class CircleController : MonoBehaviour
{
    [Header("Настройки круга")]
    public float impulseForce = 10f;
    public float bounceForce = 5f;
    public Color normalColor = Color.blue;
    public Color hitColor = Color.red;
    public float colorChangeDuration = 0.3f;
    
    [Header("Физические настройки")]
    public float mass = 10f;
    public bool isStatic = true;
    public float restitution = 0.8f;
    public float friction = 0.6f;
    
    [Header("Анимация")]
    public Transform childToAnimate;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.95f, 1, 1f);
    
    private SpriteRenderer spriteRenderer;
    private PhysxGameManager gameManager;
    private float colorChangeTimer = 0f;
    private bool isHit = false;
    private CircleCollider2D circleCollider;
    private Vector3 originalChildScale;
    private Coroutine scaleAnimationCoroutine;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        
        // Оптимизированный поиск менеджера игры
        if (gameManager == null)
        {
            gameManager = PhysxGameManager.Instance;
        }
        
        // Настраиваем физику круга
        if (circleCollider != null)
        {
            // Создаем физический материал для лучшего отскока
            PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D();
            physicsMaterial.friction = friction;
            physicsMaterial.bounciness = restitution;
            circleCollider.sharedMaterial = physicsMaterial;
        }
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
        
        // Сохраняем оригинальный масштаб дочернего объекта
        if (childToAnimate != null)
        {
            originalChildScale = childToAnimate.localScale;
        }
        else
        {
            // Если дочерний объект не назначен, используем сам объект
            childToAnimate = transform;
            originalChildScale = Vector3.one;
        }
    }
    
    void Update()
    {
        // Возвращаем нормальный цвет после удара
        if (isHit)
        {
            colorChangeTimer += Time.deltaTime;
            if (colorChangeTimer >= colorChangeDuration)
            {
                isHit = false;
                colorChangeTimer = 0f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = normalColor;
                }
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("dice"))
        {
            ApplyImpulse(collision.gameObject, collision);
            ChangeColor();
            StartScaleAnimation();
            
            // Уведомляем менеджер игры
            if (gameManager != null)
            {
                gameManager.OnCircleHit(gameObject, collision.gameObject);
            }
        }
    }
    
    void ApplyImpulse(GameObject dice, Collision2D collision)
    {
        Rigidbody2D diceRb = dice.GetComponent<Rigidbody2D>();
        if (diceRb != null)
        {
            // Вычисляем направление от центра круга к кубику
            Vector2 direction = (dice.transform.position - transform.position).normalized;
            
            // Получаем точку контакта для более точного импульса
            Vector2 contactPoint = collision.contacts[0].point;
            Vector2 contactNormal = collision.contacts[0].normal;
            
            // Применяем основной импульс от центра круга
            diceRb.AddForce(direction * impulseForce, ForceMode2D.Impulse);
            
            // Добавляем отскок вверх с учетом нормали контакта
            Vector2 bounceDirection = Vector2.Reflect(contactNormal, Vector2.up);
            diceRb.AddForce(bounceDirection * bounceForce, ForceMode2D.Impulse);
            
            // Добавляем небольшой случайный импульс для разнообразия
            Vector2 randomImpulse = new Vector2(
                Random.Range(-1f, 1f),
                Random.Range(0.5f, 1.5f)
            );
            diceRb.AddForce(randomImpulse, ForceMode2D.Impulse);
            
            // Ограничиваем минимальную скорость для предотвращения застревания
            if (diceRb.linearVelocity.magnitude < 2f)
            {
                diceRb.linearVelocity = direction * 3f;
            }
        }
    }
    
    void ChangeColor()
    {
        isHit = true;
        colorChangeTimer = 0f;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitColor;
        }
    }
    
    void StartScaleAnimation()
    {
        if (scaleAnimationCoroutine != null)
        {
            StopCoroutine(scaleAnimationCoroutine);
        }
        
        scaleAnimationCoroutine = StartCoroutine(ScaleAnimation());
    }
    
    IEnumerator ScaleAnimation()
    {
        float animationDuration = colorChangeDuration * 2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            float progress = elapsedTime / animationDuration;
            float scaleValue = scaleCurve.Evaluate(progress);
            
            if (childToAnimate != null)
            {
                childToAnimate.localScale = originalChildScale * scaleValue;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Возвращаем к оригинальному масштабу
        if (childToAnimate != null)
        {
            childToAnimate.localScale = originalChildScale;
        }
        
        scaleAnimationCoroutine = null;
    }
    
    // Визуализация в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireSphere(transform.position, collider.radius);
        }
        
        // Показываем направление импульса
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector2.up * 2f);
    }
} 