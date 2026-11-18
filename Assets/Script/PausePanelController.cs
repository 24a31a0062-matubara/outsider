using UnityEngine;

public class PausePanelController : MonoBehaviour
{
    [SerializeField] private GameObject saveLoadPanel;

    // ポーズ画面を開く
    public void OpenPanel()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    // ポーズ画面を閉じる
    public void ClosePanel()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    // セーブ・ロード画面を開く
    public void OpenSaveLoadPanel()
    {
        saveLoadPanel.SetActive(true);
    }

    // セーブ・ロード画面を閉じる
    public void CloseSaveLoadPanel()
    {
        saveLoadPanel.SetActive(false);
    }
}