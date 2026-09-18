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
    /// Editor window for batch modifying UI object positions and sizes across multiple scenes.
    /// </summary>
    public class BatchUIPositionEditor : EditorWindow
    {
        // Target configuration class
        [Serializable]
        public class TargetConfig
        {
            public bool enabled = true;
            public string targetName = "";
            public MatchMode matchMode = MatchMode.ExactName;
            public string hierarchyPath = "";
            public Vector2 anchoredPosition = Vector2.zero;
            public bool preserveZ = true;
            public Vector2 sizeDelta = Vector2.zero;
            public bool preserveSize = true;
        }

        public enum MatchMode
        {
            ExactName,
            ExactHierarchyPath
        }

        // UI state
        private Vector2 scrollPosition;
        private bool includeInactiveObjects = true;
        private bool warnOnMultipleMatches = true;
        private SceneSelectionMode sceneSelectionMode = SceneSelectionMode.Folder;
        private string scenesFolderPath = "Assets/Scenes";
        
        private List<TargetConfig> targets = new List<TargetConfig>();
        
        // Operation state
        private bool isProcessing = false;
        private string progressInfo = "";
        private float progressValue = 0f;

        [MenuItem("Tools/UI/Batch UI Position Editor")]
        public static void ShowWindow()
        {
            GetWindow<BatchUIPositionEditor>("Batch UI Position Editor");
        }

        private void OnEnable()
        {
            LoadTargets();
            LoadSettings();
        }

        private void OnDisable()
        {
            SaveTargets();
            SaveSettings();
        }

        private void OnGUI()
        {
            if (isProcessing)
            {
                DrawProcessingState();
                return;
            }

            DrawMainUI();
        }

        private void DrawMainUI()
        {
            GUILayout.Label("Batch UI Position Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawSceneSelection();
            EditorGUILayout.Space();

            DrawTargetsSection();
            EditorGUILayout.Space();

            DrawOptions();
            EditorGUILayout.Space();

            DrawActionButtons();
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
                        // Convert to relative path
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

        private void DrawTargetsSection()
        {
            EditorGUILayout.LabelField("Targets", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            for (int i = 0; i < targets.Count; i++)
            {
                DrawTargetItem(i);
            }
            
            EditorGUILayout.EndScrollView();
            
            if (GUILayout.Button("+ Add Target"))
            {
                targets.Add(new TargetConfig());
            }
        }

        private void DrawTargetItem(int index)
        {
            TargetConfig target = targets[index];
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();
            
            target.enabled = EditorGUILayout.Toggle(target.enabled, GUILayout.Width(20));
            
            EditorGUILayout.LabelField($"Target {index + 1}", GUILayout.Width(80));
            
            if (GUILayout.Button("Duplicate", GUILayout.Width(70)))
            {
                TargetConfig duplicate = new TargetConfig
                {
                    enabled = target.enabled,
                    targetName = target.targetName,
                    matchMode = target.matchMode,
                    hierarchyPath = target.hierarchyPath,
                    anchoredPosition = target.anchoredPosition,
                    preserveZ = target.preserveZ,
                    sizeDelta = target.sizeDelta,
                    preserveSize = target.preserveSize
                };
                targets.Insert(index + 1, duplicate);
            }
            
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                targets.RemoveAt(index);
                return; // Exit early since we modified the list
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Match Mode:", GUILayout.Width(80));
            target.matchMode = (MatchMode)EditorGUILayout.EnumPopup(target.matchMode, GUILayout.Width(120));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (target.matchMode == MatchMode.ExactName)
            {
                EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
                target.targetName = EditorGUILayout.TextField(target.targetName);
            }
            else
            {
                EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
                target.hierarchyPath = EditorGUILayout.TextField(target.hierarchyPath);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position:", GUILayout.Width(60));
            target.anchoredPosition.x = EditorGUILayout.FloatField(target.anchoredPosition.x, GUILayout.Width(50));
            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            target.anchoredPosition.y = EditorGUILayout.FloatField(target.anchoredPosition.y, GUILayout.Width(50));
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            target.preserveZ = EditorGUILayout.ToggleLeft("Preserve Z", target.preserveZ);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Size:", GUILayout.Width(60));
            target.sizeDelta.x = EditorGUILayout.FloatField(target.sizeDelta.x, GUILayout.Width(50));
            EditorGUILayout.LabelField("W", GUILayout.Width(15));
            target.sizeDelta.y = EditorGUILayout.FloatField(target.sizeDelta.y, GUILayout.Width(50));
            EditorGUILayout.LabelField("H", GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            target.preserveSize = EditorGUILayout.ToggleLeft("Preserve Size", target.preserveSize);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawOptions()
        {
            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            
            includeInactiveObjects = EditorGUILayout.ToggleLeft("Include Inactive Objects", includeInactiveObjects);
            warnOnMultipleMatches = EditorGUILayout.ToggleLeft("Warn on Multiple Matches", warnOnMultipleMatches);
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Preview Matches"))
            {
                PreviewMatches();
            }
            
            if (GUILayout.Button("Move Objects Across Scenes"))
            {
                StartBatchOperation();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawProcessingState()
        {
            EditorGUILayout.LabelField("Processing Scenes...", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(progressInfo);
            
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), progressValue, $"{progressValue * 100:0}%");
            
            if (GUILayout.Button("Cancel"))
            {
                CancelOperation();
            }
        }

        private void PreviewMatches()
        {
            // Clear console for clean preview
            Debug.ClearDeveloperConsole();
            
            // Validate configuration
            if (!ValidateConfiguration())
                return;
            
            // Get scenes to process
            List<string> sceneGuids = GetScenesToProcess();
            if (sceneGuids.Count == 0)
            {
                EditorUtility.DisplayDialog("No Scenes Found", "No scenes found matching the selection criteria.", "OK");
                return;
            }
            
            // Store current scene to restore later
            string originalScenePath = SceneManager.GetActiveScene().path;
            
            int totalMatches = 0;
            Dictionary<string, int> targetMatches = new Dictionary<string, int>();
            Dictionary<string, int> targetSizeChanges = new Dictionary<string, int>();
            
            // Initialize target match counts
            foreach (var target in targets.Where(t => t.enabled))
            {
                string targetId = target.matchMode == MatchMode.ExactName ? 
                    $"Name: {target.targetName}" : 
                    $"Path: {target.hierarchyPath}";
                targetMatches[targetId] = 0;
                targetSizeChanges[targetId] = 0;
            }
            
            // Process each scene
            foreach (string guid in sceneGuids)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                
                if (!scene.IsValid())
                {
                    Debug.LogWarning($"[Batch UI] Failed to open scene: {scenePath}");
                    continue;
                }
                
                // Find root game objects
                GameObject[] rootObjects = scene.GetRootGameObjects();
                
                foreach (GameObject rootObj in rootObjects)
                {
                    Transform[] transforms = rootObj.GetComponentsInChildren<Transform>(includeInactiveObjects);
                    
                    foreach (Transform transform in transforms)
                    {
                        GameObject go = transform.gameObject;
                        
                        // Check each enabled target
                        foreach (TargetConfig target in targets.Where(t => t.enabled))
                        {
                            bool matches = false;
                            
                            if (target.matchMode == MatchMode.ExactName)
                            {
                                matches = go.name == target.targetName;
                            }
                            else // ExactHierarchyPath
                            {
                                string path = GetHierarchyPath(transform);
                                matches = path == target.hierarchyPath;
                            }
                            
                            if (matches)
                            {
                                totalMatches++;
                                string targetId = target.matchMode == MatchMode.ExactName ? 
                                    $"Name: {target.targetName}" : 
                                    $"Path: {target.hierarchyPath}";
                                
                                if (targetMatches.ContainsKey(targetId))
                                    targetMatches[targetId]++;
                                
                                // Check if size will be changed
                                if (!target.preserveSize)
                                {
                                    if (targetSizeChanges.ContainsKey(targetId))
                                        targetSizeChanges[targetId]++;
                                }
                                
                                // Log the match
                                Debug.Log($"[Batch UI Preview] Found: {scenePath} / {GetHierarchyPath(transform)}");
                            }
                        }
                    }
                }
                
                // Close the scene if it wasn't originally open
                if (scenePath != originalScenePath &&
                    EditorSceneManager.GetSceneByName(scene.name).IsValid() &&
                    !EditorSceneManager.GetSceneByName(scene.name).isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
            
            // Restore original scene
            if (!string.IsNullOrEmpty(originalScenePath))
            {
                EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
            }
            
            // Show preview results
            string previewResult = $"Preview Complete\n\nScenes checked: {sceneGuids.Count}\nTotal matches found: {totalMatches}\n\n";
            
            foreach (var kvp in targetMatches)
            {
                int sizeChanges = targetSizeChanges.ContainsKey(kvp.Key) ? targetSizeChanges[kvp.Key] : 0;
                previewResult += $"{kvp.Key}: {kvp.Value} matches";
                if (!targets.First(t => t.enabled && 
                                    ((t.matchMode == MatchMode.ExactName && t.targetName == kvp.Key.Split(':')[1].Trim()) || 
                                     (t.matchMode == MatchMode.ExactHierarchyPath && t.hierarchyPath == kvp.Key.Split(':')[1].Trim()))).preserveSize)
                {
                    previewResult += $" (size will be changed for {sizeChanges} objects)";
                }
                previewResult += "\n";
            }
            
            EditorUtility.DisplayDialog("Preview Results", previewResult, "OK");
        }

        private void StartBatchOperation()
        {
            // Validate configuration
            if (!ValidateConfiguration())
                return;
            
            // Check for unsaved changes
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // User chose to save or cancel, proceed only if they saved
                if (EditorSceneManager.GetActiveScene().isDirty)
                {
                    // User cancelled the save operation
                    return;
                }
            }
            
            // Get scenes to process
            List<string> sceneGuids = GetScenesToProcess();
            if (sceneGuids.Count == 0)
            {
                EditorUtility.DisplayDialog("No Scenes Found", "No scenes found matching the selection criteria.", "OK");
                return;
            }
            
            // Show confirmation dialog
            int enabledTargets = targets.Count(t => t.enabled);
            string message = $"Batch UI Position and Size Update\n\n" +
                           $"Scenes: {sceneGuids.Count}\n" +
                           $"Targets: {enabledTargets}\n\n" +
                           $"This operation will modify and save matching RectTransform positions and sizes in the selected scenes.\n" +
                           $"Sprites and other object properties will not be changed.\n\n" +
                           $"Continue?";
                           
            if (!EditorUtility.DisplayDialog("Batch UI Position and Size Update", message, "Apply Changes", "Cancel"))
            {
                return;
            }
            
            // Start processing
            isProcessing = true;
            progressValue = 0f;
            progressInfo = "Starting batch operation...";
            
            // Begin the actual processing in a delayed call to allow UI to update
            EditorApplication.update += ProcessBatchOperation;
        }

        private void ProcessBatchOperation()
        {
            if (!isProcessing)
                return;
                
            try
            {
                // Get scenes to process
                List<string> sceneGuids = GetScenesToProcess();
                
                // Store current scene to restore later
                string originalScenePath = SceneManager.GetActiveScene().path;
                Scene originalScene = EditorSceneManager.GetActiveScene();
                
                int scenesProcessed = 0;
                int scenesModified = 0;
                int totalObjectsFound = 0;
                int totalObjectsPositionChanged = 0;
                int totalObjectsSizeChanged = 0;
                int totalObjectsSkipped = 0;
                int totalObjectsAlreadyCorrect = 0;
                Dictionary<string, int> targetStats = new Dictionary<string, int>();
                Dictionary<string, int> targetPositionChanged = new Dictionary<string, int>();
                Dictionary<string, int> targetSizeChanged = new Dictionary<string, int>();
                Dictionary<string, int> targetSkipped = new Dictionary<string, int>();
                Dictionary<string, int> targetAlreadyCorrect = new Dictionary<string, int>();
                
                // Initialize target stats
                foreach (var target in targets.Where(t => t.enabled))
                {
                    string targetId = target.matchMode == MatchMode.ExactName ? 
                        $"Name: {target.targetName}" : 
                        $"Path: {target.hierarchyPath}";
                    targetStats[targetId] = 0;
                    targetPositionChanged[targetId] = 0;
                    targetSizeChanged[targetId] = 0;
                    targetSkipped[targetId] = 0;
                    targetAlreadyCorrect[targetId] = 0;
                }
                
                // Process each scene
                for (int i = 0; i < sceneGuids.Count; i++)
                {
                    if (!isProcessing) // Check for cancellation
                        break;
                        
                    string guid = sceneGuids[i];
                    string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                    
                    progressValue = (float)i / sceneGuids.Count;
                    progressInfo = $"Processing scene {i + 1} / {sceneGuids.Count}\n{scenePath}";
                    
                    Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                    
                    if (!scene.IsValid())
                    {
                        Debug.LogError($"[Batch UI] Failed to open scene: {scenePath}");
                        continue;
                    }
                    
                    bool sceneModified = false;
                    
                    // Find root game objects
                    GameObject[] rootObjects = scene.GetRootGameObjects();
                    
                    foreach (GameObject rootObj in rootObjects)
                    {
                        Transform[] transforms = rootObj.GetComponentsInChildren<Transform>(includeInactiveObjects);
                        
                        foreach (Transform transform in transforms)
                        {
                            GameObject go = transform.gameObject;
                            
                            // Check each enabled target
                            foreach (TargetConfig target in targets.Where(t => t.enabled))
                            {
                                bool matches = false;
                                
                                if (target.matchMode == MatchMode.ExactName)
                                {
                                    matches = go.name == target.targetName;
                                }
                                else // ExactHierarchyPath
                                {
                                    string path = GetHierarchyPath(transform);
                                    matches = path == target.hierarchyPath;
                                }
                                
                                if (matches)
                                {
                                    totalObjectsFound++;
                                    string targetId = target.matchMode == MatchMode.ExactName ? 
                                        $"Name: {target.targetName}" : 
                                        $"Path: {target.hierarchyPath}";
                                    
                                    if (targetStats.ContainsKey(targetId))
                                        targetStats[targetId]++;
                                    
                                    // Check if object has RectTransform
                                    RectTransform rectTransform = go.GetComponent<RectTransform>();
                                    if (rectTransform != null)
                                    {
                                        bool objectModified = false;
                                        
                                        // Record undo
                                        Undo.RecordObject(rectTransform, "Batch Move UI Object");
                                        
                                        // Modify position if not preserving
                                        if (!target.preserveZ || 
                                            rectTransform.anchoredPosition.x != target.anchoredPosition.x || 
                                            rectTransform.anchoredPosition.y != target.anchoredPosition.y)
                                        {
                                            Vector3 position = rectTransform.anchoredPosition3D;
                                            position.x = target.anchoredPosition.x;
                                            position.y = target.anchoredPosition.y;
                                            
                                            if (!target.preserveZ)
                                                position.z = 0f;
                                                
                                            rectTransform.anchoredPosition3D = position;
                                            
                                            totalObjectsPositionChanged++;
                                            if (targetPositionChanged.ContainsKey(targetId))
                                                targetPositionChanged[targetId]++;
                                            
                                            objectModified = true;
                                            
                                            // Log the position change
                                            Debug.Log($"[Batch UI] Moved: {scenePath} / {GetHierarchyPath(transform)} " +
                                                    $"({position.x}, {position.y})");
                                        }
                                        
                                        // Modify size if not preserving
                                        if (!target.preserveSize && 
                                            (rectTransform.sizeDelta.x != target.sizeDelta.x || 
                                             rectTransform.sizeDelta.y != target.sizeDelta.y))
                                        {
                                            rectTransform.sizeDelta = target.sizeDelta;
                                            
                                            totalObjectsSizeChanged++;
                                            if (targetSizeChanged.ContainsKey(targetId))
                                                targetSizeChanged[targetId]++;
                                            
                                            objectModified = true;
                                            
                                            // Log the size change
                                            Debug.Log($"[Batch UI] Resized: {scenePath} / {GetHierarchyPath(transform)} " +
                                                    $"({rectTransform.sizeDelta.x}, {rectTransform.sizeDelta.y})");
                                        }
                                        
                                        if (objectModified)
                                        {
                                            sceneModified = true;
                                        }
                                        else
                                        {
                                            totalObjectsAlreadyCorrect++;
                                            if (targetAlreadyCorrect.ContainsKey(targetId))
                                                targetAlreadyCorrect[targetId]++;
                                        }
                                    }
                                    else
                                    {
                                        totalObjectsSkipped++;
                                        if (targetSkipped.ContainsKey(targetId))
                                            targetSkipped[targetId]++;

                                        Debug.LogWarning($"[Batch UI] Target '{go.name}' found in {scenePath} " +
                                                       $"but it does not have a RectTransform and was skipped.");
                                    }
                                }
                            }
                        }
                    }
                    
                    // Save scene if modified
                    if (sceneModified)
                    {
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        scenesModified++;
                    }
                    
                    scenesProcessed++;
                }
                
                // Restore original scene
                if (!string.IsNullOrEmpty(originalScenePath))
                {
                    EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
                }
                
                // Show final summary
                string summary = $"Batch Operation Complete\n\n" +
                               $"Scenes processed: {scenesProcessed}\n" +
                               $"Scenes modified: {scenesModified}\n\n" +
                               $"Objects found: {totalObjectsFound}\n" +
                               $"Objects position changed: {totalObjectsPositionChanged}\n" +
                               $"Objects size changed: {totalObjectsSizeChanged}\n" +
                               $"Skipped (no RectTransform): {totalObjectsSkipped}\n" +
                               $"Already at target: {totalObjectsAlreadyCorrect}\n\n";

                foreach (var kvp in targetStats)
                {
                    int posChanged = targetPositionChanged.ContainsKey(kvp.Key) ? targetPositionChanged[kvp.Key] : 0;
                    int sizeChanged = targetSizeChanged.ContainsKey(kvp.Key) ? targetSizeChanged[kvp.Key] : 0;
                    int skipped = targetSkipped.ContainsKey(kvp.Key) ? targetSkipped[kvp.Key] : 0;
                    int already = targetAlreadyCorrect.ContainsKey(kvp.Key) ? targetAlreadyCorrect[kvp.Key] : 0;
                    summary += $"{kvp.Key}\n" +
                               $"    Found: {kvp.Value}\n" +
                               $"    Position changed: {posChanged}\n" +
                               $"    Size changed: {sizeChanged}\n" +
                               $"    Skipped (no RectTransform): {skipped}\n" +
                               $"    Already at target: {already}\n\n";
                }
                
                EditorUtility.DisplayDialog("Batch Operation Complete", summary, "OK");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Batch UI] Error during batch operation: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"An error occurred: {e.Message}", "OK");
            }
            finally
            {
                // Clean up
                isProcessing = false;
                EditorApplication.update -= ProcessBatchOperation;
            }
        }

        private void CancelOperation()
        {
            isProcessing = false;
            EditorApplication.update -= ProcessBatchOperation;
            
            // Try to restore original scene if possible
            // Note: This is a simplified approach - in a real implementation,
            // we would track the original scene more carefully
            EditorUtility.DisplayDialog("Cancelled", "Batch operation cancelled.", "OK");
        }

        private bool ValidateConfiguration()
        {
            // Check if any targets are enabled
            if (!targets.Any(t => t.enabled))
            {
                EditorUtility.DisplayDialog("No Targets Enabled", "Please enable at least one target configuration.", "OK");
                return false;
            }
            
            // Check for empty target names/paths
            foreach (TargetConfig target in targets.Where(t => t.enabled))
            {
                if (target.matchMode == MatchMode.ExactName && string.IsNullOrEmpty(target.targetName))
                {
                    EditorUtility.DisplayDialog("Invalid Target", "Target name cannot be empty for Exact Name matching mode.", "OK");
                    return false;
                }
                
                if (target.matchMode == MatchMode.ExactHierarchyPath && string.IsNullOrEmpty(target.hierarchyPath))
                {
                    EditorUtility.DisplayDialog("Invalid Target", "Hierarchy path cannot be empty for Exact Hierarchy Path matching mode.", "OK");
                    return false;
                }
            }
            
            return true;
        }

        private List<string> GetScenesToProcess()
        {
            List<string> sceneGuids = new List<string>();
            
            if (sceneSelectionMode == SceneSelectionMode.AllProjectScenes)
            {
                // Find all scenes in the project
                string[] guids = AssetDatabase.FindAssets("t:Scene");
                sceneGuids.AddRange(guids);
            }
            else if (sceneSelectionMode == SceneSelectionMode.Folder)
            {
                // Find scenes in the specified folder (recursive)
                if (!string.IsNullOrEmpty(scenesFolderPath))
                {
                    // Ensure the path is relative to Assets folder
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

        private void LoadTargets()
        {
            // In a full implementation, we would load targets from EditorPrefs or a ScriptableObject
            // For now, we'll start with an empty list
            // This is a placeholder for persistence implementation
        }

        private void SaveTargets()
        {
            // In a full implementation, we would save targets to EditorPrefs or a ScriptableObject
            // This is a placeholder for persistence implementation
        }

        private void LoadSettings()
        {
            // Load editor preferences
            includeInactiveObjects = EditorPrefs.GetString("BatchUIEditor_IncludeInactive", "true") == "true";
            warnOnMultipleMatches = EditorPrefs.GetString("BatchUIEditor_WarnOnMultipleMatches", "true") == "true";
            sceneSelectionMode = (SceneSelectionMode)EditorPrefs.GetInt("BatchUIEditor_SceneSelectionMode", (int)SceneSelectionMode.Folder);
            scenesFolderPath = EditorPrefs.GetString("BatchUIEditor_ScenesFolder", "Assets/Scenes");
        }

        private void SaveSettings()
        {
            // Save editor preferences
            EditorPrefs.SetString("BatchUIEditor_IncludeInactive", includeInactiveObjects.ToString());
            EditorPrefs.SetString("BatchUIEditor_WarnOnMultipleMatches", warnOnMultipleMatches.ToString());
            EditorPrefs.SetInt("BatchUIEditor_SceneSelectionMode", (int)sceneSelectionMode);
            EditorPrefs.SetString("BatchUIEditor_ScenesFolder", scenesFolderPath);
        }

        private enum SceneSelectionMode
        {
            AllProjectScenes,
            Folder
        }
    }
}
