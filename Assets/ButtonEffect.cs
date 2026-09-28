using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private float normalSize = 80f;
    [SerializeField] private float hoverSize = 100f;

    private void Start()
    {
        Debug.Log("[Hover] " + gameObject.name + " : Start");

        if (buttonText == null)
        {
            Debug.LogError("[Hover] " + gameObject.name + " : Button Textが設定されていません！");
            return;
        }

        buttonText.fontSize = normalSize;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("[Hover] " + gameObject.name + " : カーソルIN");

        if (buttonText == null)
        {
            Debug.LogError("[Hover] Button Textがありません！");
            return;
        }

        buttonText.fontSize = hoverSize;

        Debug.Log("[Hover] 文字サイズを " + hoverSize + " に変更");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("[Hover] " + gameObject.name + " : カーソルOUT");

        if (buttonText == null)
        {
            Debug.LogError("[Hover] Button Textがありません！");
            return;
        }

        buttonText.fontSize = normalSize;

        Debug.Log("[Hover] 文字サイズを " + normalSize + " に戻しました");
    }
}