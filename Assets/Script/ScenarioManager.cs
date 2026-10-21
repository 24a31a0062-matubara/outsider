using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private GameObject choicePanel;

    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [SerializeField] private ScenarioDatabase database;
    [SerializeField] private FlagManager flagManager;

    [SerializeField] private string startNodeID = "START";
    [SerializeField] private float textSpeed = 0.05f;

    private ScenarioNode currentNode;
    private List<ScenarioChoice> availableChoices = new List<ScenarioChoice>();

    // 訪問済みノードを記録する
    private HashSet<string> visitedNodes = new HashSet<string>();

    // 現在表示する文章
    private string currentText;

    private bool isTyping;
    private bool textFinished;

    private void Start()
    {
        choicePanel.SetActive(false);
        LoadNode(startNodeID);
    }

    private void Update()
    {
        if (choicePanel.activeSelf)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        if (isTyping)
        {
            StopAllCoroutines();

            scenarioText.text = currentText;

            isTyping = false;
            textFinished = true;

            // 文字送りを飛ばした場合も進行方式を反映する
            OnTextFinished();
        }
        else if (textFinished)
        {
            // クリックで次へ進む設定の場合のみ移動する
            if (currentNode.progressType == NodeProgressType.ClickToContinue)
            {
                ContinueToNextNode();
            }
        }
    }

    public void LoadNode(string nodeID)
    {
        StopAllCoroutines();

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
            return;

        // 初回訪問かどうかを判定する
        bool isFirstVisit = visitedNodes.Add(nodeID);

        // 基本は通常の文章を使う
        currentText = currentNode.text;

        // 2回目以降で再訪問用の文章が設定されていれば切り替える
        if (!isFirstVisit &&
            !string.IsNullOrEmpty(currentNode.revisitText))
        {
            currentText = currentNode.revisitText;
        }

        if (!string.IsNullOrEmpty(currentNode.setFlag))
        {
            flagManager.SetFlag(currentNode.setFlag);
        }

        if (currentNode.isEnding)
        {
            if (!string.IsNullOrEmpty(currentNode.endingFlag))
            {
                flagManager.SetFlag(currentNode.endingFlag);

                Debug.Log(
                    "エンディングを記録しました: " +
                    currentNode.endingFlag
                );
            }
        }

        scenarioText.text = "";
        choicePanel.SetActive(false);

        isTyping = false;
        textFinished = false;

        StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        textFinished = false;

        foreach (char letter in currentText)
        {
            scenarioText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        textFinished = true;

        OnTextFinished();
    }

    private void OnTextFinished()
    {
        switch (currentNode.progressType)
        {
            case NodeProgressType.Choices:
                ShowChoices();
                break;

            case NodeProgressType.ClickToContinue:
                choicePanel.SetActive(false);
                break;

            case NodeProgressType.Stop:
                choicePanel.SetActive(false);
                break;
        }
    }

    private void ShowChoices()
    {
        availableChoices.Clear();

        if (currentNode.choices != null)
        {
            foreach (ScenarioChoice choice in currentNode.choices)
            {
                if (choice == null) continue;

                bool conditionsMet = true;

                if (choice.requiredFlags != null)
                {
                    foreach (string requiredFlag in choice.requiredFlags)
                    {
                        if (!flagManager.HasFlag(requiredFlag))
                        {
                            conditionsMet = false;
                            break;
                        }
                    }
                }

                if (conditionsMet)
                {
                    availableChoices.Add(choice);
                }
            }
        }

        choiceAText.gameObject.SetActive(false);
        choiceBText.gameObject.SetActive(false);
        choiceCText.gameObject.SetActive(false);

        if (availableChoices.Count == 0)
        {
            choicePanel.SetActive(false);
            return;
        }

        if (availableChoices.Count >= 1)
        {
            choiceAText.text = availableChoices[0].choiceText;
            choiceAText.gameObject.SetActive(true);
        }

        if (availableChoices.Count >= 2)
        {
            choiceBText.text = availableChoices[1].choiceText;
            choiceBText.gameObject.SetActive(true);
        }

        if (availableChoices.Count >= 3)
        {
            choiceCText.text = availableChoices[2].choiceText;
            choiceCText.gameObject.SetActive(true);
        }

        choicePanel.SetActive(true);
    }

    private void ContinueToNextNode()
    {
        if (currentNode == null || currentNode.choices == null)
            return;

        // クリック進行では、条件を満たす最初の選択肢へ進む
        availableChoices.Clear();

        foreach (ScenarioChoice choice in currentNode.choices)
        {
            if (choice == null) continue;

            bool conditionsMet = true;

            if (choice.requiredFlags != null)
            {
                foreach (string requiredFlag in choice.requiredFlags)
                {
                    if (!flagManager.HasFlag(requiredFlag))
                    {
                        conditionsMet = false;
                        break;
                    }
                }
            }

            if (conditionsMet)
            {
                LoadNode(choice.nextNodeID);
                return;
            }
        }

        // 条件を満たす移動先がなければ停止する
        Debug.Log(
            "次へ進める選択肢がありません: " +
            currentNode.nodeID
        );
    }

    public void SelectChoice(int choiceIndex)
    {
        if (choiceIndex < 0 || choiceIndex >= availableChoices.Count)
            return;

        string nextNodeID = availableChoices[choiceIndex].nextNodeID;

        LoadNode(nextNodeID);
    }
}

