using UnityEngine;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private float textSpeed = 0.05f;

    // シナリオ
    private string[] scenario =
    {
        "宇宙船は静かに航行していた。\n",
        "地球への帰還まで、あと三日。\n\n",
        "乗組員は全部で十二人。\n\n",
        "そして、この中の誰かが――。"
    };

    private int currentIndex = 0;

    private bool isTyping = false;
    private bool textFinished = false;

    // これまで表示した文章
    private string displayedText = "";

    private void Start()
    {
        ShowText();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 文字を表示中
            if (isTyping)
            {
                StopAllCoroutines();

                displayedText += scenario[currentIndex];
                scenarioText.text = displayedText;

                isTyping = false;
                textFinished = true;
            }
            // 文字を表示し終わっている
            else if (textFinished)
            {
                currentIndex++;

                if (currentIndex < scenario.Length)
                {
                    ShowText();
                }
                else
                {
                    Debug.Log("シナリオ終了");
                }
            }
        }
    }

    // 次の文章を表示
    private void ShowText()
    {
        scenarioText.text = displayedText;

        StartCoroutine(TypeText());
    }

    // 文字を1文字ずつ表示
    private System.Collections.IEnumerator TypeText()
    {
        isTyping = true;

        string text = scenario[currentIndex];
        string currentText = "";

        foreach (char letter in text)
        {
            currentText += letter;

            scenarioText.text = displayedText + currentText;

            yield return new WaitForSeconds(textSpeed);
        }

        displayedText += text;

        scenarioText.text = displayedText;

        isTyping = false;
        textFinished = true;
    }

    // 選択肢が選ばれたとき
    public void SelectChoice(int choice)
    {
        switch (choice)
        {
            case 0:
                Debug.Log("A：食堂に行く");
                break;

            case 1:
                Debug.Log("B：操縦室へ行く");
                break;

            case 2:
                Debug.Log("C：自室に戻る");
                break;
        }
    }
}