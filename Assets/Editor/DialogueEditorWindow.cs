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

        // DialogueData 선택
        currentDialogue = (DialogueData)EditorGUILayout.ObjectField("Dialogue Data", currentDialogue, typeof(DialogueData), false);

        if (currentDialogue == null)
        {
            EditorGUILayout.HelpBox("DialogueData를 선택하세요", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();

        // 노드 리스트와 상세 정보를 나란히 표시
        EditorGUILayout.BeginHorizontal();

        // 왼쪽: 노드 리스트
        EditorGUILayout.BeginVertical("box", GUILayout.Width(200));
        DrawNodeList();
        EditorGUILayout.EndVertical();

        // 오른쪽: 선택된 노드 상세 정보
        EditorGUILayout.BeginVertical("box");
        DrawNodeDetails();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void DrawNodeList()
    {
        EditorGUILayout.LabelField("노드 목록", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (int i = 0; i < currentDialogue.nodes.Length; i++)
        {
            var node = currentDialogue.nodes[i];

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            if (selectedNodeIndex == i)
            {
                buttonStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn_on.png") as Texture2D;
            }

            if (GUILayout.Button($"노드 {node.id}\n{node.speakerName}", buttonStyle, GUILayout.Height(50)))
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
            EditorGUILayout.HelpBox("노드를 선택하세요", MessageType.Info);
            return;
        }

        var node = currentDialogue.nodes[selectedNodeIndex];

        EditorGUILayout.LabelField($"노드 {node.id} 상세 정보", EditorStyles.boldLabel);

        EditorGUILayout.LabelField("화자:", node.speakerName);
        EditorGUILayout.LabelField("대사:");
        EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(80));

        if (node.choices != null && node.choices.Length > 0)
        {
            EditorGUILayout.LabelField("선택지:");
            for (int i = 0; i < node.choices.Length; i++)
            {
                var choice = node.choices[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"선택지 {i + 1}:");
                EditorGUILayout.TextArea(choice.choiceText, GUILayout.Height(30));
                EditorGUILayout.LabelField($"다음 노드: {choice.nextNodeId}, 점수: {choice.scoreChange}");
                EditorGUILayout.EndVertical();
            }
        }
    }
}