using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class QuestContainer : ScriptableObject
{
    public List<NodeLinkData> nodeLinks = new List<NodeLinkData>();
    public List<QuestNodeData> questNodeData = new List<QuestNodeData>();
    public List<ExposedProperty> exposedProperties = new List<ExposedProperty>();
}
