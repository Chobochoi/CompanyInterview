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

    // ����/�ٿ��ֱ��
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

        // ��� �׸���
        nodeList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Dialogue Nodes");
        };

        // ��� �׸���
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

        // ��� ���� ��
        nodeList.onSelectCallback = (ReorderableList list) =>
        {
            selectedNodeIndex = list.index;
        };

        // �� ��� �߰� ��
        nodeList.onAddCallback = (ReorderableList list) =>
        {
            AddNewNode();
        };

        // ��� ���� ��
        nodeList.onRemoveCallback = (ReorderableList list) =>
        {
            RemoveNode(list.index);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DialogueData dialogueData = (DialogueData)target;

        // ���
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dialogue Data Editor", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // ��� ����
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField($"�� ��� ��: {dialogueData.nodes.Length}");
        EditorGUILayout.LabelField($"�� ������ ��: {GetTotalChoiceCount(dialogueData)}");
        EditorGUILayout.EndVertical();

        // �̸����� ���
        showPreview = EditorGUILayout.Toggle("�̸����� ǥ��", showPreview);

        EditorGUILayout.Space();

        // ��ƿ��Ƽ ��ư��
        DrawUtilityButtons(dialogueData);

        EditorGUILayout.Space();

        // ��� ����Ʈ
        nodeList.DoLayoutList();

        EditorGUILayout.Space();

        // ���õ� ��� ����
        if (selectedNodeIndex >= 0 && selectedNodeIndex < dialogueData.nodes.Length)
        {
            DrawNodeEditor(dialogueData, selectedNodeIndex);
        }

        // �̸�����
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

        if (GUILayout.Button("�� ��� �߰�"))
        {
            AddNewNode();
        }

        if (GUILayout.Button("ID �ڵ� ����"))
        {
            AutoAssignIDs(dialogueData);
        }

        if (GUILayout.Button("���� ����"))
        {
            ValidateStructure(dialogueData);
        }

        if (GUILayout.Button("�׽�Ʈ ����") && Application.isPlaying)
        {
            TestDialogue(dialogueData);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = copiedNode != null;
        if (GUILayout.Button("��� �ٿ��ֱ�"))
        {
            PasteNode(dialogueData);
        }
        GUI.enabled = true;

        if (GUILayout.Button("��ü �ʱ�ȭ"))
        {
            if (EditorUtility.DisplayDialog("Ȯ��", "��� ��带 �����Ͻðڽ��ϱ�?", "����", "���"))
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
        EditorGUILayout.LabelField($"��� {node.id} ����", EditorStyles.boldLabel);

        // ��� ���� ��ư
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("����"))
        {
            CopyNode(node);
        }
        if (GUILayout.Button("����"))
        {
            RemoveNode(index);
            return;
        }
        if (GUILayout.Button("����"))
        {
            MoveNode(dialogueData, index, -1);
        }
        if (GUILayout.Button("�Ʒ���"))
        {
            MoveNode(dialogueData, index, 1);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // �⺻ ���� ����
        node.id = EditorGUILayout.IntField("ID", node.id);
        node.speakerName = EditorGUILayout.TextField("ȭ�� �̸�", node.speakerName);

        EditorGUILayout.LabelField("��� ����:");
        node.dialogueText = EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(60));

        node.characterTexture = (Texture2D)EditorGUILayout.ObjectField("ĳ���� �̹���", node.characterTexture, typeof(Texture2D), false);
        node.nextNodeId = EditorGUILayout.IntField("���� ��� ID (-1: ����)", node.nextNodeId);

        EditorGUILayout.Space();

        // ������ ����
        DrawChoiceEditor(node);

        EditorGUILayout.EndVertical();

        EditorUtility.SetDirty(dialogueData);
    }

    void DrawChoiceEditor(DialogueData.DialogueNode node)
    {
        EditorGUILayout.LabelField("������", EditorStyles.boldLabel);

        // ������ �߰�/���� ��ư
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("������ �߰�"))
        {
            AddChoice(node);
        }

        GUI.enabled = copiedChoice != null;
        if (GUILayout.Button("������ �ٿ��ֱ�"))
        {
            PasteChoice(node);
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        if (node.choices == null)
        {
            node.choices = new DialogueData.Choice[0];
        }

        // ������ ���
        for (int i = 0; i < node.choices.Length; i++)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"������ {i + 1}", EditorStyles.miniBoldLabel);

            if (GUILayout.Button("����", GUILayout.Width(50)))
            {
                CopyChoice(node.choices[i]);
            }
            if (GUILayout.Button("����", GUILayout.Width(50)))
            {
                RemoveChoice(node, i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            node.choices[i].choiceText = EditorGUILayout.TextField("������ �ؽ�Ʈ", node.choices[i].choiceText);
            node.choices[i].nextNodeId = EditorGUILayout.IntField("���� ��� ID", node.choices[i].nextNodeId);
            node.choices[i].scoreChange = EditorGUILayout.IntField("���� ��ȭ", node.choices[i].scoreChange);

            EditorGUILayout.EndVertical();
        }
    }

    void DrawPreview(DialogueData dialogueData)
    {
        EditorGUILayout.LabelField("�̸�����", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

        foreach (var node in dialogueData.nodes)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"[{node.id}] {node.speakerName}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"�� {(node.nextNodeId == -1 ? "END" : node.nextNodeId.ToString())}", GUILayout.Width(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(node.dialogueText, EditorStyles.wordWrappedLabel);

            if (node.choices != null && node.choices.Length > 0)
            {
                EditorGUILayout.LabelField("������:", EditorStyles.miniBoldLabel);
                for (int i = 0; i < node.choices.Length; i++)
                {
                    var choice = node.choices[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"  {i + 1}. {choice.choiceText}");
                    EditorGUILayout.LabelField($"��{choice.nextNodeId} ({choice.scoreChange:+0;-0;0})", GUILayout.Width(80));
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

        // �� ��� ����
        DialogueData.DialogueNode newNode = new DialogueData.DialogueNode();
        newNode.id = GetNextAvailableID(dialogueData);
        newNode.speakerName = "�� ȭ��";
        newNode.dialogueText = "���ο� ��縦 �Է��ϼ���.";
        newNode.nextNodeId = -1;
        newNode.choices = new DialogueData.Choice[0];

        // �迭�� �߰�
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
        newChoice.choiceText = "�� ������";
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
            EditorUtility.DisplayDialog("���� ���", "��尡 �����ϴ�!", "Ȯ��");
            return;
        }

        string issues = "";

        // �ߺ� ID �˻�
        for (int i = 0; i < dialogueData.nodes.Length; i++)
        {
            for (int j = i + 1; j < dialogueData.nodes.Length; j++)
            {
                if (dialogueData.nodes[i].id == dialogueData.nodes[j].id)
                {
                    issues += $"�ߺ� ID �߰�: {dialogueData.nodes[i].id}\n";
                }
            }
        }

        // ��ȿ���� ���� nextNodeId �˻�
        foreach (var node in dialogueData.nodes)
        {
            if (node.nextNodeId != -1 && !HasNodeWithId(dialogueData, node.nextNodeId))
            {
                issues += $"��� {node.id}: �������� �ʴ� nextNodeId {node.nextNodeId}\n";
            }

            if (node.choices != null)
            {
                foreach (var choice in node.choices)
                {
                    if (choice.nextNodeId != -1 && !HasNodeWithId(dialogueData, choice.nextNodeId))
                    {
                        issues += $"��� {node.id}: ���������� �������� �ʴ� nextNodeId {choice.nextNodeId}\n";
                    }
                }
            }
        }

        // �� �ؽ�Ʈ �˻�
        foreach (var node in dialogueData.nodes)
        {
            if (string.IsNullOrEmpty(node.dialogueText))
            {
                issues += $"��� {node.id}: ��� ������ ����ֽ��ϴ�.\n";
            }

            if (string.IsNullOrEmpty(node.speakerName))
            {
                issues += $"��� {node.id}: ȭ�� �̸��� ����ֽ��ϴ�.\n";
            }

            if (node.choices != null)
            {
                for (int i = 0; i < node.choices.Length; i++)
                {
                    if (string.IsNullOrEmpty(node.choices[i].choiceText))
                    {
                        issues += $"��� {node.id}: ������ {i + 1}�� �ؽ�Ʈ�� ����ֽ��ϴ�.\n";
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(issues))
        {
            EditorUtility.DisplayDialog("���� ���", "��� ������ �����Դϴ�!", "Ȯ��");
        }
        else
        {
            EditorUtility.DisplayDialog("���� ���", "�߰ߵ� ����:\n" + issues, "Ȯ��");
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
            EditorUtility.DisplayDialog("�׽�Ʈ ����", "DialogueManager�� ã�� �� �����ϴ�.", "Ȯ��");
        }
    }

    #endregion
}