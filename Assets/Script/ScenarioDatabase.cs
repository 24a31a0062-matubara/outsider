using System.Collections.Generic;
using UnityEngine;

public class ScenarioDatabase : MonoBehaviour
{
    public List<ScenarioNode> nodes = new List<ScenarioNode>();

    public ScenarioNode GetNode(string id)
    {
        foreach (ScenarioNode node in nodes)
        {
            if (node.nodeID == id)
            {
                return node;
            }
        }

        Debug.LogError("Node‚ªŒ©‚Â‚©‚è‚Ü‚¹‚ñ: " + id);
        return null;
    }
}