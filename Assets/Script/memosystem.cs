using System;
using System.IO;
using System.Collections;
using UnityEngine;
using TMPro;

public class memosystem : MonoBehaviour
{
    [Header("MEMO Panel")]
    [SerializeField] private GameObject memoPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField victimInput;
    [SerializeField] private TMP_InputField suspectInput;
    [SerializeField] private TMP_InputField evidenceInput;
    [SerializeField] private TMP_InputField questionsInput;
    [SerializeField] private TMP_InputField freeInput;

    private TMP_InputField selectedInput;

    private string savePath;
    private Coroutine saveCoroutine;

    [Serializable]
    private class MemoData
    {
        public string victim = "";
        public string suspect = "";
        public string evidence = "";
        public string questions = "";
        public string free = "";
    }

    private MemoData memoData = new MemoData();

    private void Awake()
    {
        savePath = Path.Combine(
            Application.persistentDataPath,
            "memo.json"
        );

        SetupInput(victimInput);
        SetupInput(suspectInput);
        SetupInput(evidenceInput);
        SetupInput(questionsInput);
        SetupInput(freeInput);

        LoadMemo();
    }

    private void SetupInput(TMP_InputField input)
    {
        if (input == null) return;

        input.characterLimit = 1000;

        input.onValueChanged.AddListener(OnTextChanged);
        input.onSelect.AddListener(OnInputSelected);
        input.onDeselect.AddListener(OnInputDeselected);
    }

    private void OnTextChanged(string text)
    {
        if (saveCoroutine != null)
        {
            StopCoroutine(saveCoroutine);
        }

        saveCoroutine = StartCoroutine(AutoSave());
    }

    private IEnumerator AutoSave()
    {
        yield return new WaitForSecondsRealtime(1f);

        SaveMemo();

        saveCoroutine = null;
    }

    private void OnInputSelected(string text)
    {
        selectedInput = GetSelectedInput();
    }

    private TMP_InputField GetSelectedInput()
    {
        TMP_InputField[] inputs =
        {
            victimInput,
            suspectInput,
            evidenceInput,
            questionsInput,
            freeInput
        };

        foreach (TMP_InputField input in inputs)
        {
            if (input != null && input.isFocused)
            {
                return input;
            }
        }

        return null;
    }

    private void OnInputDeselected(string text)
    {
        if (selectedInput != null && !selectedInput.isFocused)
        {
            selectedInput = null;
        }
    }

    public void OpenMemo()
    {
        memoPanel.SetActive(true);
    }

    public void CloseMemo()
    {
        SaveMemo();
        memoPanel.SetActive(false);
    }

    public void ClearSelected()
    {
        if (selectedInput == null)
        {
            Debug.Log("消去する入力欄が選択されていません。");
            return;
        }

        selectedInput.text = "";
        SaveMemo();
    }

    public void ClearAll()
    {
        victimInput.text = "";
        suspectInput.text = "";
        evidenceInput.text = "";
        questionsInput.text = "";
        freeInput.text = "";

        SaveMemo();
    }

    private void SaveMemo()
    {
        memoData.victim = victimInput.text;
        memoData.suspect = suspectInput.text;
        memoData.evidence = evidenceInput.text;
        memoData.questions = questionsInput.text;
        memoData.free = freeInput.text;

        string json = JsonUtility.ToJson(memoData, true);

        try
        {
            File.WriteAllText(savePath, json);
            Debug.Log("MEMOを保存しました。");
        }
        catch (Exception e)
        {
            Debug.LogError("MEMOの保存に失敗しました: " + e.Message);
        }
    }

    private void LoadMemo()
    {
        if (!File.Exists(savePath))
        {
            return;
        }

        try
        {
            string json = File.ReadAllText(savePath);
            memoData = JsonUtility.FromJson<MemoData>(json);

            if (memoData == null)
            {
                memoData = new MemoData();
            }

            victimInput.text = memoData.victim;
            suspectInput.text = memoData.suspect;
            evidenceInput.text = memoData.evidence;
            questionsInput.text = memoData.questions;
            freeInput.text = memoData.free;

            Debug.Log("MEMOを読み込みました。");
        }
        catch (Exception e)
        {
            Debug.LogError("MEMOの読み込みに失敗しました: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        SaveMemo();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveMemo();
        }
    }
}