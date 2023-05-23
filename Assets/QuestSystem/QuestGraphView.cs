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

public class QuestGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    public QuestGraphView()
    {
        styleSheets.Add(Resources.Load<StyleSheet>(path: "QuestStyleSheet"));
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

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

        node.capabilities &= ~Capabilities.Movable;
        node.capabilities &= ~Capabilities.Deletable;

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

        questNode.styleSheets.Add(Resources.Load<StyleSheet>(path: "NodeStyleSheet"));

        var button = new Button(clickEvent: () => { AddChoicePort(questNode); });
        button.text = "New Output";
        questNode.titleContainer.Add(button);

        var textField = new TextField(string.Empty);
        textField.RegisterValueChangedCallback(evt =>
        {
            questNode.questText = evt.newValue;
            questNode.title = evt.newValue;
        });
        textField.SetValueWithoutNotify(questNode.title);
        questNode.mainContainer.Add(textField);

        questNode.RefreshExpandedState();
        questNode.RefreshPorts();
        questNode.SetPosition(new Rect(Vector2.zero, defaultNodeSize));

        return questNode;
    }

    public  void AddChoicePort(QuestGraphNode questNode, string overriddenPortName = "")
    {
        var generatedPort = GeneratePort(questNode, Direction.Output);

        var oldLabel = generatedPort.contentContainer.Q<Label>("type");
        generatedPort.contentContainer.Remove(oldLabel);

        var outputPortCount = questNode.outputContainer.Query("connector").ToList().Count;

        var choicePortName = string.IsNullOrEmpty(overriddenPortName) ? 
            $"Choice {outputPortCount + 1}" : 
            overriddenPortName;

        var textField = new TextField
        {
            name = String.Empty,
            value = choicePortName
        };

        textField.RegisterValueChangedCallback(evt => generatedPort.portName = evt.newValue);
        generatedPort.contentContainer.Add(new Label("  "));
        generatedPort.contentContainer.Add(textField);
        var deleteButton = new Button(clickEvent: () => RemovePort(questNode, generatedPort))
        {
            text = "X"
        };
        generatedPort.contentContainer.Add(deleteButton);

        generatedPort.portName = choicePortName;
        questNode.outputContainer.Add(generatedPort);
        questNode.RefreshExpandedState();
        questNode.RefreshPorts();
    }

    private void RemovePort(QuestGraphNode questNode, Port generatedPort)
    {
        var targetEdge = edges.ToList().Where(x => x.output.portName == generatedPort.portName && x.output.node == generatedPort.node);

        if (!targetEdge.Any()) return;
        var edge = targetEdge.First();
        edge.input.Disconnect(edge);
        RemoveElement(targetEdge.First());

        questNode.outputContainer.Remove(generatedPort);
        questNode.RefreshPorts();
        questNode.RefreshExpandedState();
    }
}
