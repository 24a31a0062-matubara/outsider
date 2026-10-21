using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    [Header("使用を許可するフラグ名")]
    [SerializeField]
    private List<string> registeredFlags = new List<string>()
{
"VisitedDining",
"FoundPanelRecord",
"HeardAlibi",
"EndingASeen",
"EndingBSeen",
"TrueEndingSeen"
};

private HashSet<string> flags = new HashSet<string>();
    private HashSet<string> registeredFlagSet;

    private const string EndingPrefix = "NOEx_Ending_";

    private void Awake()
    {
        registeredFlagSet = new HashSet<string>(registeredFlags);

        // 保存済みのエンディング記録を読み込む
        foreach (string flag in registeredFlags)
        {
            if (IsEndingFlag(flag) &&
                PlayerPrefs.GetInt(EndingPrefix + flag, 0) == 1)
            {
                flags.Add(flag);
            }
        }
    }

    public bool IsFlagRegistered(string flagName)
    {
        if (registeredFlagSet == null)
            registeredFlagSet = new HashSet<string>(registeredFlags);

        return !string.IsNullOrWhiteSpace(flagName)
            && registeredFlagSet.Contains(flagName);
    }

    public void SetFlag(string flagName)
    {
        if (!IsRegistered(flagName))
            return;

        flags.Add(flagName);

        // エンディングの記録だけを保存する
        if (IsEndingFlag(flagName))
        {
            PlayerPrefs.SetInt(EndingPrefix + flagName, 1);
            PlayerPrefs.Save();
        }

        Debug.Log("フラグON: " + flagName);
    }

    public bool HasFlag(string flagName)
    {
        if (!IsRegistered(flagName))
            return false;

        return flags.Contains(flagName);
    }

    public void RemoveFlag(string flagName)
    {
        if (!IsRegistered(flagName))
            return;

        flags.Remove(flagName);

        if (IsEndingFlag(flagName))
        {
            PlayerPrefs.DeleteKey(EndingPrefix + flagName);
            PlayerPrefs.Save();
        }

        Debug.Log("フラグOFF: " + flagName);
    }

    private bool IsEndingFlag(string flagName)
    {
        return flagName == "EndingASeen"
            || flagName == "EndingBSeen"
            || flagName == "TrueEndingSeen";
    }

    private bool IsRegistered(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName))
        {
            Debug.LogError("フラグ名が空欄です！");
            return false;
        }

        if (!IsFlagRegistered(flagName))
        {
            Debug.LogError("未登録のフラグ名です: " + flagName);
            return false;
        }
        return true;
    }

}
