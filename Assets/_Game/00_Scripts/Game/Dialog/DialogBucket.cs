using UnityEngine;
using Slafurry.Utils.Attributes;

[GameAssetCreator("Game/Dialog", "Dialog Bucket", order: 1)]
[CreateAssetMenu(fileName = "NewDialogBucket", menuName = "Game/Dialog/Bucket")]
public class DialogBucket : ScriptableObject
{
    public Dialog[] dialogs;
}
