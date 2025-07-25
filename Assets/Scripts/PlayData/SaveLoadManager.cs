using UnityEngine;
using System.IO;
using System;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    private const string SAVE_FOLDER = "SaveData";
    private const string SAVE_EXTENSION = ".json";
    private const int MAX_SAVE_SLOTS = 3;

    private PlayerData[] saveSlots = new PlayerData[MAX_SAVE_SLOTS];
    private string saveFolderPath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSaveSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSaveSystem()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }

        LoadAllSlots();
    }

    public void SavePlayerData(PlayerData playerData, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Àß¸øµÈ ½½·Ô ÀÎµ¦½º: {slotIndex}");
            return;
        }

        try
        {
            playerData.lastPlayDate = DateTime.Now;
            string fileName = $"SaveSlot_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(saveFolderPath, fileName);
            string jsonData = JsonUtility.ToJson(playerData, true);

            File.WriteAllText(filePath, jsonData);
            saveSlots[slotIndex] = playerData;

            Debug.Log($"½½·Ô {slotIndex}¿¡ ÀúÀå ¿Ï·á: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ÀúÀå ½ÇÆÐ: {e.Message}");
        }
    }

    public PlayerData LoadPlayerData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Àß¸øµÈ ½½·Ô ÀÎµ¦½º: {slotIndex}");
            return null;
        }

        return saveSlots[slotIndex];
    }

    public void LoadAllSlots()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            try
            {
                string fileName = $"SaveSlot_{i}{SAVE_EXTENSION}";
                string filePath = Path.Combine(saveFolderPath, fileName);

                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    saveSlots[i] = JsonUtility.FromJson<PlayerData>(jsonData);
                }
                else
                {
                    saveSlots[i] = new PlayerData(); // ºó ½½·Ô
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"½½·Ô {i} ·Îµå ½ÇÆÐ: {e.Message}");
                saveSlots[i] = new PlayerData();
            }
        }
    }

    public PlayerData[] GetAllSaveSlots()
    {
        return saveSlots;
    }

    public bool HasSaveData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            return false;

        return saveSlots[slotIndex] != null && !saveSlots[slotIndex].IsEmpty();
    }

    public void DeleteSaveData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            return;

        try
        {
            string fileName = $"SaveSlot_{slotIndex}{SAVE_EXTENSION}";
            string filePath = Path.Combine(saveFolderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            saveSlots[slotIndex] = new PlayerData();
            Debug.Log($"½½·Ô {slotIndex} »èÁ¦ ¿Ï·á");
        }
        catch (Exception e)
        {
            Debug.LogError($"½½·Ô {slotIndex} »èÁ¦ ½ÇÆÐ: {e.Message}");
        }
    }
}