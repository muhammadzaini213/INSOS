using UnityEngine;

[System.Serializable]
public struct Dialog
{
    public string name;
    public string dialog;

    [Header("Boy Sprite")]
    public Sprite boySprite;

    [Header("Girl Sprite")]
    public Sprite girlSprite;

    [Header("Events")]
    public bool fireOnNewLine;

    public Sprite GetSprite(bool isBoy) => isBoy ? boySprite : girlSprite;
}
