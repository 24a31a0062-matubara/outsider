using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class ScenarioManager : MonoBehaviour
{
    [Header("文章表示")]
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private GameObject choicePanel;

    [Header("選択肢")]
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [Header("データ")]
    [SerializeField] private ScenarioDatabase database;
    [SerializeField] private FlagManager flagManager;

    [Header("設定")]
    [SerializeField] private string startNodeID = "START";
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private float clickCooldown = 0.2f;

    private ScenarioNode currentNode;

    private readonly List<ScenarioChoice> availableChoices =
        new List<ScenarioChoice>();

    private readonly HashSet<string> visitedNodes =
        new HashSet<string>();

    private readonly List<string> sentences =
        new List<string>();

    private int currentSentenceIndex;
    private int currentCharacterIndex;

    private string currentPageText = "";
    private string currentSentence = "";

    private bool isTyping;
    private bool waitingForClick;
    private bool waitingForNextPage;

    private float lastClickTime = -999f;

    private Coroutine typingCoroutine;

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

        // クリック連打による連続進行を防止
        if (Time.unscaledTime - lastClickTime < clickCooldown)
            return;

        lastClickTime = Time.unscaledTime;

        // 文章表示中なら、可能な範囲まで一気に表示
        if (isTyping)
        {
            CompleteCurrentSentence();
            return;
        }

        if (!waitingForClick)
            return;

        // 次のページへ進む
        if (waitingForNextPage)
        {
            currentPageText = "";
            scenarioText.text = "";
            scenarioText.pageToDisplay = 1;

            waitingForNextPage = false;

            ContinueCurrentSentenceOnNewPage();
            return;
        }

        // 次の文章へ進む
        if (currentSentenceIndex < sentences.Count)
        {
            ShowNextSentence();
            return;
        }

        // ノードの進行
        if (currentNode.progressType == NodeProgressType.ClickToContinue)
        {
            ContinueToNextNode();
        }
        else
        {
            FinishNodeText();
        }
    }

    public void LoadNode(string nodeID)
    {
        StopTyping();

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
        {
            Debug.LogError("ノードが見つかりません: " + nodeID);
            return;
        }

        bool isFirstVisit = visitedNodes.Add(nodeID);

        string textToDisplay = currentNode.text;

        if (!isFirstVisit &&
            !string.IsNullOrEmpty(currentNode.revisitText))
        {
            textToDisplay = currentNode.revisitText;
        }

        if (!string.IsNullOrEmpty(currentNode.setFlag))
        {
            flagManager.SetFlag(currentNode.setFlag);
        }

        if (currentNode.isEnding &&
            !string.IsNullOrEmpty(currentNode.endingFlag))
        {
            flagManager.SetFlag(currentNode.endingFlag);

            Debug.Log(
                "エンディングを記録しました: " +
                currentNode.endingFlag
            );
        }

        sentences.Clear();
        sentences.AddRange(SplitIntoSentences(textToDisplay));

        currentSentenceIndex = 0;
        currentCharacterIndex = 0;

        currentPageText = "";
        currentSentence = "";

        scenarioText.text = "";
        scenarioText.pageToDisplay = 1;

        choicePanel.SetActive(false);

        isTyping = false;
        waitingForClick = true;
        waitingForNextPage = false;

        if (sentences.Count == 0)
            FinishNodeText();
    }

    // 文章を句読点ごとに分割する
    // 閉じカッコは直前の文章に含める
    private List<string> SplitIntoSentences(string text)
    {
        List<string> result = new List<string>();

        if (string.IsNullOrEmpty(text))
            return result;

        string current = "";

        Stack<char> quoteStack = new Stack<char>();

        foreach (char character in text)
        {
            current += character;

            // 括弧の開始
            if (character == '「' ||
                character == '『')
            {
                quoteStack.Push(character);
                continue;
            }

            // 括弧の終了
            if (character == '」' ||
                character == '』')
            {
                if (quoteStack.Count > 0)
                {
                    char openingQuote = quoteStack.Peek();

                    // 対応する閉じ括弧の場合だけ閉じる
                    if ((openingQuote == '「' && character == '」') ||
                        (openingQuote == '『' && character == '』'))
                    {
                        quoteStack.Pop();
                    }
                }

                // 外側の会話が終了した場合
                if (quoteStack.Count == 0)
                {
                    result.Add(current.Trim());
                    current = "";
                }

                continue;
            }

            // 会話の外側にある句読点で文章を分割
            if (quoteStack.Count == 0 &&
                (character == '。' ||
                 character == '！' ||
                 character == '？' ||
                 character == '!' ||
                 character == '?'))
            {
                result.Add(current.Trim());
                current = "";
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            result.Add(current.Trim());
        }

        return result;
    }

    private void ShowNextSentence()
    {
        if (currentSentenceIndex >= sentences.Count)
        {
            FinishNodeText();
            return;
        }

        currentSentence = sentences[currentSentenceIndex];
        currentCharacterIndex = 0;

        string separator = string.IsNullOrEmpty(currentPageText)
            ? ""
            : "\n\n";

        string candidateText =
            currentPageText + separator + currentSentence;

        // 文章を追加した場合にページが増えるか確認
        scenarioText.text = candidateText;
        scenarioText.ForceMeshUpdate();

        if (scenarioText.textInfo.pageCount > 1 &&
            !string.IsNullOrEmpty(currentPageText))
        {
            scenarioText.text = currentPageText;
            scenarioText.pageToDisplay = 1;
            scenarioText.ForceMeshUpdate();

            waitingForNextPage = true;
            waitingForClick = true;
            return;
        }

        scenarioText.text = currentPageText + separator;
        scenarioText.pageToDisplay = 1;
        scenarioText.ForceMeshUpdate();

        StartTyping();
    }

    private void ContinueCurrentSentenceOnNewPage()
    {
        if (currentCharacterIndex >= currentSentence.Length)
        {
            currentSentenceIndex++;
            waitingForClick = true;
            return;
        }

        scenarioText.text = "";
        scenarioText.pageToDisplay = 1;

        StartTyping();
    }

    private void StartTyping()
    {
        isTyping = true;
        waitingForClick = false;

        typingCoroutine = StartCoroutine(TypeSentence());
    }

    private IEnumerator TypeSentence()
    {
        string visibleText = "";

        while (currentCharacterIndex < currentSentence.Length)
        {
            char nextCharacter =
                currentSentence[currentCharacterIndex];

            string candidate = visibleText + nextCharacter;

            string pagePrefix = string.IsNullOrEmpty(currentPageText)
                ? ""
                : currentPageText + "\n\n";

            scenarioText.text = pagePrefix + candidate;
            scenarioText.ForceMeshUpdate();

            // ページからはみ出す直前で停止
            if (scenarioText.textInfo.pageCount > 1)
            {
                scenarioText.text = pagePrefix + visibleText;
                scenarioText.ForceMeshUpdate();

                waitingForNextPage = true;
                isTyping = false;
                waitingForClick = true;
                typingCoroutine = null;

                yield break;
            }

            visibleText = candidate;
            currentCharacterIndex++;

            scenarioText.text = pagePrefix + visibleText;

            yield return new WaitForSeconds(textSpeed);
        }

        string separator = string.IsNullOrEmpty(currentPageText)
            ? ""
            : "\n\n";

        currentPageText += separator + visibleText;

        scenarioText.text = currentPageText;
        scenarioText.ForceMeshUpdate();

        currentSentenceIndex++;

        isTyping = false;
        waitingForClick = true;
        typingCoroutine = null;
    }

    private void CompleteCurrentSentence()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        string visibleText =
            currentSentence.Substring(0, currentCharacterIndex);

        string pagePrefix = string.IsNullOrEmpty(currentPageText)
            ? ""
            : currentPageText + "\n\n";

        // クリックされたら、表示できる範囲まで一気に表示する
        while (currentCharacterIndex < currentSentence.Length)
        {
            char nextCharacter =
                currentSentence[currentCharacterIndex];

            string candidate = visibleText + nextCharacter;

            scenarioText.text = pagePrefix + candidate;
            scenarioText.ForceMeshUpdate();

            // ページを超える場合は、そこで止める
            if (scenarioText.textInfo.pageCount > 1)
            {
                scenarioText.text = pagePrefix + visibleText;
                scenarioText.ForceMeshUpdate();

                isTyping = false;
                waitingForClick = true;
                waitingForNextPage = true;

                return;
            }

            visibleText = candidate;
            currentCharacterIndex++;
        }

        // 一文を最後まで表示できた場合
        currentPageText += pagePrefix == ""
            ? visibleText
            : "\n\n" + visibleText;

        scenarioText.text = currentPageText;
        scenarioText.ForceMeshUpdate();

        currentSentenceIndex++;

        isTyping = false;
        waitingForClick = true;
        waitingForNextPage = false;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private void FinishNodeText()
    {
        waitingForClick = false;

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
                if (choice == null)
                    continue;

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
                    availableChoices.Add(choice);
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
        if (currentNode == null)
            return;

        // ClickToContinueの場合はScenarioNodeのNext Node IDを使用
        if (currentNode.progressType == NodeProgressType.ClickToContinue)
        {
            if (string.IsNullOrWhiteSpace(currentNode.nextNodeID))
            {
                Debug.LogError(
                    currentNode.nodeID +
                    "：クリック後の移動先（Next Node ID）が設定されていません。",
                    currentNode
                );

                waitingForClick = false;
                return;
            }

            LoadNode(currentNode.nextNodeID);
            return;
        }

        Debug.LogWarning(
            "ClickToContinue以外のノードから次へ進もうとしました: " +
            currentNode.nodeID
        );
    }

    public void SelectChoice(int choiceIndex)
    {
        if (choiceIndex < 0 ||
            choiceIndex >= availableChoices.Count)
            return;

        LoadNode(availableChoices[choiceIndex].nextNodeID);
    }
}