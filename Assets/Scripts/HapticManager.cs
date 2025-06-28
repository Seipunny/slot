using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public void TriggerHaptic(string type)
    {
#if !UNITY_EDITOR && !UNITY_STANDALONE // ��������� ���������� � ��������� � �� standalone-���������� (��)
        Application.ExternalCall("handleHapticFeedbackEvent", type);
#else
        //Debug.Log("Trigger");
#endif
    }
}
