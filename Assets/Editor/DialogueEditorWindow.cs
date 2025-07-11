using UnityEngine;
using UnityEditor;

public class DialogueEditorWindow : EditorWindow
{
    private DialogueData currentDialogue;
    private Vector2 scrollPosition;
    private int selectedNodeIndex = -1;

    [MenuItem("Tools/Dialogue Editor")]
    public static void OpenWindow()
    {
        GetWindow<DialogueEditorWindow>("Dialogue Editor");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Dialogue Editor", EditorStyles.boldLabel);

        // DialogueData ����
        currentDialogue = (DialogueData)EditorGUILayout.ObjectField("Dialogue Data", currentDialogue, typeof(DialogueData), false);

        if (currentDialogue == null)
        {
            EditorGUILayout.HelpBox("DialogueData�� �����ϼ���", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        // ��� ����Ʈ�� �� ������ ������ ǥ��
        EditorGUILayout.BeginHorizontal();

        // ����: ��� ����Ʈ
        EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
        DrawNodeList();
        EditorGUILayout.EndVertical();

        // ������: ���õ� ��� �� ����
        EditorGUILayout.BeginVertical("box");
        DrawNodeDetails();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void DrawNodeList()
    {
        EditorGUILayout.LabelField("��� ���", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < currentDialogue.nodes.Length; i++)
        {
            var node = currentDialogue.nodes[i];

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            if (selectedNodeIndex == i)
            {
                buttonStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn_on.png") as Texture2D;
            }

            if (GUILayout.Button($"��� {node.id}\n{node.speakerName}", buttonStyle, GUILayout.Height(50)))
            {
                selectedNodeIndex = i;
            }
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawNodeDetails()
    {
        if (selectedNodeIndex < 0 || selectedNodeIndex >= currentDialogue.nodes.Length)
        {
            EditorGUILayout.HelpBox("��带 �����ϼ���", MessageType.Info);
            return;
        }

        var node = currentDialogue.nodes[selectedNodeIndex];

        EditorGUILayout.LabelField($"��� {node.id} �� ����", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("ȭ��:", node.speakerName);
        EditorGUILayout.LabelField("���:");
        EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(80));

        if (node.choices != null && node.choices.Length > 0)
        {
            EditorGUILayout.LabelField("������:");
            for (int i = 0; i < node.choices.Length; i++)
            {
                var choice = node.choices[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"������ {i + 1}:");
                EditorGUILayout.TextArea(choice.choiceText, GUILayout.Height(30));
                EditorGUILayout.LabelField($"���� ���: {choice.nextNodeId}, ����: {choice.scoreChange}");
                EditorGUILayout.EndVertical();
            }
        }
    }
}