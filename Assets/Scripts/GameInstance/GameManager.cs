using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Data")]
    public PlayerData currentPlayerData;
    public int currentSaveSlot = -1;

    [Header("Scene Names")]
    public string mainMenuSceneName = "Intro";
    public string gameSceneName = "Main";

    private float gameStartTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameStartTime = Time.time;
    }

    void Update()
    {
        // �÷��� �ð� ������Ʈ
        if (currentPlayerData != null)
        {
            currentPlayerData.playTime += Time.deltaTime;
        }
    }

    public void StartNewGame(PlayerData playerData, int slotIndex)
    {
        currentPlayerData = playerData;
        currentSaveSlot = slotIndex;

        Debug.Log($"�� ���� ����: {playerData.nickname}");

        // ���� ������ ��ȯ
        LoadGameScene();
    }

    public void LoadGame(PlayerData playerData, int slotIndex)
    {
        currentPlayerData = playerData;
        currentSaveSlot = slotIndex;

        Debug.Log($"���� �ε�: {playerData.nickname}");

        // ����� ������ ��ȯ
        LoadGameScene();
    }

    void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void SaveCurrentGame()
    {
        if (currentPlayerData != null && currentSaveSlot != -1)
        {
            SaveLoadManager.Instance.SavePlayerData(currentPlayerData, currentSaveSlot);
            Debug.Log("���� ���� �Ϸ�!");
        }
        else
        {
            Debug.LogWarning("������ �����Ͱ� �����ϴ�!");
        }
    }

    public void ReturnToMainMenu()
    {
        currentPlayerData = null;
        currentSaveSlot = -1;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        SaveCurrentGame(); // ���� �� �ڵ� ����

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveCurrentGame(); // ���� �Ͻ� ������ �� �ڵ� ����
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveCurrentGame(); // ���� ��Ŀ���� ���� �� �ڵ� ����
        }
    }
}