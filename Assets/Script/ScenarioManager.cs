using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ScenarioManager : MonoBehaviour
{
    [Header("文章表示")]
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private GameObject choicePanel;

    [Header("選択肢")]
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;
    [SerializeField] private TMP_Text choiceDText;

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
    private string currentNodeText = "";

    private bool isTyping;
    private bool waitingForClick;
    private bool waitingForNextPage;

    private bool waitingForMouseRelease;

    private float lastClickTime = -999f;

    private Coroutine typingCoroutine;
    private Coroutine choiceButtonCoroutine;
    private bool choicesReady;

    private void Start()
    {
        choicePanel.SetActive(false);
        LoadNode(startNodeID);
    }

    private void Update()
    {
        // ロード直後、マウスボタンが離されるまで入力を受け付けない
        if (waitingForMouseRelease)
        {
            if (Input.GetMouseButton(0))
                return;

            waitingForMouseRelease = false;
            return;
        }

        if (choicePanel.activeSelf)
            return;

        if (!Input.GetMouseButtonDown(0))
            return;

        // UIをクリックした場合はシナリオを進めない
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
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

        if (choiceButtonCoroutine != null)
        {
            StopCoroutine(choiceButtonCoroutine);
            choiceButtonCoroutine = null;
        }

        choicesReady = false;

        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
        {
            Debug.LogError("ノードが見つかりません: " + nodeID);
            return;
        }

        bool isFirstVisit = visitedNodes.Add(nodeID);

        currentNodeText = currentNode.text;

        if (!isFirstVisit &&
            !string.IsNullOrEmpty(currentNode.revisitText))
        {
            currentNodeText = currentNode.revisitText;
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
        sentences.AddRange(SplitIntoSentences(currentNodeText));

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

            if (character == '「' || character == '『')
            {
                quoteStack.Push(character);
                continue;
            }

            if (character == '」' || character == '』')
            {
                if (quoteStack.Count > 0)
                {
                    char openingQuote = quoteStack.Peek();

                    if ((openingQuote == '「' && character == '」') ||
                        (openingQuote == '『' && character == '』'))
                    {
                        quoteStack.Pop();
                    }
                }

                if (quoteStack.Count == 0)
                {
                    result.Add(current.Trim());
                    current = "";
                }

                continue;
            }

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
        // セーブデータから復元した場合も、表示済み文字を維持する
        string visibleText =
            currentSentence.Substring(0, currentCharacterIndex);

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

        while (currentCharacterIndex < currentSentence.Length)
        {
            char nextCharacter =
                currentSentence[currentCharacterIndex];

            string candidate = visibleText + nextCharacter;

            scenarioText.text = pagePrefix + candidate;
            scenarioText.ForceMeshUpdate();

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
        choiceDText.gameObject.SetActive(false);

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

        if (availableChoices.Count >= 4)
        {
            choiceDText.text = availableChoices[3].choiceText;
            choiceDText.gameObject.SetActive(true);
        }

        SetChoiceButtonsInteractable(false);
        choicesReady = false;

        choicePanel.SetActive(true);

        choiceButtonCoroutine =
            StartCoroutine(EnableChoiceButtonsAfterDelay());
    }

    private IEnumerator EnableChoiceButtonsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(0.3f);

        if (choicePanel.activeSelf)
        {
            SetChoiceButtonsInteractable(true);
            choicesReady = true;
        }

        choiceButtonCoroutine = null;
    }

    private void SetChoiceButtonsInteractable(bool interactable)
    {
        SetChoiceButtonInteractable(choiceAText, interactable);
        SetChoiceButtonInteractable(choiceBText, interactable);
        SetChoiceButtonInteractable(choiceCText, interactable);
        SetChoiceButtonInteractable(choiceDText, interactable);
    }

    private void SetChoiceButtonInteractable(
        TMP_Text choiceText,
        bool interactable)
    {
        if (choiceText == null)
            return;

        Button button = choiceText.GetComponentInParent<Button>();

        if (button != null)
        {
            button.interactable =
                interactable && choiceText.gameObject.activeSelf;
        }
    }

    private void ContinueToNextNode()
    {
        if (currentNode == null)
            return;

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
        if (!choicesReady || !choicePanel.activeSelf)
            return;

        if (choiceIndex < 0 ||
            choiceIndex >= availableChoices.Count)
            return;

        LoadNode(availableChoices[choiceIndex].nextNodeID);
    }

    // ==========================================
    // セーブ機能
    // ==========================================

    public SaveData CreateSaveData(int slotIndex)
    {
        if (currentNode == null)
        {
            Debug.LogError("現在のノードが存在しないためセーブできません。");
            return null;
        }

        SaveData data = new SaveData();

        data.slotIndex = slotIndex;
        data.currentNodeID = currentNode.nodeID;

        data.currentSentenceIndex = currentSentenceIndex;
        data.currentCharacterIndex = currentCharacterIndex;

        data.currentPageText = currentPageText;
        data.currentSentence = currentSentence;
        data.nodeText = currentNodeText;

        data.displayedText = scenarioText.text;

        data.isTyping = isTyping;
        data.waitingForClick = waitingForClick;
        data.waitingForNextPage = waitingForNextPage;

        data.choicesVisible = choicePanel.activeSelf;

        data.activeFlags = flagManager.GetProgressFlagsForSave();

        data.visitedNodes = new List<string>(visitedNodes);

        return data;
    }

    // ==========================================
    // ロード機能
    // ==========================================

    public void RestoreSaveData(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("ロードするデータがありません。");
            return;
        }

        StopTyping();

        if (choiceButtonCoroutine != null)
        {
            StopCoroutine(choiceButtonCoroutine);
            choiceButtonCoroutine = null;
        }

        choicesReady = false;

        currentNode = database.GetNode(data.currentNodeID);

        if (currentNode == null)
        {
            Debug.LogError("ロード先のノードが見つかりません: " + data.currentNodeID);
            return;
        }

        // フラグを復元
        flagManager.LoadProgressFlags(data.activeFlags);

        // 訪問済みノードを復元
        visitedNodes.Clear();

        if (data.visitedNodes != null)
        {
            foreach (string nodeID in data.visitedNodes)
            {
                visitedNodes.Add(nodeID);
            }
        }

        // 現在のノード文章を復元
        currentNodeText = data.nodeText;

        sentences.Clear();
        sentences.AddRange(SplitIntoSentences(currentNodeText));

        // 文章の表示位置を復元
        currentSentenceIndex = Mathf.Clamp(
            data.currentSentenceIndex,
            0,
            sentences.Count
        );

        currentCharacterIndex = Mathf.Max(
            0,
            data.currentCharacterIndex
        );

        currentPageText = data.currentPageText;
        currentSentence = data.currentSentence;

        // 表示状態を復元
        scenarioText.text = data.displayedText;
        scenarioText.pageToDisplay = 1;
        scenarioText.ForceMeshUpdate();

        isTyping = data.isTyping;
        waitingForClick = data.waitingForClick;
        waitingForNextPage = data.waitingForNextPage;

        choicePanel.SetActive(false);

        // 選択肢表示中だった場合
        if (data.choicesVisible)
        {
            isTyping = false;
            waitingForClick = false;
            waitingForNextPage = false;

            ShowChoices();
        }
        else if (isTyping)
        {
            // タイピング途中なら残りの文字を表示する処理を再開
            typingCoroutine = StartCoroutine(TypeSentence());
        }

        // ロード直後の誤クリック防止
        waitingForMouseRelease = true;
        lastClickTime = Time.unscaledTime;

        Debug.Log("セーブデータをロードしました: " + data.currentNodeID);
    }
}