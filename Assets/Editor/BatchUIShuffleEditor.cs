using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Slafurry.Editor
{
    /// <summary>
    /// Editor window for batch randomizing Correct/Wrong button positions across multiple scenes.
    /// Each scene is processed independently: the pair's anchored positions are swapped
    /// (either always, or per a per-scene coin flip), so players cannot memorize placement.
    /// Only RectTransform positions are modified; sprites and all other data are preserved.
    /// </summary>
    public class BatchUIShuffleEditor : EditorWindow
    {
        public enum ShuffleMode
        {
            RandomSwap,
            SwapAll
        }

        // Pair configuration
        private string correctButtonName = "Correct_Button";
        private string wrongButtonName = "Wrong_Button";
        private ShuffleMode shuffleMode = ShuffleMode.RandomSwap;
        private int seed = 0;
        private bool preserveZ = true;

        // Scene selection
        private bool includeInactiveObjects = true;
        private SceneSelectionMode sceneSelectionMode = SceneSelectionMode.Folder;
        private string scenesFolderPath = "Assets/_Game/04_Scenes";

        [MenuItem("Tools/UI/Batch UI Button Shuffler")]
        public static void ShowWindow()
        {
            GetWindow<BatchUIShuffleEditor>("Batch UI Button Shuffler");
        }

        private void OnEnable()
        {
            LoadSettings();
            if (seed == 0)
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        private void OnDisable()
        {
            SaveSettings();
        }

        private void OnGUI()
        {
            GUILayout.Label("Batch UI Button Shuffler", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Swaps the anchored positions of a Correct/Wrong button pair in each selected scene. " +
                "Only RectTransform positions change — sprites, buttons, and all other properties are preserved.",
                MessageType.Info);
            EditorGUILayout.Space();

            DrawPairConfig();
            EditorGUILayout.Space();

            DrawSceneSelection();
            EditorGUILayout.Space();

            DrawOptions();
            EditorGUILayout.Space();

            DrawActionButtons();
        }

        private void DrawPairConfig()
        {
            EditorGUILayout.LabelField("Button Pair", EditorStyles.boldLabel);

            correctButtonName = EditorGUILayout.TextField("Correct Button Name:", correctButtonName);
            wrongButtonName = EditorGUILayout.TextField("Wrong Button Name:", wrongButtonName);
            shuffleMode = (ShuffleMode)EditorGUILayout.EnumPopup("Shuffle Mode:", shuffleMode);

            EditorGUILayout.BeginHorizontal();
            seed = EditorGUILayout.IntField("Seed:", seed);
            if (GUILayout.Button("Random", GUILayout.Width(70)))
            {
                seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            if (shuffleMode == ShuffleMode.RandomSwap)
            {
                EditorGUILayout.HelpBox(
                    "RandomSwap: each scene flips its own coin (seeded). " +
                    "Roughly half the scenes swap, half stay — keep the seed to reproduce a run.",
                    MessageType.None);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "SwapAll: every scene with a complete pair is swapped. " +
                    "Run it again to swap everything back.",
                    MessageType.None);
            }
        }

        private void DrawSceneSelection()
        {
            EditorGUILayout.LabelField("Scene Selection", EditorStyles.boldLabel);

            sceneSelectionMode = (SceneSelectionMode)EditorGUILayout.EnumPopup("Mode:", sceneSelectionMode);

            if (sceneSelectionMode == SceneSelectionMode.Folder)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Folder:");
                scenesFolderPath = EditorGUILayout.TextField(scenesFolderPath);

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    string selectedPath = EditorUtility.OpenFolderPanel("Select Scenes Folder", Application.dataPath, "");
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        if (selectedPath.StartsWith(Application.dataPath))
                        {
                            scenesFolderPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                        }
                        else
                        {
                            scenesFolderPath = selectedPath;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawOptions()
        {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);

            preserveZ = EditorGUILayout.ToggleLeft("Preserve Z Position", preserveZ);
            includeInactiveObjects = EditorGUILayout.ToggleLeft("Include Inactive Objects", includeInactiveObjects);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Preview Pairs"))
            {
                PreviewPairs();
            }

            if (GUILayout.Button("Shuffle Buttons Across Scenes"))
            {
                RunShuffle();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void PreviewPairs()
        {
            Debug.ClearDeveloperConsole();

            if (!ValidateConfiguration())
                return;

            List<string> sceneGuids = GetScenesToProcess();
            if (sceneGuids.Count == 0)
            {
                EditorUtility.DisplayDialog("No Scenes Found", "No scenes found matching the selection criteria.", "OK");
                return;
            }

            int pairComplete = 0;
            int missingCorrect = 0;
            int missingWrong = 0;
            int missingBoth = 0;
            int noRect = 0;

            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                bool alreadyLoaded = EditorSceneManager.GetSceneByPath(scenePath).IsValid();
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

                if (!scene.IsValid())
                {
                    Debug.LogWarning($"[Batch Shuffle Preview] Failed to open scene: {scenePath}");
                    continue;
                }

                List<GameObject> correctMatches = FindByExactName(scene, correctButtonName);
                List<GameObject> wrongMatches = FindByExactName(scene, wrongButtonName);

                if (correctMatches.Count == 0 && wrongMatches.Count == 0)
                {
                    missingBoth++;
                    Debug.Log($"[Batch Shuffle Preview] {scenePath}: neither button found.");
                }
                else if (correctMatches.Count == 0)
                {
                    missingCorrect++;
                    Debug.LogWarning($"[Batch Shuffle Preview] {scenePath}: '{correctButtonName}' not found.");
                }
                else if (wrongMatches.Count == 0)
                {
                    missingWrong++;
                    Debug.LogWarning($"[Batch Shuffle Preview] {scenePath}: '{wrongButtonName}' not found.");
                }
                else if (correctMatches.Count != 1 || wrongMatches.Count != 1)
                {
                    Debug.LogWarning($"[Batch Shuffle Preview] {scenePath}: duplicate matches " +
                                     $"({correctButtonName}: {correctMatches.Count}, {wrongButtonName}: {wrongMatches.Count}). " +
                                     "Exact-name duplicates will be skipped — use unique names.");
                }
                else if (correctMatches[0].GetComponent<RectTransform>() == null ||
                         wrongMatches[0].GetComponent<RectTransform>() == null)
                {
                    noRect++;
                    Debug.LogWarning($"[Batch Shuffle Preview] {scenePath}: pair found but missing RectTransform.");
                }
                else
                {
                    pairComplete++;
                    Debug.Log($"[Batch Shuffle Preview] {scenePath}: pair OK " +
                              $"({GetHierarchyPath(correctMatches[0].transform)} <-> {GetHierarchyPath(wrongMatches[0].transform)})");
                }

                if (!alreadyLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }

            string result = $"Preview Complete\n\n" +
                            $"Scenes checked: {sceneGuids.Count}\n" +
                            $"Complete pairs: {pairComplete}\n" +
                            $"Missing correct button: {missingCorrect}\n" +
                            $"Missing wrong button: {missingWrong}\n" +
                            $"Missing both: {missingBoth}\n" +
                            $"Missing RectTransform: {noRect}\n\n" +
                            $"Mode: {shuffleMode}\n" +
                            $"Seed: {seed}";

            EditorUtility.DisplayDialog("Preview Results", result, "OK");
        }

        private void RunShuffle()
        {
            if (!ValidateConfiguration())
                return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    return;
                }
            }

            List<string> sceneGuids = GetScenesToProcess();
            if (sceneGuids.Count == 0)
            {
                EditorUtility.DisplayDialog("No Scenes Found", "No scenes found matching the selection criteria.", "OK");
                return;
            }

            string message = $"Batch Shuffle Correct/Wrong Buttons\n\n" +
                             $"Scenes: {sceneGuids.Count}\n" +
                             $"Pair: {correctButtonName} <-> {wrongButtonName}\n" +
                             $"Mode: {shuffleMode}\n" +
                             $"Seed: {seed}\n\n" +
                             $"This operation swaps matching RectTransform positions and saves modified scenes.\n" +
                             $"Sprites and other object properties will not be changed.\n\n" +
                             $"Continue?";

            if (!EditorUtility.DisplayDialog("Batch Shuffle Buttons", message, "Shuffle", "Cancel"))
            {
                return;
            }

            string originalScenePath = SceneManager.GetActiveScene().path;
            global::System.Random rng = new global::System.Random(seed);

            int scenesProcessed = 0;
            int scenesModified = 0;
            int swapped = 0;
            int kept = 0;
            int skipped = 0;
            int failed = 0;

            try
            {
                for (int i = 0; i < sceneGuids.Count; i++)
                {
                    string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

                    if (EditorUtility.DisplayCancelableProgressBar(
                            "Batch Shuffling Buttons",
                            $"Scene {i + 1} / {sceneGuids.Count}\n{scenePath}",
                            (float)i / sceneGuids.Count))
                    {
                        Debug.Log("[Batch Shuffle] Cancelled by user.");
                        break;
                    }

                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

                    if (!scene.IsValid())
                    {
                        failed++;
                        Debug.LogError($"[Batch Shuffle] Failed to open scene: {scenePath}");
                        continue;
                    }

                    scenesProcessed++;

                    List<GameObject> correctMatches = FindByExactName(scene, correctButtonName);
                    List<GameObject> wrongMatches = FindByExactName(scene, wrongButtonName);

                    if (correctMatches.Count != 1 || wrongMatches.Count != 1)
                    {
                        skipped++;
                        Debug.LogWarning($"[Batch Shuffle] Skipped {scenePath}: expected exactly 1 of each button " +
                                         $"({correctButtonName}: {correctMatches.Count}, {wrongButtonName}: {wrongMatches.Count}).");
                        continue;
                    }

                    RectTransform correctRect = correctMatches[0].GetComponent<RectTransform>();
                    RectTransform wrongRect = wrongMatches[0].GetComponent<RectTransform>();

                    if (correctRect == null || wrongRect == null)
                    {
                        skipped++;
                        Debug.LogWarning($"[Batch Shuffle] Skipped {scenePath}: pair found but missing RectTransform.");
                        continue;
                    }

                    bool shouldSwap = shuffleMode == ShuffleMode.SwapAll || rng.Next(2) == 0;

                    if (!shouldSwap)
                    {
                        kept++;
                        Debug.Log($"[Batch Shuffle] Kept {scenePath}: coin flip left the pair in place " +
                                  $"({GetHierarchyPath(correctRect.transform)} <-> {GetHierarchyPath(wrongRect.transform)}).");
                        continue;
                    }

                    Vector3 correctPos = correctRect.anchoredPosition3D;
                    Vector3 wrongPos = wrongRect.anchoredPosition3D;

                    if (Mathf.Approximately(correctPos.x, wrongPos.x) &&
                        Mathf.Approximately(correctPos.y, wrongPos.y))
                    {
                        kept++;
                        Debug.Log($"[Batch Shuffle] Kept {scenePath}: both buttons already share the same position.");
                        continue;
                    }

                    Undo.RecordObject(correctRect, "Batch Shuffle Buttons");
                    Undo.RecordObject(wrongRect, "Batch Shuffle Buttons");

                    Vector3 newCorrect = wrongPos;
                    Vector3 newWrong = correctPos;

                    if (preserveZ)
                    {
                        newCorrect.z = correctPos.z;
                        newWrong.z = wrongPos.z;
                    }
                    else
                    {
                        newCorrect.z = 0f;
                        newWrong.z = 0f;
                    }

                    correctRect.anchoredPosition3D = newCorrect;
                    wrongRect.anchoredPosition3D = newWrong;

                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);

                    scenesModified++;
                    swapped++;
                    Debug.Log($"[Batch Shuffle] Swapped {scenePath}: " +
                              $"{GetHierarchyPath(correctRect.transform)} <-> {GetHierarchyPath(wrongRect.transform)}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Batch Shuffle] Error during batch operation: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"An error occurred: {e.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                if (!string.IsNullOrEmpty(originalScenePath))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                }
            }

            string summary = $"Batch Shuffle Complete\n\n" +
                             $"Scenes processed: {scenesProcessed}\n" +
                             $"Scenes modified: {scenesModified}\n\n" +
                             $"Swapped: {swapped}\n" +
                             $"Kept in place: {kept}\n" +
                             $"Skipped: {skipped}\n" +
                             $"Failed scenes: {failed}\n\n" +
                             $"Mode: {shuffleMode}\n" +
                             $"Seed: {seed}";

            EditorUtility.DisplayDialog("Batch Shuffle Complete", summary, "OK");
        }

        private List<GameObject> FindByExactName(Scene scene, string objectName)
        {
            List<GameObject> matches = new List<GameObject>();

            foreach (GameObject rootObj in scene.GetRootGameObjects())
            {
                Transform[] transforms = rootObj.GetComponentsInChildren<Transform>(includeInactiveObjects);

                foreach (Transform transform in transforms)
                {
                    if (transform.gameObject.name == objectName)
                    {
                        matches.Add(transform.gameObject);
                    }
                }
            }

            return matches;
        }

        private bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(correctButtonName) || string.IsNullOrEmpty(wrongButtonName))
            {
                EditorUtility.DisplayDialog("Invalid Pair", "Both button names must be set.", "OK");
                return false;
            }

            if (correctButtonName == wrongButtonName)
            {
                EditorUtility.DisplayDialog("Invalid Pair", "Correct and wrong button names must differ.", "OK");
                return false;
            }

            return true;
        }

        private List<string> GetScenesToProcess()
        {
            List<string> sceneGuids = new List<string>();

            if (sceneSelectionMode == SceneSelectionMode.AllProjectScenes)
            {
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                sceneGuids.AddRange(guids);
            }
            else if (sceneSelectionMode == SceneSelectionMode.Folder)
            {
                if (!string.IsNullOrEmpty(scenesFolderPath))
                {
                    string searchPath = scenesFolderPath.StartsWith("Assets/") ? scenesFolderPath : $"Assets/{scenesFolderPath}";
                    string[] guids = AssetDatabase.FindAssets("t:Scene", new[] { searchPath });
                    sceneGuids.AddRange(guids);
                }
            }

            return sceneGuids;
        }

        private string GetHierarchyPath(Transform transform)
        {
            string path = transform.name;
            Transform parent = transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private void LoadSettings()
        {
            correctButtonName = EditorPrefs.GetString("BatchUIShuffle_CorrectName", "Correct_Button");
            wrongButtonName = EditorPrefs.GetString("BatchUIShuffle_WrongName", "Wrong_Button");
            shuffleMode = (ShuffleMode)EditorPrefs.GetInt("BatchUIShuffle_Mode", (int)ShuffleMode.RandomSwap);
            seed = EditorPrefs.GetInt("BatchUIShuffle_Seed", 0);
            preserveZ = EditorPrefs.GetString("BatchUIShuffle_PreserveZ", "true") == "true";
            includeInactiveObjects = EditorPrefs.GetString("BatchUIShuffle_IncludeInactive", "true") == "true";
            sceneSelectionMode = (SceneSelectionMode)EditorPrefs.GetInt("BatchUIShuffle_SceneSelectionMode", (int)SceneSelectionMode.Folder);
            scenesFolderPath = EditorPrefs.GetString("BatchUIShuffle_ScenesFolder", "Assets/_Game/04_Scenes");
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString("BatchUIShuffle_CorrectName", correctButtonName);
            EditorPrefs.SetString("BatchUIShuffle_WrongName", wrongButtonName);
            EditorPrefs.SetInt("BatchUIShuffle_Mode", (int)shuffleMode);
            EditorPrefs.SetInt("BatchUIShuffle_Seed", seed);
            EditorPrefs.SetString("BatchUIShuffle_PreserveZ", preserveZ.ToString());
            EditorPrefs.SetString("BatchUIShuffle_IncludeInactive", includeInactiveObjects.ToString());
            EditorPrefs.SetInt("BatchUIShuffle_SceneSelectionMode", (int)sceneSelectionMode);
            EditorPrefs.SetString("BatchUIShuffle_ScenesFolder", scenesFolderPath);
        }

        private enum SceneSelectionMode
        {
            AllProjectScenes,
            Folder
        }
    }
}
