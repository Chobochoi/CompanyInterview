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
    /// �̱��� �������� DialogueManager �ν��Ͻ� ���� �� ����
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
        // ���� �� ��ȭâ ��Ȱ��ȭ
        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// ��ȭ ���� �Լ� - ���ο� ��ȭ �����ͷ� ��ȭ �ý��� �ʱ�ȭ
    /// </summary>
    /// <param name="dialogue">������ ��ȭ ������</param>
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
    /// ���� ����� ������ UI�� ǥ���ϴ� �Լ�
    /// </summary>
    void DisplayCurrentNode()
    {
        if (currentDialogue == null || currentNodeIndex >= currentDialogue.nodes.Length)
        {
            Debug.LogError("Invalid dialogue or node index!");
            return;
        }

        var node = currentDialogue.nodes[currentNodeIndex];

        // ȭ�� �̸� ������Ʈ
        speakerNameText.text = node.speakerName;

        // ĳ���� �̹��� ������Ʈ
        UpdateCharacterImage(node.characterTexture);

        // ��ȭ �ؽ�Ʈ Ÿ���� �ִϸ��̼�
        StartCoroutine(TypeText(node.dialogueText));

        // ���� ������ ����
        ClearChoices();

        // ������ ���� (Ÿ���� �Ϸ� ��)
        StartCoroutine(ShowChoicesAfterDelay(node));
    }

    /// <summary>
    /// ĳ���� �̹����� ������Ʈ�ϴ� �Լ�
    /// </summary>
    /// <param name="texture">������ �ؽ�ó</param>
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
    /// �ؽ�Ʈ Ÿ���� �ִϸ��̼� �ڷ�ƾ
    /// </summary>
    /// <param name="text">Ÿ������ �ؽ�Ʈ</param>
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
    /// ������ �� �������� ǥ���ϴ� �ڷ�ƾ
    /// </summary>
    /// <param name="node">���� ���</param>
    IEnumerator ShowChoicesAfterDelay(DialogueData.DialogueNode node)
    {
        // Ÿ���� �Ϸ���� ���
        yield return new WaitUntil(() => !isTyping);

        // �߰� ������
        yield return new WaitForSeconds(choiceDelayTime);

        // ������ ����
        if (node.choices != null && node.choices.Length > 0)
        {
            CreateChoiceButtons(node.choices);
        }
        else
        {
            // �������� ������ ���� ���� �ڵ� ���� �Ǵ� ����
            HandleNodeWithoutChoices(node);
        }
    }

    /// <summary>
    /// ���� ������ ��ư���� ��� �����ϴ� �Լ�
    /// </summary>
    void ClearChoices()
    {
        foreach (Transform child in choiceParent)
            Destroy(child.gameObject);
    }

    /// <summary>
    /// ������ ��ư���� �����ϴ� �Լ�
    /// </summary>
    /// <param name="choices">������ ������ �迭</param>
    void CreateChoiceButtons(DialogueData.Choice[] choices)
    {
        for (int i = 0; i < choices.Length; i++)
        {
            CreateChoiceButton(choices[i], i);
        }
    }

    /// <summary>
    /// �������� ���� ��带 ó���ϴ� �Լ� (�ڵ� ���� �Ǵ� ����)
    /// </summary>
    /// <param name="node">ó���� ���</param>
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
    /// ���� ������ ��ư�� �����ϰ� �̺�Ʈ�� �����ϴ� �Լ�
    /// </summary>
    /// <param name="choice">������ ������ ������</param>
    /// <param name="index">������ �ε���</param>
    void CreateChoiceButton(DialogueData.Choice choice, int index)
    {
        GameObject button = Instantiate(choiceButtonPrefab, choiceParent);
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        Button buttonComponent = button.GetComponent<Button>();

        // ��ư �ؽ�Ʈ ����
        buttonText.text = choice.choiceText;

        // ��ư Ŭ�� �̺�Ʈ ���
        buttonComponent.onClick.AddListener(() => {
            OnChoiceSelected(choice, index);
        });

        // �ִϸ��̼� ȿ�� (���û���)
        StartCoroutine(AnimateChoiceButton(button, index));
    }

    /// <summary>
    /// ������ ��ư �ִϸ��̼� �ڷ�ƾ
    /// </summary>
    /// <param name="button">�ִϸ��̼��� ��ư</param>
    /// <param name="index">��ư �ε��� (������ ��)</param>
    IEnumerator AnimateChoiceButton(GameObject button, int index)
    {
        // ��ư �ʱ� ������ ����
        button.transform.localScale = Vector3.zero;

        // �ε����� ���� ������
        yield return new WaitForSeconds(index * 0.1f);

        // ������ �ִϸ��̼�
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
    /// �������� ���õǾ��� �� ó���ϴ� �Լ�
    /// </summary>
    /// <param name="choice">���õ� ������</param>
    /// <param name="index">������ �ε���</param>
    void OnChoiceSelected(DialogueData.Choice choice, int index)
    {
        // ���� ��ȭ ó��
        playerScore += choice.scoreChange;

        Debug.Log($"������ {index + 1} ����: {choice.choiceText}");
        Debug.Log($"���� ��ȭ: {choice.scoreChange}, ���� ����: {playerScore}");

        // ���� ���� �̵�
        MoveToNode(choice.nextNodeId);
    }

    /// <summary>
    /// ��� ��ư�� �����ϴ� �Լ� (�������� ���� �� ���)
    /// </summary>
    /// <param name="nextNodeId">���� ��� ID</param>
    void CreateContinueButton(int nextNodeId)
    {
        GameObject button = Instantiate(choiceButtonPrefab, choiceParent);
        button.GetComponentInChildren<TextMeshProUGUI>().text = "���";

        button.GetComponent<Button>().onClick.AddListener(() => {
            MoveToNode(nextNodeId);
        });
    }

    /// <summary>
    /// ������ ��� ID�� �̵��ϴ� �Լ�
    /// </summary>
    /// <param name="nodeId">�̵��� ��� ID (-1�̸� ��ȭ ����)</param>
    void MoveToNode(int nodeId)
    {
        if (nodeId == -1)
        {
            EndDialogue();
            return;
        }

        // nodeId�� ��� ã��
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
    /// ��ȭ�� �����ϰ� ����� ó���ϴ� �Լ�
    /// </summary>
    void EndDialogue()
    {
        dialoguePanel.SetActive(false);

        // ���� ��� ó��
        ProcessInterviewResult();
    }

    /// <summary>
    /// ���� ����� ������ ���� ó���ϴ� �Լ�
    /// </summary>
    void ProcessInterviewResult()
    {
        Debug.Log($"���� ����! ���� ����: {playerScore}");

        // ������ ���� ��� �б�
        if (playerScore >= 80)
        {
            Debug.Log("�հ�! �����մϴ�!");
        }
        else if (playerScore >= 60)
        {
            Debug.Log("����� ��ȸ�� �־����ϴ�.");
        }
        else
        {
            Debug.Log("���հ��Դϴ�. �ٽ� �����غ�����.");
        }
    }

    /// <summary>
    /// ���� �÷��̾��� ������ ��ȯ�ϴ� �Լ�
    /// </summary>
    /// <returns>���� ����</returns>
    public int GetCurrentScore()
    {
        return playerScore;
    }

    /// <summary>
    /// ��ȭ�� ���� ������ Ȯ���ϴ� �Լ�
    /// </summary>
    /// <returns>��ȭ ���� ����</returns>
    public bool IsDialogueActive()
    {
        return dialoguePanel.activeInHierarchy;
    }
}