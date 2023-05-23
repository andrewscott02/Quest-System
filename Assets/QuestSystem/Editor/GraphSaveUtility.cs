using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

public class GraphSaveUtility
{
    private QuestGraphView _targetGraphView;
    private QuestContainer _containerCache;

    private List<Edge> _edges => _targetGraphView.edges.ToList();
    private List<QuestGraphNode> _nodes => _targetGraphView.nodes.ToList().Cast<QuestGraphNode>().ToList();

    public static GraphSaveUtility GetInstance(QuestGraphView targetGraphView)
    {
        return new GraphSaveUtility
        {
            _targetGraphView = targetGraphView
        };
    }

    public void SaveGraph(string fileName)
    {
        if (!_edges.Any()) return;

        QuestContainer questContainer = ScriptableObject.CreateInstance<QuestContainer>();

        Edge[] connectedPorts = _edges.Where(x => x.input.node != null).ToArray();

        for(int i = 0; i < connectedPorts.Length; i++)
        {
            QuestGraphNode outputNode = connectedPorts[i].output.node as QuestGraphNode;
            QuestGraphNode inputNode = connectedPorts[i].input.node as QuestGraphNode;

            questContainer.nodeLinks.Add(new NodeLinkData
            {
                baseNodeGUID = outputNode.GUID,
                portName = connectedPorts[i].output.portName,
                targetNodeGUID = inputNode.GUID
            });
        }

        foreach(var item in _nodes.Where(node => !node.entryPoint))
        {
            questContainer.questNodeData.Add(new QuestNodeData
            {
                GUID = item.GUID,
                questText = item.questText,
                position = item.GetPosition().position
            });
        }

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        {
            Debug.Log("no folder, creating one");
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        AssetDatabase.CreateAsset(questContainer, $"Assets/Resources/{fileName}.asset");
        AssetDatabase.SaveAssets();
    }

    public void LoadGraph(string fileName)
    {
        _containerCache = Resources.Load<QuestContainer>(fileName);

        if (_containerCache == null)
        {
            EditorUtility.DisplayDialog("File Not Found", "Target quest graph file does not exist", "OK");
            return;
        }

        ClearGraph();
        CreateNodes();
        ConnectNodes();
    }

    private void ClearGraph()
    {
        _nodes.Find(x => x.entryPoint).GUID = _containerCache.nodeLinks[0].baseNodeGUID;

        foreach (var node in _nodes)
        {
            if (node.entryPoint) continue;

            _edges.Where(x => x.input.node == node).ToList()
                .ForEach(edge => _targetGraphView.RemoveElement(edge));

            _targetGraphView.RemoveElement(node);
        }
    }

    private void CreateNodes()
    {
        foreach (var item in _containerCache.questNodeData)
        {
            QuestGraphNode tempNode = _targetGraphView.CreateQuestNode(item.questText, Vector2.zero);
            tempNode.GUID = item.GUID;
            _targetGraphView.AddElement(tempNode);

            List<NodeLinkData> nodePorts = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == item.GUID).ToList();
            nodePorts.ForEach(x => _targetGraphView.AddChoicePort(tempNode, x.portName));
        }
    }

    private void ConnectNodes()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            var connections = _containerCache.nodeLinks.Where(x => x.baseNodeGUID == _nodes[i].GUID).ToList();

            for(int j = 0; j < connections.Count; j++)
            {
                string targetNodeGUID = connections[j].targetNodeGUID;
                QuestGraphNode targetNode = _nodes.First(x => x.GUID == targetNodeGUID);
                LinkNodes(_nodes[i].outputContainer[j].Q<Port>(), (Port)targetNode.inputContainer[0]);

                targetNode.SetPosition(new Rect
                    (
                    _containerCache.questNodeData.First(x => x.GUID == targetNodeGUID).position,
                    _targetGraphView.defaultNodeSize
                    ));
            }
        }
    }

    private void LinkNodes(Port output, Port input)
    {
        Edge tempEdge = new Edge()
        {
            output = output,
            input = input
        };

        tempEdge.input.Connect(tempEdge);
        tempEdge.output.Connect(tempEdge);
        _targetGraphView.Add(tempEdge);
    }
}
