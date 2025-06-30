using UnityEngine;

public class PhysicsSettings : MonoBehaviour
{
    [Header("Глобальные настройки физики")]
    public float timeScale = 1f;
    public int velocityIterations = 8;
    public int positionIterations = 3;
    public float defaultGravity = -9.81f;
    
    [Header("Настройки коллизий")]
    public bool autoSimulation = true;
    public float fixedTimeStep = 0.02f;
    public int maxSubSteps = 5;
    
    [Header("Настройки снапинга")]
    public bool enableSleeping = true;
    public float sleepThreshold = 0.005f;
    public float defaultContactOffset = 0.01f;
    
    void Start()
    {
        ApplyPhysicsSettings();
    }
    
    void ApplyPhysicsSettings()
    {
        // Настройки времени
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = fixedTimeStep;
        
        Physics2D.velocityIterations = velocityIterations;
        Physics2D.positionIterations = positionIterations;
        Physics2D.gravity = new Vector2(0, defaultGravity);
        
        Physics2D.defaultContactOffset = defaultContactOffset;
        
        
        Debug.Log("Настройки физики применены: " +
                  "Velocity Iterations: " + velocityIterations + ", " +
                  "Position Iterations: " + positionIterations + ", " +
                  "Fixed Time Step: " + fixedTimeStep);
    }
    
    void OnValidate()
    {
        // Применяем настройки в редакторе
        if (Application.isPlaying)
        {
            ApplyPhysicsSettings();
        }
    }
    
    // Методы для динамического изменения настроек
    public void SetTimeScale(float newTimeScale)
    {
        timeScale = newTimeScale;
        Time.timeScale = timeScale;
    }
    
    public void SetGravity(float newGravity)
    {
        defaultGravity = newGravity;
        Physics2D.gravity = new Vector2(0, defaultGravity);
    }
    
    public void SetVelocityIterations(int iterations)
    {
        velocityIterations = Mathf.Clamp(iterations, 1, 20);
        Physics2D.velocityIterations = velocityIterations;
    }
    
    public void SetPositionIterations(int iterations)
    {
        positionIterations = Mathf.Clamp(iterations, 1, 10);
        Physics2D.positionIterations = positionIterations;
    }
    
    // Визуализация в редакторе
    void OnDrawGizmos()
    {
        // Показываем направление гравитации
        Gizmos.color = Color.yellow;
        Vector3 gravityVector = new Vector3(0, defaultGravity * 0.1f, 0);
        Gizmos.DrawRay(transform.position, gravityVector);
        
        // Показываем текущие настройки
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.right * 2, 0.5f);
        }
    }
} 