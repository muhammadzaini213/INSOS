using UnityEditor;
using UnityEngine;
using Slafurry.System.Player;

namespace Slafurry.Editor
{
    public class PlayerDataEditor : EditorWindow
    {
        [MenuItem("Slafurry/Player Data")]
        public static void ShowWindow()
        {
            var window = GetWindow<PlayerDataEditor>();
            window.titleContent = new GUIContent("Player Data");
            window.minSize = new Vector2(250, 150);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Current Gender", EditorStyles.boldLabel);

            Gender current = PlayerData.CurrentGender;
            EditorGUILayout.LabelField("Status:", current.ToString());

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            bool isBoy = current == Gender.Boy;
            bool isGirl = current == Gender.Girl;

            GUI.enabled = !isBoy;
            if (GUILayout.Button("Set Boy", GUILayout.Height(30)))
            {
                PlayerData.SetBoy();
                Repaint();
            }

            GUI.enabled = !isGirl;
            if (GUILayout.Button("Set Girl", GUILayout.Height(30)))
            {
                PlayerData.SetGirl();
                Repaint();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Changing gender will update all CharacterSprite components in the scene at runtime.",
                MessageType.Info);
        }
    }
}
