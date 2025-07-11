using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    private ReorderableList nodeList;
    private Vector2 scrollPosition;
    private int selectedNodeIndex = -1;
    private bool showPreview = true;

    // 복사/붙여넣기용
    private DialogueData.DialogueNode copiedNode;
    private DialogueData.Choice copiedChoice;

    void OnEnable()
    {
        SetupNodeList();
    }

    void SetupNodeList()
    {
        var dialogueData = (DialogueData)target;

        nodeList = new ReorderableList(serializedObject, serializedObject.FindProperty("nodes"), true, true, true, true);

        // 헤더 그리기
        nodeList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Dialogue Nodes");
        };

        // 요소 그리기
        nodeList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            var element = nodeList.serializedProperty.GetArrayElementAtIndex(index);
            var node = dialogueData.nodes[index];

            rect.y += 2;
            rect.height = EditorGUIUtility.singleLineHeight;

            string label = $"Node {node.id}: {node.speakerName} - {(string.IsNullOrEmpty(node.dialogueText) ? "No Text" : node.dialogueText.Substring(0, Mathf.Min(30, node.dialogueText.Length)))}";
            if (node.dialogueText.Length > 30) label += "...";

            EditorGUI.LabelField(rect, label);
        };

        // 요소 선택 시
        nodeList.onSelectCallback = (ReorderableList list) =>
        {
            selectedNodeIndex = list.index;
        };

        // 새 요소 추가 시
        nodeList.onAddCallback = (ReorderableList list) =>
        {
            AddNewNode();
        };

        // 요소 제거 시
        nodeList.onRemoveCallback = (ReorderableList list) =>
        {
            RemoveNode(list.index);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DialogueData dialogueData = (DialogueData)target;

        // 헤더
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Data Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // 통계 정보
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"총 노드 수: {dialogueData.nodes.Length}");
        EditorGUILayout.LabelField($"총 선택지 수: {GetTotalChoiceCount(dialogueData)}");
        EditorGUILayout.EndVertical();

        // 미리보기 토글
        showPreview = EditorGUILayout.Toggle("미리보기 표시", showPreview);

        EditorGUILayout.Space();

        // 유틸리티 버튼들
        DrawUtilityButtons(dialogueData);

        EditorGUILayout.Space();

        // 노드 리스트
        nodeList.DoLayoutList();

        EditorGUILayout.Space();

        // 선택된 노드 편집
        if (selectedNodeIndex >= 0 && selectedNodeIndex < dialogueData.nodes.Length)
        {
            DrawNodeEditor(dialogueData, selectedNodeIndex);
        }

        // 미리보기
        if (showPreview)
        {
            EditorGUILayout.Space();
            DrawPreview(dialogueData);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawUtilityButtons(DialogueData dialogueData)
    {
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("새 노드 추가"))
        {
            AddNewNode();
        }

        if (GUILayout.Button("ID 자동 정렬"))
        {
            AutoAssignIDs(dialogueData);
        }

        if (GUILayout.Button("구조 검증"))
        {
            ValidateStructure(dialogueData);
        }

        if (GUILayout.Button("테스트 실행") && Application.isPlaying)
        {
            TestDialogue(dialogueData);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = copiedNode != null;
        if (GUILayout.Button("노드 붙여넣기"))
        {
            PasteNode(dialogueData);
        }
        GUI.enabled = true;

        if (GUILayout.Button("전체 초기화"))
        {
            if (EditorUtility.DisplayDialog("확인", "모든 노드를 삭제하시겠습니까?", "삭제", "취소"))
            {
                ClearAllNodes(dialogueData);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawNodeEditor(DialogueData dialogueData, int index)
    {
        var node = dialogueData.nodes[index];

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"노드 {node.id} 편집", EditorStyles.boldLabel);

        // 노드 조작 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("복사"))
        {
            CopyNode(node);
        }
        if (GUILayout.Button("삭제"))
        {
            RemoveNode(index);
            return;
        }
        if (GUILayout.Button("위로"))
        {
            MoveNode(dialogueData, index, -1);
        }
        if (GUILayout.Button("아래로"))
        {
            MoveNode(dialogueData, index, 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // 기본 정보 편집
        node.id = EditorGUILayout.IntField("ID", node.id);
        node.speakerName = EditorGUILayout.TextField("화자 이름", node.speakerName);

        EditorGUILayout.LabelField("대사 내용:");
        node.dialogueText = EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(60));

        node.characterTexture = (Texture2D)EditorGUILayout.ObjectField("캐릭터 이미지", node.characterTexture, typeof(Texture2D), false);
        node.nextNodeId = EditorGUILayout.IntField("다음 노드 ID (-1: 종료)", node.nextNodeId);

        EditorGUILayout.Space();

        // 선택지 편집
        DrawChoiceEditor(node);

        EditorGUILayout.EndVertical();

        EditorUtility.SetDirty(dialogueData);
    }

    void DrawChoiceEditor(DialogueData.DialogueNode node)
    {
        EditorGUILayout.LabelField("선택지", EditorStyles.boldLabel);

        // 선택지 추가/제거 버튼
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("선택지 추가"))
        {
            AddChoice(node);
        }

        GUI.enabled = copiedChoice != null;
        if (GUILayout.Button("선택지 붙여넣기"))
        {
            PasteChoice(node);
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        if (node.choices == null)
        {
            node.choices = new DialogueData.Choice[0];
        }

        // 선택지 목록
        for (int i = 0; i < node.choices.Length; i++)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"선택지 {i + 1}", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("복사", GUILayout.Width(50)))
            {
                CopyChoice(node.choices[i]);
            }
            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                RemoveChoice(node, i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            node.choices[i].choiceText = EditorGUILayout.TextField("선택지 텍스트", node.choices[i].choiceText);
            node.choices[i].nextNodeId = EditorGUILayout.IntField("다음 노드 ID", node.choices[i].nextNodeId);
            node.choices[i].scoreChange = EditorGUILayout.IntField("점수 변화", node.choices[i].scoreChange);

            EditorGUILayout.EndVertical();
        }
    }

    void DrawPreview(DialogueData dialogueData)
    {
        EditorGUILayout.LabelField("미리보기", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        foreach (var node in dialogueData.nodes)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{node.id}] {node.speakerName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"→ {(node.nextNodeId == -1 ? "END" : node.nextNodeId.ToString())}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(node.dialogueText, EditorStyles.wordWrappedLabel);

            if (node.choices != null && node.choices.Length > 0)
            {
                EditorGUILayout.LabelField("선택지:", EditorStyles.miniBoldLabel);
                for (int i = 0; i < node.choices.Length; i++)
                {
                    var choice = node.choices[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {i + 1}. {choice.choiceText}");
                    EditorGUILayout.LabelField($"→{choice.nextNodeId} ({choice.scoreChange:+0;-0;0})", GUILayout.Width(80));
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    #region Node Operations

    void AddNewNode()
    {
        DialogueData dialogueData = (DialogueData)target;

        // 새 노드 생성
        DialogueData.DialogueNode newNode = new DialogueData.DialogueNode();
        newNode.id = GetNextAvailableID(dialogueData);
        newNode.speakerName = "새 화자";
        newNode.dialogueText = "새로운 대사를 입력하세요.";
        newNode.nextNodeId = -1;
        newNode.choices = new DialogueData.Choice[0];

        // 배열에 추가
        System.Array.Resize(ref dialogueData.nodes, dialogueData.nodes.Length + 1);
        dialogueData.nodes[dialogueData.nodes.Length - 1] = newNode;

        selectedNodeIndex = dialogueData.nodes.Length - 1;

        EditorUtility.SetDirty(dialogueData);
    }

    void RemoveNode(int index)
    {
        DialogueData dialogueData = (DialogueData)target;

        if (index >= 0 && index < dialogueData.nodes.Length)
        {
            List<DialogueData.DialogueNode> nodeList = new List<DialogueData.DialogueNode>(dialogueData.nodes);
            nodeList.RemoveAt(index);
            dialogueData.nodes = nodeList.ToArray();

            if (selectedNodeIndex >= dialogueData.nodes.Length)
            {
                selectedNodeIndex = dialogueData.nodes.Length - 1;
            }

            EditorUtility.SetDirty(dialogueData);
        }
    }

    void CopyNode(DialogueData.DialogueNode node)
    {
        copiedNode = new DialogueData.DialogueNode();
        copiedNode.id = node.id;
        copiedNode.speakerName = node.speakerName;
        copiedNode.dialogueText = node.dialogueText;
        copiedNode.characterTexture = node.characterTexture;
        copiedNode.nextNodeId = node.nextNodeId;

        if (node.choices != null)
        {
            copiedNode.choices = new DialogueData.Choice[node.choices.Length];
            for (int i = 0; i < node.choices.Length; i++)
            {
                copiedNode.choices[i] = new DialogueData.Choice();
                copiedNode.choices[i].choiceText = node.choices[i].choiceText;
                copiedNode.choices[i].nextNodeId = node.choices[i].nextNodeId;
                copiedNode.choices[i].scoreChange = node.choices[i].scoreChange;
            }
        }
    }

    void PasteNode(DialogueData dialogueData)
    {
        if (copiedNode != null)
        {
            DialogueData.DialogueNode newNode = new DialogueData.DialogueNode();
            newNode.id = GetNextAvailableID(dialogueData);
            newNode.speakerName = copiedNode.speakerName;
            newNode.dialogueText = copiedNode.dialogueText;
            newNode.characterTexture = copiedNode.characterTexture;
            newNode.nextNodeId = copiedNode.nextNodeId;

            if (copiedNode.choices != null)
            {
                newNode.choices = new DialogueData.Choice[copiedNode.choices.Length];
                for (int i = 0; i < copiedNode.choices.Length; i++)
                {
                    newNode.choices[i] = new DialogueData.Choice();
                    newNode.choices[i].choiceText = copiedNode.choices[i].choiceText;
                    newNode.choices[i].nextNodeId = copiedNode.choices[i].nextNodeId;
                    newNode.choices[i].scoreChange = copiedNode.choices[i].scoreChange;
                }
            }

            System.Array.Resize(ref dialogueData.nodes, dialogueData.nodes.Length + 1);
            dialogueData.nodes[dialogueData.nodes.Length - 1] = newNode;

            EditorUtility.SetDirty(dialogueData);
        }
    }

    void MoveNode(DialogueData dialogueData, int index, int direction)
    {
        int newIndex = index + direction;
        if (newIndex >= 0 && newIndex < dialogueData.nodes.Length)
        {
            var temp = dialogueData.nodes[index];
            dialogueData.nodes[index] = dialogueData.nodes[newIndex];
            dialogueData.nodes[newIndex] = temp;

            selectedNodeIndex = newIndex;
            EditorUtility.SetDirty(dialogueData);
        }
    }

    #endregion

    #region Choice Operations

    void AddChoice(DialogueData.DialogueNode node)
    {
        if (node.choices == null)
        {
            node.choices = new DialogueData.Choice[0];
        }

        DialogueData.Choice newChoice = new DialogueData.Choice();
        newChoice.choiceText = "새 선택지";
        newChoice.nextNodeId = -1;
        newChoice.scoreChange = 0;

        System.Array.Resize(ref node.choices, node.choices.Length + 1);
        node.choices[node.choices.Length - 1] = newChoice;
    }

    void RemoveChoice(DialogueData.DialogueNode node, int index)
    {
        if (node.choices != null && index >= 0 && index < node.choices.Length)
        {
            List<DialogueData.Choice> choiceList = new List<DialogueData.Choice>(node.choices);
            choiceList.RemoveAt(index);
            node.choices = choiceList.ToArray();
        }
    }

    void CopyChoice(DialogueData.Choice choice)
    {
        copiedChoice = new DialogueData.Choice();
        copiedChoice.choiceText = choice.choiceText;
        copiedChoice.nextNodeId = choice.nextNodeId;
        copiedChoice.scoreChange = choice.scoreChange;
    }

    void PasteChoice(DialogueData.DialogueNode node)
    {
        if (copiedChoice != null)
        {
            AddChoice(node);
            var lastChoice = node.choices[node.choices.Length - 1];
            lastChoice.choiceText = copiedChoice.choiceText;
            lastChoice.nextNodeId = copiedChoice.nextNodeId;
            lastChoice.scoreChange = copiedChoice.scoreChange;
        }
    }

    #endregion

    #region Utility Functions

    int GetNextAvailableID(DialogueData dialogueData)
    {
        int maxId = 0;
        foreach (var node in dialogueData.nodes)
        {
            if (node.id > maxId)
                maxId = node.id;
        }
        return maxId + 1;
    }

    void AutoAssignIDs(DialogueData dialogueData)
    {
        for (int i = 0; i < dialogueData.nodes.Length; i++)
        {
            dialogueData.nodes[i].id = i + 1;
        }
        EditorUtility.SetDirty(dialogueData);
    }

    void ClearAllNodes(DialogueData dialogueData)
    {
        dialogueData.nodes = new DialogueData.DialogueNode[0];
        selectedNodeIndex = -1;
        EditorUtility.SetDirty(dialogueData);
    }

    int GetTotalChoiceCount(DialogueData dialogueData)
    {
        int count = 0;
        foreach (var node in dialogueData.nodes)
        {
            if (node.choices != null)
                count += node.choices.Length;
        }
        return count;
    }

    void ValidateStructure(DialogueData dialogueData)
    {
        if (dialogueData.nodes == null || dialogueData.nodes.Length == 0)
        {
            EditorUtility.DisplayDialog("검증 결과", "노드가 없습니다!", "확인");
            return;
        }

        string issues = "";

        // 중복 ID 검사
        for (int i = 0; i < dialogueData.nodes.Length; i++)
        {
            for (int j = i + 1; j < dialogueData.nodes.Length; j++)
            {
                if (dialogueData.nodes[i].id == dialogueData.nodes[j].id)
                {
                    issues += $"중복 ID 발견: {dialogueData.nodes[i].id}\n";
                }
            }
        }

        // 유효하지 않은 nextNodeId 검사
        foreach (var node in dialogueData.nodes)
        {
            if (node.nextNodeId != -1 && !HasNodeWithId(dialogueData, node.nextNodeId))
            {
                issues += $"노드 {node.id}: 존재하지 않는 nextNodeId {node.nextNodeId}\n";
            }

            if (node.choices != null)
            {
                foreach (var choice in node.choices)
                {
                    if (choice.nextNodeId != -1 && !HasNodeWithId(dialogueData, choice.nextNodeId))
                    {
                        issues += $"노드 {node.id}: 선택지에서 존재하지 않는 nextNodeId {choice.nextNodeId}\n";
                    }
                }
            }
        }

        // 빈 텍스트 검사
        foreach (var node in dialogueData.nodes)
        {
            if (string.IsNullOrEmpty(node.dialogueText))
            {
                issues += $"노드 {node.id}: 대사 내용이 비어있습니다.\n";
            }

            if (string.IsNullOrEmpty(node.speakerName))
            {
                issues += $"노드 {node.id}: 화자 이름이 비어있습니다.\n";
            }

            if (node.choices != null)
            {
                for (int i = 0; i < node.choices.Length; i++)
                {
                    if (string.IsNullOrEmpty(node.choices[i].choiceText))
                    {
                        issues += $"노드 {node.id}: 선택지 {i + 1}의 텍스트가 비어있습니다.\n";
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(issues))
        {
            EditorUtility.DisplayDialog("검증 결과", "모든 구조가 정상입니다!", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("검증 결과", "발견된 문제:\n" + issues, "확인");
        }
    }

    bool HasNodeWithId(DialogueData dialogueData, int id)
    {
        foreach (var node in dialogueData.nodes)
        {
            if (node.id == id)
                return true;
        }
        return false;
    }

    void TestDialogue(DialogueData dialogueData)
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData);
        }
        else
        {
            EditorUtility.DisplayDialog("테스트 오류", "DialogueManager를 찾을 수 없습니다.", "확인");
        }
    }

    #endregion
}