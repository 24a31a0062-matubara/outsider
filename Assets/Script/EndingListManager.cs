using System.Text;
using TMPro;
using UnityEngine;

public class EndingListManager : MonoBehaviour
{
    [SerializeField] private TMP_Text endingListText;

    private readonly string[] endingFlags =
    {
        "EndingASeen",
        "EndingBSeen",
        "TrueEndingSeen"
    };

    private readonly string[] endingNames =
    {
        "エンディング A",
        "エンディング B",
        "トゥルーエンド"
    };

    private void Start()
    {
        ShowEndingList();
    }

    private void ShowEndingList()
    {
        StringBuilder text = new StringBuilder();

        for (int i = 0; i < endingFlags.Length; i++)
        {
            bool isUnlocked =
                PlayerPrefs.GetInt("NOEx_Ending_" + endingFlags[i], 0) == 1;

            if (isUnlocked)
            {
                text.AppendLine("・" + endingNames[i]);
            }
            else
            {
                text.AppendLine("・？？？");
            }
        }

        endingListText.text = text.ToString();
    }
}