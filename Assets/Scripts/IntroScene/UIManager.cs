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
        // Main Menu 버튼 이벤트 설정
        startGameButton.onClick.AddListener(ShowCharacterCreation);
        loadGameButton.onClick.AddListener(ShowLoadGame);
        quitGameButton.onClick.AddListener(QuitGame);

        // Character Creation 버튼 이벤트 설정
        createCharacterButton.onClick.AddListener(CreateCharacter);
        cancelCreationButton.onClick.AddListener(ShowMainMenu);

        // Gender 토글 설정
        maleToggle.onValueChanged.AddListener(OnGenderChanged);
        femaleToggle.onValueChanged.AddListener(OnGenderChanged);

        // Customization 버튼들 설정
        SetupCustomizationButtons();

        // Load Game 버튼 설정
        for (int i = 0; i < loadSlotButtons.Length; i++)
        {
            int slotIndex = i; // 클로저 문제 해결
            loadSlotButtons[i].onClick.AddListener(() => LoadGame(slotIndex));
            deleteSlotButtons[i].onClick.AddListener(() => DeleteSaveSlot(slotIndex));
        }

        backToMenuButton.onClick.AddListener(ShowMainMenu);
    }

    void SetupCustomizationButtons()
    {
        // Skin Color 버튼들
        for (int i = 0; i < skinColorButtons.Length; i++)
        {
            int index = i;
            skinColorButtons[i].onClick.AddListener(() => SelectSkinColor(index));
        }

        // Hair Style 버튼들
        for (int i = 0; i < hairStyleButtons.Length; i++)
        {
            int index = i;
            hairStyleButtons[i].onClick.AddListener(() => SelectHairStyle(index));
        }

        // Hair Color 버튼들
        for (int i = 0; i < hairColorButtons.Length; i++)
        {
            int index = i;
            hairColorButtons[i].onClick.AddListener(() => SelectHairColor(index));
        }

        // Eye Color 버튼들
        for (int i = 0; i < eyeColorButtons.Length; i++)
        {
            int index = i;
            eyeColorButtons[i].onClick.AddListener(() => SelectEyeColor(index));
        }

        // Outfit 버튼들
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

        // 캐릭터 생성 UI 초기화
        ResetCharacterCreation();
    }

    public void ShowLoadGame()
    {
        mainMenuPanel.SetActive(false);
        characterCreationPanel.SetActive(false);
        loadGamePanel.SetActive(true);

        // 저장된 슬롯 정보 업데이트
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
            // 둘 다 선택된 경우, 나중에 선택된 것만 유지
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
        // 캐릭터 미리보기 업데이트
        // 실제로는 3D 모델이나 이미지를 조합해서 보여줄 수 있습니다
        Debug.Log($"캐릭터 미리보기 업데이트: 성별={GetSelectedGender()}, 피부={selectedSkinColor}, 헤어={selectedHairStyle}");
    }

    void UpdateCustomizationButtons()
    {
        // 선택된 버튼들의 시각적 피드백 업데이트
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
            Debug.LogWarning("닉네임을 입력해주세요!");
            return;
        }

        // 새 플레이어 데이터 생성
        PlayerData newPlayerData = new PlayerData();
        newPlayerData.nickname = nicknameInput.text.Trim();
        newPlayerData.gender = GetSelectedGender();
        newPlayerData.skinColorIndex = selectedSkinColor;
        newPlayerData.hairStyleIndex = selectedHairStyle;
        newPlayerData.hairColorIndex = selectedHairColor;
        newPlayerData.eyeColorIndex = selectedEyeColor;
        newPlayerData.outfitIndex = selectedOutfit;

        // 빈 슬롯 찾기
        int availableSlot = FindAvailableSlot();
        if (availableSlot == -1)
        {
            Debug.LogWarning("사용 가능한 저장 슬롯이 없습니다!");
            return;
        }

        // 데이터 저장
        SaveLoadManager.Instance.SavePlayerData(newPlayerData, availableSlot);

        // 게임 시작
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
        return -1; // 빈 슬롯 없음
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
            Debug.LogWarning("로드할 데이터가 없습니다!");
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