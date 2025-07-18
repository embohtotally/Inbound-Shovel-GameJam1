using UnityEngine;

// This [System.Serializable] attribute is VERY important.
// It's what allows Unity to show this in the Inspector.
[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string dialogueText;
    public Sprite speakerImageA;
    public Sprite speakerImageB;
    public bool isSpeakerAActive;
}