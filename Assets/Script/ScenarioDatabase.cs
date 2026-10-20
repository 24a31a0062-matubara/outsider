using System.Collections.Generic;
using UnityEngine;

public class ScenarioDatabase : MonoBehaviour
{
    public List<ScenarioNode> nodes = new List<ScenarioNode>();

    [SerializeField]
    private FlagManager flagManager;

    public ScenarioNode GetNode(string id)
    {
        foreach (ScenarioNode node in nodes)
        {
            if (node == null)
                continue;

            if (node.nodeID == id)
                return node;
        }

        Debug.LogError("Nodeが見つかりません: " + id);
        return null;
    }

    [ContextMenu("ノード設定を検査")]
    public void ValidateNodes()
    {
        HashSet<string> nodeIDs = new HashSet<string>();
        bool hasError = false;

        // ノードIDの重複・空欄を検査
        foreach (ScenarioNode node in nodes)
        {
            if (node == null)
            {
                Debug.LogError("ScenarioDatabaseに未設定のノードがあります。", this);
                hasError = true;
                continue;
            }

            if (string.IsNullOrWhiteSpace(node.nodeID))
            {
                Debug.LogError("Node IDが空欄です: " + node.name, node);
                hasError = true;
                continue;
            }

            if (!nodeIDs.Add(node.nodeID))
            {
                Debug.LogError("Node IDが重複しています: " + node.nodeID, node);
                hasError = true;
            }
        }

        // フラグ名と選択肢の移動先を検査
        foreach (ScenarioNode node in nodes)
        {
            if (node == null)
                continue;

            // ノード進入時に設定するフラグ
            if (!string.IsNullOrWhiteSpace(node.setFlag))
            {
                if (flagManager == null ||
                    !flagManager.IsFlagRegistered(node.setFlag))
                {
                    Debug.LogError(
                        node.nodeID + "：未登録のsetFlag「" +
                        node.setFlag + "」です。",
                        node);
                    hasError = true;
                }
            }

            if (node.choices == null)
                continue;

            foreach (ScenarioChoice choice in node.choices)
            {
                if (choice == null)
                    continue;

                // 移動先の検査
                if (string.IsNullOrWhiteSpace(choice.nextNodeID))
                {
                    Debug.LogError(
                        node.nodeID + "：選択肢「" +
                        choice.choiceText + "」の移動先が空欄です。",
                        node);
                    hasError = true;
                }
                else if (!nodeIDs.Contains(choice.nextNodeID))
                {
                    Debug.LogError(
                        node.nodeID + "：移動先「" +
                        choice.nextNodeID + "」が見つかりません。",
                        node);
                    hasError = true;
                }

                // 選択肢に必要なフラグの検査
                if (choice.requiredFlags == null)
                    continue;

                foreach (string flag in choice.requiredFlags)
                {
                    if (string.IsNullOrWhiteSpace(flag))
                        continue;

                    if (flagManager == null ||
                        !flagManager.IsFlagRegistered(flag))
                    {
                        Debug.LogError(
                            node.nodeID + "：選択肢「" +
                            choice.choiceText +
                            "」に未登録のRequired Flag「" +
                            flag + "」があります。",
                            node);
                        hasError = true;
                    }
                }
            }
        }

        if (!hasError)
        {
            Debug.Log("ノード設定の検査完了：エラーはありません！", this);
        }
    }
}