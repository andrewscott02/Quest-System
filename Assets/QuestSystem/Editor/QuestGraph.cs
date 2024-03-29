using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

//https://www.youtube.com/playlist?list=PLF3U0rzFKlTGzz-AUFacf9_OKiW_hGYIR
//https://github.com/merpheus-dev/NodeBasedDialogueSystem/blob/master/com.subtegral.dialoguesystem/Editor/GraphSaveUtility.cs

//https://www.youtube.com/playlist?list=PL0yxB6cCkoWK38XT4stSztcLueJ_kTx5f

public class QuestGraph : EditorWindow
{
    private QuestGraphView _graphView;
    private string _fileName = "New Quest";

    [MenuItem("Graph/Quest Graph")]
    public static void OpenQuestGraphWindow()
    {
        var window = GetWindow<QuestGraph>();
        window.titleContent = new GUIContent(text: "Quest Graph");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
        GenerateMiniMap();
        GenerateBlackBoard();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(_graphView);
    }

    private void ConstructGraphView()
    {
        _graphView = new QuestGraphView(this)
        {
            name = "Quest Graph"
        };

        _graphView.StretchToParentSize();
        rootVisualElement.Add(_graphView);
    }

    private void GenerateToolbar()
    {
        var toolbar = new Toolbar();

        var fileNameTextField = new TextField(label: "File Name:");
        fileNameTextField.SetValueWithoutNotify(_fileName);
        fileNameTextField.MarkDirtyRepaint();
        fileNameTextField.RegisterValueChangedCallback(evt => _fileName = evt.newValue);
        toolbar.Add(fileNameTextField);

        toolbar.Add(child: new Button(clickEvent: () => RequestDataOperation(true)) {text = "Save Data" });
        toolbar.Add(child: new Button(clickEvent: () => RequestDataOperation(false)) { text = "Load Data" });

        rootVisualElement.Add(toolbar);
    }

    private void RequestDataOperation(bool save)
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            EditorUtility.DisplayDialog("Invalid file name!", "Please enter a valie file name.", ok: "OK");
            return;
        }

        var saveUtility = GraphSaveUtility.GetInstance(_graphView);
        if (save)
            saveUtility.SaveGraph(_fileName);
        else
            saveUtility.LoadGraph(_fileName);
    }

    private void GenerateMiniMap()
    {
        var miniMap = new MiniMap { anchored = true};
        miniMap.SetPosition(new Rect(10, 30, 200, 140));
        _graphView.Add(miniMap);
    }

    private void GenerateBlackBoard()
    {
        var blackBoard = new Blackboard(_graphView);
        blackBoard.Add(new BlackboardSection { title = "Exposed Properties" });

        blackBoard.addItemRequested = _blackboard =>
        {
            _graphView.AddPropertyToBlackBoard(new ExposedProperty());
        };

        blackBoard.editTextRequested = (blackBoard1, element, newValue) =>
        {
            var oldPropertyName = ((BlackboardField)element).text;
            if (_graphView.exposedProperties.Any(x => x.propertyName == newValue))
            {
                EditorUtility.DisplayDialog("Error", "This property name already exists, please choose another one!", "OK");
                return;
            }

            var propertyIndex = _graphView.exposedProperties.FindIndex(x => x.propertyName == oldPropertyName);
            _graphView.exposedProperties[propertyIndex].propertyName = newValue;
            ((BlackboardField)element).text = newValue;
        };

        blackBoard.SetPosition(new Rect(this.position.width - 210, 30, 200, 300));

        _graphView.blackBoard = blackBoard;

        _graphView.Add(blackBoard);
    }
}
