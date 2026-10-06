using UnityEngine;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private GameObject choicePanel;

    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;

    [SerializeField] private ScenarioDatabase database;

    [SerializeField] private string startNodeID = "START";

    [SerializeField] private FlagManager flagManager;

    private ScenarioNode currentNode;

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

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();

                scenarioText.text = currentNode.text;

                isTyping = false;
                textFinished = true;
            }
        }
    }

    public void LoadNode(string nodeID)
    {
        currentNode = database.GetNode(nodeID);

        if (currentNode == null)
            return;
        if (!string.IsNullOrEmpty(currentNode.setFlag))
        {
            flagManager.SetFlag(currentNode.setFlag);
        }

        scenarioText.text = "";

        choicePanel.SetActive(false);

        StartCoroutine(TypeText());
    }

    private System.Collections.IEnumerator TypeText()
    {
        isTyping = true;
        textFinished = false;

        foreach (char letter in currentNode.text)
        {
            scenarioText.text += letter;

            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        textFinished = true;

        ShowChoices();
    }

    private void ShowChoices()
    {
        if (currentNode.choices == null || currentNode.choices.Count == 0)
            return;

        choiceAText.gameObject.SetActive(false);
        choiceBText.gameObject.SetActive(false);
        choiceCText.gameObject.SetActive(false);

        if (currentNode.choices.Count >= 1)
        {
            choiceAText.text = currentNode.choices[0].choiceText;
            choiceAText.gameObject.SetActive(true);
        }

        if (currentNode.choices.Count >= 2)
        {
            choiceBText.text = currentNode.choices[1].choiceText;
            choiceBText.gameObject.SetActive(true);
        }

        if (currentNode.choices.Count >= 3)
        {
            choiceCText.text = currentNode.choices[2].choiceText;
            choiceCText.gameObject.SetActive(true);
        }

        choicePanel.SetActive(true);
    }

    public void SelectChoice(int choiceIndex)
    {
        if (currentNode == null)
            return;

        if (choiceIndex < 0 || choiceIndex >= currentNode.choices.Count)
            return;

        string nextNodeID = currentNode.choices[choiceIndex].nextNodeID;

        LoadNode(nextNodeID);
    }
}