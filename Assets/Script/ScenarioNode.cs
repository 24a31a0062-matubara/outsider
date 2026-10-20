using System.Collections.Generic;
using UnityEngine;

public enum NodeProgressType
{
    Choices,        // 選択肢を表示する
    ClickToContinue, // クリックで次へ進む
    Stop            // その場で停止する
}

[System.Serializable]
public class ScenarioChoice
{
    public string choiceText;
    public string nextNodeID;

    [Header("表示に必要なフラグ（すべて必要）")]
    public List<string> requiredFlags = new List<string>();
}

[CreateAssetMenu(fileName = "ScenarioNode", menuName = "NOEx/Scenario Node")]
public class ScenarioNode : ScriptableObject
{
    public string nodeID;

    [TextArea(3, 10)]
    public string text;

    [Header("文章を読み終えた後の動作")]
    public NodeProgressType progressType = NodeProgressType.Choices;

    [Header("選択肢")]
    public List<ScenarioChoice> choices = new List<ScenarioChoice>();

    [Header("このノードに入ったときONにするフラグ")]
    public string setFlag;

    [Header("エンディング設定")]
    public bool isEnding = false;

    [ContextMenu("このノードの設定を検査")]
    public void ValidateNode()
    {
        bool hasError = false;

        if (string.IsNullOrWhiteSpace(nodeID))
        {
            Debug.LogError(name + "：Node IDが空欄です。", this);
            hasError = true;
        }

        if (progressType == NodeProgressType.Choices &&
            (choices == null || choices.Count == 0))
        {
            Debug.LogWarning(
                nodeID + "：選択肢を表示する設定ですが、選択肢がありません。",
                this);
        }

        if (choices != null)
        {
            foreach (ScenarioChoice choice in choices)
            {
                if (choice == null)
                    continue;

                if (string.IsNullOrWhiteSpace(choice.choiceText))
                {
                    Debug.LogWarning(
                        nodeID + "：選択肢の文章が空欄です。",
                        this);
                }

                if (string.IsNullOrWhiteSpace(choice.nextNodeID))
                {
                    Debug.LogError(
                        nodeID + "：「" + choice.choiceText +
                        "」の移動先が空欄です。",
                        this);
                    hasError = true;
                }

                if (choice.requiredFlags == null)
                    continue;

                foreach (string flag in choice.requiredFlags)
                {
                    if (string.IsNullOrWhiteSpace(flag))
                    {
                        Debug.LogWarning(
                            nodeID + "：「" + choice.choiceText +
                            "」に空欄のRequired Flagがあります。",
                            this);
                    }
                }
            }
        }

        if (!hasError)
        {
            Debug.Log(nodeID + "：ノード内の基本設定を検査しました。", this);
        }
    }
}