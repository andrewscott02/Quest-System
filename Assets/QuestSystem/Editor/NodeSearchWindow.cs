using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
{
    private QuestGraphView _graphView;
    private EditorWindow _window;

    public void Init(QuestGraphView graphView, EditorWindow window) 
    { 
        _graphView = graphView; 
        _window = window;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var tree = new List<SearchTreeEntry>
        {
            new SearchTreeGroupEntry(new GUIContent("Create Elements"), 0),
            new SearchTreeGroupEntry(new GUIContent("Quest Node"), 1),
            new SearchTreeEntry(new GUIContent("Quest Node"))
            {
                userData = new QuestGraphNode(), level = 2
            }
        };
        return tree;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        var worldMousePos = _window.rootVisualElement.ChangeCoordinatesTo(_window.rootVisualElement.parent, 
            context.screenMousePosition - _window.position.position);

        var localMousePos = _graphView.contentViewContainer.WorldToLocal(worldMousePos);

        switch (SearchTreeEntry.userData)
        {
            case QuestGraphNode:
                _graphView.CreateNode("Quest Node", localMousePos);
                return true;
            default:
                return false;
        }
    }
}
