using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScenarioChoice
{
    public string choiceText;
    public string nextNodeID;
}

[CreateAssetMenu(fileName = "ScenarioNode", menuName = "NOEx/Scenario Node")]
public class ScenarioNode : ScriptableObject
{
    public string nodeID;

    [TextArea(3, 10)]
    public string text;

    public List<ScenarioChoice> choices;

    // このノードを通過したときにONにするフラグ
    public string setFlag;
}