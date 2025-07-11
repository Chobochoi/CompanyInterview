using UnityEngine;
using UnityEngine.UI;

public class InterviewStarter : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData interviewDialogue; // Inspector���� �Ҵ�

    [Header("UI References")]
    public Button startInterviewButton; // ���� ���� ��ư

    /// <summary>
    /// ������Ʈ �ʱ�ȭ �� ��ư �̺�Ʈ ���
    /// </summary>
    void Start()
    {
        // ��ư Ŭ�� �̺�Ʈ ���
        if (startInterviewButton != null)
        {
            startInterviewButton.onClick.AddListener(StartInterview);
        }
    }

    /// <summary>
    /// ���� ��ȭ�� �����ϴ� �Լ�
    /// </summary>
    public void StartInterview()
    {
        if (interviewDialogue != null && DialogueManager.Instance != null)
        {
            // �̹� ��ȭ�� ���� ���̸� �������� ����
            if (!DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.StartDialogue(interviewDialogue);

                // ���� ��ư ��Ȱ��ȭ (���û���)
                if (startInterviewButton != null)
                {
                    startInterviewButton.interactable = false;
                }
            }
        }
        else
        {
            Debug.LogError("DialogueData �Ǵ� DialogueManager�� �����ϴ�!");
        }
    }

    /// <summary>
    /// ��ȭ ���� �� ��ư �ٽ� Ȱ��ȭ�ϴ� �Լ�
    /// </summary>
    public void OnDialogueEnd()
    {
        if (startInterviewButton != null)
        {
            startInterviewButton.interactable = true;
        }
    }
}