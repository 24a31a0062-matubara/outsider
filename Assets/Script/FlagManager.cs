using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    private HashSet<string> flags = new HashSet<string>();

    public void SetFlag(string flagName)
    {
        flags.Add(flagName);

        Debug.Log("フラグON: " + flagName);
    }

    public bool HasFlag(string flagName)
    {
        return flags.Contains(flagName);
    }

    public void RemoveFlag(string flagName)
    {
        flags.Remove(flagName);

        Debug.Log("フラグOFF: " + flagName);
    }
}