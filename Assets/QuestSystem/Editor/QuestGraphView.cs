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

public class QuestGraphView : GraphView
{
    public readonly Vector2 defaultNodeSize = new Vector2(150, 200);

    public Blackboard blackBoard;
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
    private NodeSearchWindow _searchWindow;

    public QuestGraphView(EditorWindow editorWindow)
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
        AddSearchWindow(editorWindow);
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

    public void CreateNode(string nodeName, Vector2 mousePosition)
    {
        AddElement(CreateQuestNode(nodeName, mousePosition));
    }

    public QuestGraphNode CreateQuestNode(string nodeName, Vector2 mousePosition)
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
        questNode.SetPosition(new Rect(mousePosition, defaultNodeSize));

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

    void AddSearchWindow(EditorWindow editorWindow)
    {
        _searchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
        _searchWindow.Init(this, editorWindow);
        nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), _searchWindow);
    }

    public void AddPropertyToBlackBoard(ExposedProperty exposedProperty)
    {
        var localPropertyName = exposedProperty.propertyName;
        var localPropertyValue = exposedProperty.propertyValue;
        while (exposedProperties.Any(x => x.propertyName == localPropertyName))
            localPropertyName = $"{localPropertyName}(1)";


        var property = new ExposedProperty();
        property.propertyName = exposedProperty.propertyName;
        property.propertyValue = exposedProperty.propertyValue;
        exposedProperties.Add(property);

        var container = new VisualElement();
        var blackboardField = new BlackboardField { text = property.propertyName, typeText = "string" };
        container.Add(blackboardField);

        var propertyValueTextField = new TextField("Value:")
        {
            value = property.propertyValue
        };

        propertyValueTextField.RegisterValueChangedCallback(evt =>
        {
            var changingPropertyIndex = exposedProperties.FindIndex(x => x.propertyName == property.propertyName);
            exposedProperties[changingPropertyIndex].propertyValue = evt.newValue;
        });

        var blackBoardValueRow = new BlackboardRow(blackboardField, propertyValueTextField);
        container.Add(blackBoardValueRow);

        blackBoard.Add(container);
    }
}