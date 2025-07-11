using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    [System.Serializable]
    public class DialogueNode
    {
        public int id;
        public string speakerName;
        [TextArea(3, 5)]
        public string dialogueText;
        public Texture2D characterTexture; // Sprite → Texture2D로 변경
        public Choice[] choices;
        public int nextNodeId = -1; // -1이면 대화 종료
    }

    [System.Serializable]
    public class Choice
    {
        [TextArea(2, 3)]
        public string choiceText;
        public int nextNodeId;
        public int scoreChange; // 면접 점수 변화
    }

    public DialogueNode[] nodes;
}