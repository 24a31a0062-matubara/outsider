using UnityEngine;
using TMPro;

public class ScenarioManagerPUROTO : MonoBehaviour
{
    [SerializeField] private TMP_Text scenarioText;
    [SerializeField] private float textSpeed = 0.05f;

    // 選択肢パネル
    [SerializeField] private GameObject choicePanel;

    // 選択肢の文字
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text choiceCText;


    // =========================
    // 最初のシナリオ
    // =========================

    private string[] scenario =
    {
        "宇宙船は静かに航行していた。\n地球への帰還まで、あと三日。",
        "乗組員は全部で十二人。\nそして、この中の誰かが――。"
    };


    // =========================
    // 最初の選択肢
    // =========================

    // A：食堂へ
    private string[] routeA =
    {
        "\n\nあなたは食堂へ向かった。",
        "\n\n食堂には、乗組員の一人がいた。",
        "\n\n彼はこちらに気づくと、ゆっくり振り向いた。"
    };

    // B：操縦室へ
    private string[] routeB =
    {
        "\n\nあなたは操縦室へ向かった。",
        "\n\n操縦室には誰もいなかった。",
        "\n\nしかし、操作パネルには不自然な記録が残っていた。"
    };

    // C：自室へ
    private string[] routeC =
    {
        "\n\nあなたは自室へ戻った。",
        "\n\n部屋の中は、出発したときと何も変わっていない。",
        "\n\n……そう思った。"
    };


    // =========================
    // Aルート・2回目の選択肢
    // =========================

    // A：話しかける
    private string[] routeAChoice =
    {
        "\n\nあなたは彼に話しかけた。",
        "\n\n彼は少し驚いたような顔をした。"
    };

    // B：様子を見る
    private string[] routeAChoice2 =
    {
        "\n\nあなたは何も言わず、彼の様子を観察した。",
        "\n\n彼は何かを隠しているようだった。"
    };


    // =========================
    // 管理用
    // =========================

    private int currentIndex = 0;
    private int routeIndex = 0;

    private string[] currentRoute;

    private bool isTyping = false;
    private bool textFinished = false;

    // 今まで表示した文章
    private string displayedText = "";


    private void Start()
    {
        // 最初は選択肢を非表示
        choicePanel.SetActive(false);

        // 最初の文章
        ShowText();
    }


    private void Update()
    {
        // 選択肢が表示されている間は文章を進めない
        if (choicePanel.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // 文字を表示中
            if (isTyping)
            {
                StopAllCoroutines();

                displayedText += GetCurrentText();
                scenarioText.text = displayedText;

                isTyping = false;
                textFinished = true;
            }

            // 文章を表示し終わっている
            else if (textFinished)
            {
                NextText();
            }
        }
    }


    // =========================
    // 現在の文章を取得
    // =========================

    private string GetCurrentText()
    {
        if (currentRoute != null)
        {
            return currentRoute[routeIndex];
        }

        return scenario[currentIndex];
    }


    // =========================
    // 次の文章へ
    // =========================

    private void NextText()
    {
        // ルート中
        if (currentRoute != null)
        {
            routeIndex++;

            if (routeIndex < currentRoute.Length)
            {
                ShowText();
            }
            else
            {
                Debug.Log("ルート終了");
            }

            return;
        }


        // 最初のシナリオ
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


    // =========================
    // 文章表示開始
    // =========================

    private void ShowText()
    {
        scenarioText.text = displayedText;

        StartCoroutine(TypeText());
    }


    // =========================
    // 文字送り
    // =========================

    private System.Collections.IEnumerator TypeText()
    {
        isTyping = true;

        string text = GetCurrentText();
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


        // =========================
        // 最初の選択肢
        // =========================

        if (currentRoute == null && currentIndex == 1)
        {
            choiceAText.text = "食堂へ行く";
            choiceBText.text = "操縦室へ行く";
            choiceCText.text = "自室へ戻る";

            choiceCText.gameObject.SetActive(true);

            choicePanel.SetActive(true);
        }


        // =========================
        // Aルートの2回目の選択肢
        // =========================

        if (currentRoute == routeA && routeIndex == 2)
        {
            choiceAText.text = "話しかける";
            choiceBText.text = "様子を見る";

            // Cボタンは今回使わない
            choiceCText.gameObject.SetActive(false);

            choicePanel.SetActive(true);
        }
    }


    // =========================
    // 選択肢を選んだとき
    // =========================

    public void SelectChoice(int choice)
    {
        // 選択肢を消す
        choicePanel.SetActive(false);


        // =========================
        // 最初の選択
        // =========================

        if (currentRoute == null)
        {
            switch (choice)
            {
                case 0:
                    currentRoute = routeA;
                    break;

                case 1:
                    currentRoute = routeB;
                    break;

                case 2:
                    currentRoute = routeC;
                    break;
            }

            routeIndex = 0;

            ShowText();

            return;
        }


        // =========================
        // Aルートの2回目の選択
        // =========================

        if (currentRoute == routeA)
        {
            switch (choice)
            {
                case 0:
                    currentRoute = routeAChoice;
                    break;

                case 1:
                    currentRoute = routeAChoice2;
                    break;
            }

            routeIndex = 0;

            ShowText();
        }
    }
}