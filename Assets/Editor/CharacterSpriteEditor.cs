using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Slafurry.Game;
using Slafurry.System.Player;

namespace Slafurry.Editor
{
    [CustomEditor(typeof(CharacterSprite))]
    public class CharacterSpriteEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            CharacterSprite characterSprite = (CharacterSprite)target;

            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            Gender currentGender = PlayerData.CurrentGender;
            Image image = characterSprite.GetComponent<Image>();

            if (image != null)
            {
                EditorGUILayout.LabelField("Current Preview", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Gender:", currentGender.ToString());

                Sprite currentSprite = currentGender == Gender.Boy
                    ? serializedObject.FindProperty("boySprite").objectReferenceValue as Sprite
                    : serializedObject.FindProperty("girlSprite").objectReferenceValue as Sprite;

                if (currentSprite != null)
                {
                    Rect rect = GUILayoutUtility.GetRect(100, 100);
                    EditorGUI.DrawPreviewTexture(rect, currentSprite.texture);
                }
            }

            if (GUILayout.Button("Apply Now"))
            {
                characterSprite.ApplySprite();
            }
        }
    }
}
