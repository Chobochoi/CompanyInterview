using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject characterCreationPanel;
    public GameObject loadGamePanel;

    [Header("Main Menu Buttons")]
    public Button startGameButton;
    public Button loadGameButton;
    public Button quitGameButton;

    [Header("Character Creation UI")]
    public TMP_InputField nicknameInput;
    public Toggle maleToggle;
    public Toggle femaleToggle;
    public Button[] skinColorButtons;
    public Button[] hairStyleButtons;
    public Button[] hairColorButtons;
    public Button[] eyeColorButtons;
    public Button[] outfitButtons;
    public Button createCharacterButton;
    public Button cancelCreationButton;

    [Header("Load Game UI")]
    public Button[] loadSlotButtons;
    public TextMeshProUGUI[] slotInfoTexts;
    public Button[] deleteSlotButtons;
    public Button backToMenuButton;

    [Header("Character Preview")]
    public RawImage characterPreview;

    private int selectedSkinColor = 0;
    private int selectedHairStyle = 0;
    private int selectedHairColor = 0;
    private int selectedEyeColor = 0;
    private int selectedOutfit = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SetupUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ShowMainMenu();
    }

    void SetupUI()
    {
        // Main Menu ��ư �̺�Ʈ ����
        startGameButton.onClick.AddListener(ShowCharacterCreation);
        loadGameButton.onClick.AddListener(ShowLoadGame);
        quitGameButton.onClick.AddListener(QuitGame);

        // Character Creation ��ư �̺�Ʈ ����
        createCharacterButton.onClick.AddListener(CreateCharacter);
        cancelCreationButton.onClick.AddListener(ShowMainMenu);

        // Gender ��� ����
        maleToggle.onValueChanged.AddListener(OnGenderChanged);
        femaleToggle.onValueChanged.AddListener(OnGenderChanged);

        // Customization ��ư�� ����
        SetupCustomizationButtons();

        // Load Game ��ư ����
        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            int slotIndex = i; // Ŭ���� ���� �ذ�
            loadSlotButtons[i].onClick.AddListener(() => LoadGame(slotIndex));
            deleteSlotButtons[i].onClick.AddListener(() => DeleteSaveSlot(slotIndex));
        }

        backToMenuButton.onClick.AddListener(ShowMainMenu);
    }

    void SetupCustomizationButtons()
    {
        // Skin Color ��ư��
        for (int i = 0; i < skinColorButtons.Length; i++)
        {
            int index = i;
            skinColorButtons[i].onClick.AddListener(() => SelectSkinColor(index));
        }

        // Hair Style ��ư��
        for (int i = 0; i < hairStyleButtons.Length; i++)
        {
            int index = i;
            hairStyleButtons[i].onClick.AddListener(() => SelectHairStyle(index));
        }

        // Hair Color ��ư��
        for (int i = 0; i < hairColorButtons.Length; i++)
        {
            int index = i;
            hairColorButtons[i].onClick.AddListener(() => SelectHairColor(index));
        }

        // Eye Color ��ư��
        for (int i = 0; i < eyeColorButtons.Length; i++)
        {
            int index = i;
            eyeColorButtons[i].onClick.AddListener(() => SelectEyeColor(index));
        }

        // Outfit ��ư��
        for (int i = 0; i < outfitButtons.Length; i++)
        {
            int index = i;
            outfitButtons[i].onClick.AddListener(() => SelectOutfit(index));
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        characterCreationPanel.SetActive(false);
        loadGamePanel.SetActive(false);
    }

    public void ShowCharacterCreation()
    {
        mainMenuPanel.SetActive(false);
        characterCreationPanel.SetActive(true);
        loadGamePanel.SetActive(false);

        // ĳ���� ���� UI �ʱ�ȭ
        ResetCharacterCreation();
    }

    public void ShowLoadGame()
    {
        mainMenuPanel.SetActive(false);
        characterCreationPanel.SetActive(false);
        loadGamePanel.SetActive(true);

        // ����� ���� ���� ������Ʈ
        UpdateLoadGameUI();
    }

    void ResetCharacterCreation()
    {
        nicknameInput.text = "";
        maleToggle.isOn = true;
        femaleToggle.isOn = false;

        selectedSkinColor = 0;
        selectedHairStyle = 0;
        selectedHairColor = 0;
        selectedEyeColor = 0;
        selectedOutfit = 0;

        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void UpdateLoadGameUI()
    {
        PlayerData[] saveSlots = SaveLoadManager.Instance.GetAllSaveSlots();

        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            if (i < saveSlots.Length)
            {
                slotInfoTexts[i].text = saveSlots[i].GetDisplayInfo();
                loadSlotButtons[i].interactable = !saveSlots[i].IsEmpty();
                deleteSlotButtons[i].interactable = !saveSlots[i].IsEmpty();
            }
        }
    }

    void OnGenderChanged(bool value)
    {
        if (maleToggle.isOn && femaleToggle.isOn)
        {
            // �� �� ���õ� ���, ���߿� ���õ� �͸� ����
            if (value && maleToggle.isOn)
            {
                femaleToggle.isOn = false;
            }
            else if (value && femaleToggle.isOn)
            {
                maleToggle.isOn = false;
            }
        }

        UpdateCharacterPreview();
    }

    void SelectSkinColor(int index)
    {
        selectedSkinColor = index;
        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void SelectHairStyle(int index)
    {
        selectedHairStyle = index;
        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void SelectHairColor(int index)
    {
        selectedHairColor = index;
        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void SelectEyeColor(int index)
    {
        selectedEyeColor = index;
        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void SelectOutfit(int index)
    {
        selectedOutfit = index;
        UpdateCharacterPreview();
        UpdateCustomizationButtons();
    }

    void UpdateCharacterPreview()
    {
        // ĳ���� �̸����� ������Ʈ
        // �����δ� 3D ���̳� �̹����� �����ؼ� ������ �� �ֽ��ϴ�
        Debug.Log($"ĳ���� �̸����� ������Ʈ: ����={GetSelectedGender()}, �Ǻ�={selectedSkinColor}, ���={selectedHairStyle}");
    }

    void UpdateCustomizationButtons()
    {
        // ���õ� ��ư���� �ð��� �ǵ�� ������Ʈ
        UpdateButtonSelection(skinColorButtons, selectedSkinColor);
        UpdateButtonSelection(hairStyleButtons, selectedHairStyle);
        UpdateButtonSelection(hairColorButtons, selectedHairColor);
        UpdateButtonSelection(eyeColorButtons, selectedEyeColor);
        UpdateButtonSelection(outfitButtons, selectedOutfit);
    }

    void UpdateButtonSelection(Button[] buttons, int selectedIndex)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            ColorBlock colors = buttons[i].colors;
            if (i == selectedIndex)
            {
                colors.normalColor = Color.yellow;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            buttons[i].colors = colors;
        }
    }

    Gender GetSelectedGender()
    {
        return maleToggle.isOn ? Gender.Male : Gender.Female;
    }

    void CreateCharacter()
    {
        if (string.IsNullOrEmpty(nicknameInput.text.Trim()))
        {
            Debug.LogWarning("�г����� �Է����ּ���!");
            return;
        }

        // �� �÷��̾� ������ ����
        PlayerData newPlayerData = new PlayerData();
        newPlayerData.nickname = nicknameInput.text.Trim();
        newPlayerData.gender = GetSelectedGender();
        newPlayerData.skinColorIndex = selectedSkinColor;
        newPlayerData.hairStyleIndex = selectedHairStyle;
        newPlayerData.hairColorIndex = selectedHairColor;
        newPlayerData.eyeColorIndex = selectedEyeColor;
        newPlayerData.outfitIndex = selectedOutfit;

        // �� ���� ã��
        int availableSlot = FindAvailableSlot();
        if (availableSlot == -1)
        {
            Debug.LogWarning("��� ������ ���� ������ �����ϴ�!");
            return;
        }

        // ������ ����
        SaveLoadManager.Instance.SavePlayerData(newPlayerData, availableSlot);

        // ���� ����
        GameManager.Instance.StartNewGame(newPlayerData, availableSlot);
    }

    int FindAvailableSlot()
    {
        PlayerData[] saveSlots = SaveLoadManager.Instance.GetAllSaveSlots();
        for (int i = 0; i < saveSlots.Length; i++)
        {
            if (saveSlots[i].IsEmpty())
            {
                return i;
            }
        }
        return -1; // �� ���� ����
    }

    void LoadGame(int slotIndex)
    {
        PlayerData playerData = SaveLoadManager.Instance.LoadPlayerData(slotIndex);
        if (playerData != null && !playerData.IsEmpty())
        {
            GameManager.Instance.LoadGame(playerData, slotIndex);
        }
        else
        {
            Debug.LogWarning("�ε��� �����Ͱ� �����ϴ�!");
        }
    }

    void DeleteSaveSlot(int slotIndex)
    {
        if (SaveLoadManager.Instance.HasSaveData(slotIndex))
        {
            SaveLoadManager.Instance.DeleteSaveData(slotIndex);
            UpdateLoadGameUI();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}