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
        // 플레이 시간 업데이트
        if (currentPlayerData != null)
        {
            currentPlayerData.playTime += Time.deltaTime;
        }
    }

    public void StartNewGame(PlayerData playerData, int slotIndex)
    {
        currentPlayerData = playerData;
        currentSaveSlot = slotIndex;

        Debug.Log($"새 게임 시작: {playerData.nickname}");

        // 게임 씬으로 전환
        LoadGameScene();
    }

    public void LoadGame(PlayerData playerData, int slotIndex)
    {
        currentPlayerData = playerData;
        currentSaveSlot = slotIndex;

        Debug.Log($"게임 로드: {playerData.nickname}");

        // 저장된 씬으로 전환
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
            Debug.Log("게임 저장 완료!");
        }
        else
        {
            Debug.LogWarning("저장할 데이터가 없습니다!");
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
        SaveCurrentGame(); // 종료 전 자동 저장

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
            SaveCurrentGame(); // 앱이 일시 정지될 때 자동 저장
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveCurrentGame(); // 앱이 포커스를 잃을 때 자동 저장
        }
    }
}