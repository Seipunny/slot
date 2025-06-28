using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public void HapticTriggerLight()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE // Исключаем выполнение в редакторе и на standalone-платформах (ПК)
        Application.ExternalCall("handleHapticFeedbackEvent", "light");
#else
        //Debug.Log("Trigger");
#endif
    }

    public void HapticTriggerMedium()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE // Исключаем выполнение в редакторе и на standalone-платформах (ПК)
        Application.ExternalCall("handleHapticFeedbackEvent", "medium");
#else
        //Debug.Log("Trigger");
#endif
    }

    public void HapticTriggerHeavy()
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE // Исключаем выполнение в редакторе и на standalone-платформах (ПК)
        Application.ExternalCall("handleHapticFeedbackEvent", "heavy");
#else
        //Debug.Log("Trigger");
#endif
    }
}
