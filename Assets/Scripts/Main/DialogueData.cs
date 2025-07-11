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
        public Texture2D characterTexture; // Sprite �� Texture2D�� ����
        public Choice[] choices;
        public int nextNodeId = -1; // -1�̸� ��ȭ ����
    }

    [System.Serializable]
    public class Choice
    {
        [TextArea(2, 3)]
        public string choiceText;
        public int nextNodeId;
        public int scoreChange; // ���� ���� ��ȭ
    }

    public DialogueNode[] nodes;
}