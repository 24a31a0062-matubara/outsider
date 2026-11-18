using System;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const int MaxSlots = 10;

    private string SaveDirectory =>
        Path.Combine(Application.persistentDataPath, "SaveData");

    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(
            SaveDirectory,
            "Save_" + (slotIndex + 1).ToString("00") + ".json"
        );
    }

    private void Awake()
    {
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }
    }

    // セーブ
    public void SaveToSlot(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("保存するデータがありません。");
            return;
        }

        if (data.slotIndex < 0 || data.slotIndex >= MaxSlots)
        {
            Debug.LogError("セーブ枠の番号が不正です。");
            return;
        }

        try
        {
            data.savedAt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            string json = JsonUtility.ToJson(data, true);

            File.WriteAllText(GetSavePath(data.slotIndex), json);

            Debug.Log("セーブ成功：スロット " + (data.slotIndex + 1));
        }
        catch (Exception e)
        {
            Debug.LogError("セーブに失敗しました：" + e.Message);
        }
    }

    // ロード
    public SaveData LoadFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
        {
            Debug.LogError("セーブ枠の番号が不正です。");
            return null;
        }

        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
        {
            Debug.Log("セーブデータがありません。");
            return null;
        }

        try
        {
            string json = File.ReadAllText(path);

            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null || data.version != 1)
            {
                Debug.LogError("セーブデータの形式が不正です。");
                return null;
            }

            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("ロードに失敗しました：" + e.Message);
            return null;
        }
    }

    // セーブデータの有無
    public bool HasSave(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return false;

        return File.Exists(GetSavePath(slotIndex));
    }

    // セーブデータ削除
    public void DeleteSave(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots)
            return;

        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("セーブデータを削除しました。");
        }
    }

    // 最新のセーブ枠を取得
    public int GetLatestSaveSlot()
    {
        int latestSlot = -1;
        DateTime latestTime = DateTime.MinValue;

        for (int i = 0; i < MaxSlots; i++)
        {
            SaveData data = LoadFromSlot(i);

            if (data == null)
                continue;

            if (DateTime.TryParse(data.savedAt, out DateTime saveTime))
            {
                if (saveTime > latestTime)
                {
                    latestTime = saveTime;
                    latestSlot = i;
                }
            }
        }

        return latestSlot;
    }
}