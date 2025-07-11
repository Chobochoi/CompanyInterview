using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public RawImage characterImage;
    public TextMeshProUGUI speakerNameText;
    public TextMeshProUGUI dialogueText;
    public Transform choiceParent;
    public GameObject choiceButtonPrefab;

    [Header("Animation Settings")]
    public float textAnimationSpeed = 0.05f;
    public float choiceDelayTime = 0.5f;

    private DialogueData currentDialogue;
    private int currentNodeIndex = 0;
    private int playerScore = 0;
    private bool isTyping = false;

    public static DialogueManager Instance { get; private set; }

    /// <summary>
    /// 싱글톤 패턴으로 DialogueManager 인스턴스 생성 및 관리
    /// </summary>
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
        // 시작 시 대화창 비활성화
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// 대화 시작 함수 - 새로운 대화 데이터로 대화 시스템 초기화
    /// </summary>
    /// <param name="dialogue">시작할 대화 데이터</param>
    public void StartDialogue(DialogueData dialogue)
    {
        if (dialogue == null)
        {
            Debug.LogError("Dialogue data is null!");
            return;
        }

        currentDialogue = dialogue;
        currentNodeIndex = 0;
        playerScore = 0;

        dialoguePanel.SetActive(true);
        DisplayCurrentNode();
    }

    /// <summary>
    /// 현재 노드의 내용을 UI에 표시하는 함수
    /// </summary>
    void DisplayCurrentNode()
    {
        if (currentDialogue == null || currentNodeIndex >= currentDialogue.nodes.Length)
        {
            Debug.LogError("Invalid dialogue or node index!");
            return;
        }

        var node = currentDialogue.nodes[currentNodeIndex];

        // 화자 이름 업데이트
        speakerNameText.text = node.speakerName;

        // 캐릭터 이미지 업데이트
        UpdateCharacterImage(node.characterTexture);

        // 대화 텍스트 타이핑 애니메이션
        StartCoroutine(TypeText(node.dialogueText));

        // 기존 선택지 제거
        ClearChoices();

        // 선택지 생성 (타이핑 완료 후)
        StartCoroutine(ShowChoicesAfterDelay(node));
    }

    /// <summary>
    /// 캐릭터 이미지를 업데이트하는 함수
    /// </summary>
    /// <param name="texture">설정할 텍스처</param>
    void UpdateCharacterImage(Texture2D texture)
    {
        if (texture != null)
        {
            characterImage.texture = texture;
            characterImage.gameObject.SetActive(true);
        }
        else
        {
            characterImage.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 텍스트 타이핑 애니메이션 코루틴
    /// </summary>
    /// <param name="text">타이핑할 텍스트</param>
    IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textAnimationSpeed);
        }

        isTyping = false;
    }

    /// <summary>
    /// 딜레이 후 선택지를 표시하는 코루틴
    /// </summary>
    /// <param name="node">현재 노드</param>
    IEnumerator ShowChoicesAfterDelay(DialogueData.DialogueNode node)
    {
        // 타이핑 완료까지 대기
        yield return new WaitUntil(() => !isTyping);

        // 추가 딜레이
        yield return new WaitForSeconds(choiceDelayTime);

        // 선택지 생성
        if (node.choices != null && node.choices.Length > 0)
        {
            CreateChoiceButtons(node.choices);
        }
        else
        {
            // 선택지가 없으면 다음 노드로 자동 진행 또는 종료
            HandleNodeWithoutChoices(node);
        }
    }

    /// <summary>
    /// 기존 선택지 버튼들을 모두 제거하는 함수
    /// </summary>
    void ClearChoices()
    {
        foreach (Transform child in choiceParent)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// 선택지 버튼들을 생성하는 함수
    /// </summary>
    /// <param name="choices">생성할 선택지 배열</param>
    void CreateChoiceButtons(DialogueData.Choice[] choices)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            CreateChoiceButton(choices[i], i);
        }
    }

    /// <summary>
    /// 선택지가 없는 노드를 처리하는 함수 (자동 진행 또는 종료)
    /// </summary>
    /// <param name="node">처리할 노드</param>
    void HandleNodeWithoutChoices(DialogueData.DialogueNode node)
    {
        if (node.nextNodeId == -1)
        {
            EndDialogue();
        }
        else
        {
            CreateContinueButton(node.nextNodeId);
        }
    }

    /// <summary>
    /// 개별 선택지 버튼을 생성하고 이벤트를 연결하는 함수
    /// </summary>
    /// <param name="choice">생성할 선택지 데이터</param>
    /// <param name="index">선택지 인덱스</param>
    void CreateChoiceButton(DialogueData.Choice choice, int index)
    {
        GameObject button = Instantiate(choiceButtonPrefab, choiceParent);
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        Button buttonComponent = button.GetComponent<Button>();

        // 버튼 텍스트 설정
        buttonText.text = choice.choiceText;

        // 버튼 클릭 이벤트 등록
        buttonComponent.onClick.AddListener(() => {
            OnChoiceSelected(choice, index);
        });

        // 애니메이션 효과 (선택사항)
        StartCoroutine(AnimateChoiceButton(button, index));
    }

    /// <summary>
    /// 선택지 버튼 애니메이션 코루틴
    /// </summary>
    /// <param name="button">애니메이션할 버튼</param>
    /// <param name="index">버튼 인덱스 (딜레이 용)</param>
    IEnumerator AnimateChoiceButton(GameObject button, int index)
    {
        // 버튼 초기 스케일 설정
        button.transform.localScale = Vector3.zero;

        // 인덱스에 따른 딜레이
        yield return new WaitForSeconds(index * 0.1f);

        // 스케일 애니메이션
        float time = 0;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(0, 1, time / 0.3f);
            button.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        button.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 선택지가 선택되었을 때 처리하는 함수
    /// </summary>
    /// <param name="choice">선택된 선택지</param>
    /// <param name="index">선택지 인덱스</param>
    void OnChoiceSelected(DialogueData.Choice choice, int index)
    {
        // 점수 변화 처리
        playerScore += choice.scoreChange;

        Debug.Log($"선택지 {index + 1} 선택: {choice.choiceText}");
        Debug.Log($"점수 변화: {choice.scoreChange}, 현재 점수: {playerScore}");

        // 다음 노드로 이동
        MoveToNode(choice.nextNodeId);
    }

    /// <summary>
    /// 계속 버튼을 생성하는 함수 (선택지가 없을 때 사용)
    /// </summary>
    /// <param name="nextNodeId">다음 노드 ID</param>
    void CreateContinueButton(int nextNodeId)
    {
        GameObject button = Instantiate(choiceButtonPrefab, choiceParent);
        button.GetComponentInChildren<TextMeshProUGUI>().text = "계속";

        button.GetComponent<Button>().onClick.AddListener(() => {
            MoveToNode(nextNodeId);
        });
    }

    /// <summary>
    /// 지정된 노드 ID로 이동하는 함수
    /// </summary>
    /// <param name="nodeId">이동할 노드 ID (-1이면 대화 종료)</param>
    void MoveToNode(int nodeId)
    {
        if (nodeId == -1)
        {
            EndDialogue();
            return;
        }

        // nodeId로 노드 찾기
        for (int i = 0; i < currentDialogue.nodes.Length; i++)
        {
            if (currentDialogue.nodes[i].id == nodeId)
            {
                currentNodeIndex = i;
                DisplayCurrentNode();
                return;
            }
        }

        Debug.LogError($"Node with ID {nodeId} not found!");
    }

    /// <summary>
    /// 대화를 종료하고 결과를 처리하는 함수
    /// </summary>
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // 면접 결과 처리
        ProcessInterviewResult();
    }

    /// <summary>
    /// 면접 결과를 점수에 따라 처리하는 함수
    /// </summary>
    void ProcessInterviewResult()
    {
        Debug.Log($"면접 종료! 최종 점수: {playerScore}");

        // 점수에 따른 결과 분기
        if (playerScore >= 80)
        {
            Debug.Log("합격! 축하합니다!");
        }
        else if (playerScore >= 60)
        {
            Debug.Log("재면접 기회가 주어집니다.");
        }
        else
        {
            Debug.Log("불합격입니다. 다시 도전해보세요.");
        }
    }

    /// <summary>
    /// 현재 플레이어의 점수를 반환하는 함수
    /// </summary>
    /// <returns>현재 점수</returns>
    public int GetCurrentScore()
    {
        return playerScore;
    }

    /// <summary>
    /// 대화가 진행 중인지 확인하는 함수
    /// </summary>
    /// <returns>대화 진행 여부</returns>
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeInHierarchy;
    }
}