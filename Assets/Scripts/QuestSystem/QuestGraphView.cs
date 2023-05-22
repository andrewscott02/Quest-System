using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;

//https://www.youtube.com/playlist?list=PLF3U0rzFKlTGzz-AUFacf9_OKiW_hGYIR

public class QuestGraphView : GraphView
{
    private readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    public QuestGraphView()
    {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        AddElement(GenerateEntryPointNode());
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        foreach(var item in ports)
        {
            if (startPort != item && startPort.node != item.node)
            {
                compatiblePorts.Add(item);
            }
        }

        return compatiblePorts;
    }

    private QuestGraphNode GenerateEntryPointNode()
    {
        var node = new QuestGraphNode
        {
            title = "START",
            GUID = Guid.NewGuid().ToString(),
            questText = "ENTRYPOINT",
            entryPoint = true
        };

        var generatedPort = GeneratePort(node, Direction.Output);
        generatedPort.portName = "Next";
        node.outputContainer.Add(generatedPort);

        node.RefreshExpandedState();
        node.RefreshPorts();

        node.SetPosition(new Rect(100, 200, 100, 150));

        return node;
    }

    private Port GeneratePort(QuestGraphNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float)); // TODO: Change specific types later
    }

    public void CreateNode(string nodeName)
    {
        AddElement(CreateQuestNode(nodeName));
    }

    public QuestGraphNode CreateQuestNode(string nodeName)
    {
        var questNode = new QuestGraphNode
        {
            title = nodeName,
            questText = nodeName,
            GUID = Guid.NewGuid().ToString(),
        };

        var inputPort = GeneratePort(questNode, Direction.Input, Port.Capacity.Multi);

        inputPort.portName = "Input";
        questNode.inputContainer.Add(inputPort);

        var button = new Button(clickEvent: () => { AddChoicePort(questNode); });

        questNode.RefreshExpandedState();
        questNode.RefreshPorts();
        questNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return questNode;
    }

    private void AddChoicePort(QuestGraphNode questNode)
    {
        var generatedPort = GeneratePort(questNode, Direction.Output);

        var outputPortCount = questNode.outputContainer.Query("connector").ToList().Count;
        generatedPort.portName = $"Choice {outputPortCount}";

        questNode.outputContainer.Add(generatedPort);
        questNode.RefreshExpandedState();
        questNode.RefreshPorts();
    }
}
