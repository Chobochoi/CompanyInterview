using UnityEngine;
using UnityEngine.UI;

public class InterviewStarter : MonoBehaviour
{
    [Header("Dialogue Data")]
    public DialogueData interviewDialogue; // Inspector에서 할당

    [Header("UI References")]
    public Button startInterviewButton; // 면접 시작 버튼

    /// <summary>
    /// 컴포넌트 초기화 시 버튼 이벤트 등록
    /// </summary>
    void Start()
    {
        // 버튼 클릭 이벤트 등록
        if (startInterviewButton != null)
        {
            startInterviewButton.onClick.AddListener(StartInterview);
        }
    }

    /// <summary>
    /// 면접 대화를 시작하는 함수
    /// </summary>
    public void StartInterview()
    {
        if (interviewDialogue != null && DialogueManager.Instance != null)
        {
            // 이미 대화가 진행 중이면 시작하지 않음
            if (!DialogueManager.Instance.IsDialogueActive())
            {
                DialogueManager.Instance.StartDialogue(interviewDialogue);

                // 시작 버튼 비활성화 (선택사항)
                if (startInterviewButton != null)
                {
                    startInterviewButton.interactable = false;
                }
            }
        }
        else
        {
            Debug.LogError("DialogueData 또는 DialogueManager가 없습니다!");
        }
    }

    /// <summary>
    /// 대화 종료 후 버튼 다시 활성화하는 함수
    /// </summary>
    public void OnDialogueEnd()
    {
        if (startInterviewButton != null)
        {
            startInterviewButton.interactable = true;
        }
    }
}