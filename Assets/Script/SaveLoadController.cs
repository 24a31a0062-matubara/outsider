using UnityEngine;
using TMPro;

public class SaveLoadController : MonoBehaviour
{
    [Header("各マネージャー")]
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private ScenarioManager scenarioManager;

    [Header("スロット情報表示（01～10の順番）")]
    [SerializeField] private TMP_Text[] slotInfoTexts = new TMP_Text[10];

    // 現在の操作モード
    private enum SaveLoadMode
    {
        Save,
        Load
    }

    private SaveLoadMode currentMode;

    private void Start()
    {
        RefreshAllSlotInfo();
    }

    // セーブモードに切り替え
    public void SetSaveMode()
    {
        currentMode = SaveLoadMode.Save;
        RefreshAllSlotInfo();

        Debug.Log("セーブモードに切り替えました。");
    }

    // ロードモードに切り替え
    public void SetLoadMode()
    {
        currentMode = SaveLoadMode.Load;
        RefreshAllSlotInfo();

        Debug.Log("ロードモードに切り替えました。");
    }

    // スロット選択時の処理
    public void SelectSlot(int slotIndex)
    {
        if (currentMode == SaveLoadMode.Save)
        {
            SaveSlot(slotIndex);
        }
        else
        {
            LoadSlot(slotIndex);
        }
    }

    // セーブ
    private void SaveSlot(int slotIndex)
    {
        SaveData data = scenarioManager.CreateSaveData(slotIndex);

        if (data == null)
        {
            Debug.LogError("セーブデータの作成に失敗しました。");
            return;
        }

        saveManager.SaveToSlot(data);

        RefreshAllSlotInfo();
    }

    // ロード
    private void LoadSlot(int slotIndex)
    {
        SaveData data = saveManager.LoadFromSlot(slotIndex);

        if (data == null)
        {
            Debug.LogWarning("このスロットにはセーブデータがありません。");
            return;
        }

        scenarioManager.RestoreSaveData(data);
    }

    // セーブデータがあるか確認
    public bool HasSave(int slotIndex)
    {
        return saveManager.HasSave(slotIndex);
    }

    // セーブデータ削除
    public void DeleteSlot(int slotIndex)
    {
        saveManager.DeleteSave(slotIndex);
        RefreshAllSlotInfo();
    }

    // 全スロットの表示更新
    public void RefreshAllSlotInfo()
    {
        for (int i = 0; i < slotInfoTexts.Length; i++)
        {
            RefreshSlotInfo(i);
        }
    }

    // 個別スロットの表示更新
    private void RefreshSlotInfo(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotInfoTexts.Length)
            return;

        if (slotInfoTexts[slotIndex] == null)
            return;

        SaveData data = saveManager.LoadFromSlot(slotIndex);

        if (data == null)
        {
            slotInfoTexts[slotIndex].text = "EMPTY";
        }
        else
        {
            slotInfoTexts[slotIndex].text = data.savedAt;
        }
    }
}