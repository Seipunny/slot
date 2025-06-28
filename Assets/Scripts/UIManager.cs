using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userIDText, fpsText, serverLogText, versionText, registerErrorText;
    [SerializeField] private GameObject canvasPVP;
    [SerializeField] private GameObject canvasMenu;
    [SerializeField] private Image avatarIcon;

    private float deltaTime;
    private const string BASE_URL = "https://api.dicemastertma.org";
    private const string SECRET_KEY = "adminowneradminowner";
    private string telegramId, username;

    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        Application.targetFrameRate = 60;
        versionText.text = $"Build: {Application.version}";
        registerErrorText.text = "";
        StopPVPPressed();
#if !UNITY_EDITOR && !UNITY_STANDALONE
        Invoke(nameof(CallSyncUserIDEvent), 1f);
        Invoke(nameof(FetchServerData), 1.5f);
#endif
    }

    public void StartPVPPressed()
    {
        canvasPVP.SetActive(true);
        canvasMenu.SetActive(false);
    }

    public void StopPVPPressed()
    {
        canvasMenu.SetActive(true);
    }

    private void CallSyncUserIDEvent() => Application.ExternalCall("handleUnitySyncUserIDEvent");

    public void SetUserID(string id)
    {
        telegramId = id;
    }

    public void SetUsername(string name)
    {
        username = name;
        userIDText.text = $"{telegramId} ({username ?? "unknown"})";
    }

    // Метод для установки аватара из Base64 строки
    public void SetAvatarFromBase64(string base64)
    {
        Debug.Log("Получен Base64 аватара из React");
        try
        {
            // Убираем префикс "data:image/jpeg;base64," или "data:image/png;base64,"
            string base64Data = base64.Split(',')[1];
            byte[] bytes = System.Convert.FromBase64String(base64Data);
            Texture2D texture = new Texture2D(2, 2); // Размер будет автоматически изменен
            texture.LoadImage(bytes); // Загружаем данные изображения

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            avatarIcon.sprite = sprite;
            avatarIcon.gameObject.SetActive(true); // Убеждаемся, что объект активен
            Debug.Log("Аватар успешно установлен из Base64");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Ошибка при обработке Base64 аватара: {e.Message}");
        }
    }

    private void FetchServerData()
    {
        if (string.IsNullOrEmpty(telegramId))
        {
            Debug.LogWarning("TelegramID is not set yet, retrying in 0.5s...");
            Invoke(nameof(FetchServerData), 0.5f);
            return;
        }
        StartCoroutine(GetServerData(telegramId));
    }

    private IEnumerator GetServerData(string telegramId)
    {
        using (var request = UnityWebRequest.Get($"{BASE_URL}/get_client/{telegramId}"))
        {
            request.SetRequestHeader("secret-key", SECRET_KEY);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                HandleServerResponse(request.downloadHandler.text);
            }
            else if (request.responseCode == 404)
            {
                yield return StartCoroutine(RegisterNewUser(telegramId, username));
            }
            else
            {
                UpdateServerLog($"Ошибка получения данных: {request.error} (Код: {request.responseCode})");
            }
        }
    }

    private IEnumerator RegisterNewUser(string telegramId, string username)
    {
        if (!int.TryParse(telegramId, out int parsedTelegramId))
        {
            UpdateRegisterError($"Ошибка: telegramId '{telegramId}' не является числом");
            yield break;
        }

        string effectiveUsername = string.IsNullOrEmpty(username) ? "new_user" : username;
        string payload = $"{{\"telegramid\":{parsedTelegramId},\"username\":\"{effectiveUsername}\",\"coins\":0,\"mmr\":0}}";

        using (var request = new UnityWebRequest($"{BASE_URL}/update_client", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("secret-key", SECRET_KEY);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log($"POST response: {responseText}");
                var response = JsonUtility.FromJson<ServerResponse>(responseText);
                if (response.status == "updated")
                {
                    UpdateServerLog($"Зарегистрирован: {telegramId} ({effectiveUsername})");
                    UpdateRegisterError("+");
                    yield return new WaitForSeconds(0.5f);
                    StartCoroutine(GetServerData(telegramId));
                }
                else
                {
                    UpdateRegisterError($"Неожиданный ответ при регистрации: {responseText}");
                }
            }
            else
            {
                UpdateRegisterError($"Ошибка регистрации: {request.error} (Код: {request.responseCode})\nОтвет: {request.downloadHandler?.text}");
            }
        }
    }

    private void HandleServerResponse(string jsonResponse)
    {
        var userData = JsonUtility.FromJson<UserData>(jsonResponse);
        UpdateServerLog($"User ID: {userData.telegramid}, Username: {userData.username}, " +
                        $"Coins: {userData.coins}, MMR: {userData.mmr}, " +
                        $"Created: {userData.created}, Last Login: {userData.lastlogin ?? "Not set"}");
    }

    private void UpdateServerLog(string message) => serverLogText.text = message;

    private void UpdateRegisterError(string message) => registerErrorText.text = message;

    void Update() => fpsText.text = "FPS: "+((int)(1f / (deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f))).ToString();

    [System.Serializable]
    private class UserData
    {
        public long telegramid;
        public string username;
        public int coins, mmr;
        public string created, lastlogin;
    }

    [System.Serializable]
    private class ServerResponse
    {
        public string status;
        public UserData client;
    }
}