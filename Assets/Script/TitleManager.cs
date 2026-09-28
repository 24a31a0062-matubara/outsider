using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // NEW GAME
    public void NewGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    // CONTINUE
    public void ContinueGame()
    {
        Debug.Log("CONTINUE‚Í‚Ü‚¾À‘•‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
    }

    // SETTINGS
    public void OpenSettings()
    {
        Debug.Log("SETTINGS‚Í‚Ü‚¾À‘•‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ");
    }

    // EXIT
    public void ExitGame()
    {
        Debug.Log("ƒQ[ƒ€‚ğI—¹‚µ‚Ü‚·");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}