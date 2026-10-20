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
        "HeardAlibi"
    };

    private HashSet<string> flags = new HashSet<string>();

    private HashSet<string> registeredFlagSet;

    private void Awake()
    {
        registeredFlagSet = new HashSet<string>(registeredFlags);
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
        Debug.Log("フラグOFF: " + flagName);
    }

    private bool IsRegistered(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName))
        {
            Debug.LogError("フラグ名が空欄です！");
            return false;
        }

        if (!registeredFlagSet.Contains(flagName))
        {
            Debug.LogError("未登録のフラグ名です: " + flagName);
            return false;
        }

        return true;
    }
}