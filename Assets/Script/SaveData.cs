using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int slotIndex;
    public string savedAt;
    public int version = 1;

    public string currentNodeID;

    public int currentSentenceIndex;
    public int currentCharacterIndex;

    public string currentPageText;
    public string currentSentence;
    public string nodeText;
    public string displayedText;

    public bool isTyping;
    public bool waitingForClick;
    public bool waitingForNextPage;
    public bool choicesVisible;

    public List<string> activeFlags = new List<string>();
    public List<string> visitedNodes = new List<string>();
}