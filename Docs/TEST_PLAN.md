# Batch UI Position Editor - Test Plan

## Test Environment
- Unity Version: 2022.3.62f3
- Test Project: INSOS JAYA JAYA (provided project)

## Test Scenarios

### Scenario 1: Basic Position Modification
1. Create 3 test scenes with UI buttons
2. Configure target: Button name "TestButton" at position (100, 50)
3. Execute batch operation
4. Verify all TestButton instances moved to (100, 50) in all scenes
5. Verify sprites, anchors, pivot, scale, rotation unchanged

### Scenario 2: Size Modification
1. Create 3 test scenes with UI buttons of different sizes
2. Configure target: Button name "TestButton" size (120, 60), Preserve Size disabled
3. Execute batch operation
4. Verify all TestButton instances resized to (120, 60)
5. Verify position and other properties unchanged
6. Verify sprites unchanged

### Scenario 3: Combined Position and Size
1. Create 3 test scenes with UI buttons
2. Configure target: Button name "TestButton" position (150, 75) size (100, 40)
3. Execute batch operation
4. Verify all TestButton instances moved to (150, 75) and sized to (100, 40)
5. Verify all other properties preserved
6. Verify sprites unchanged

### Scenario 4: Hierarchy Path Matching
1. Create test scenes with nested UI structure: Canvas/Panel/SubmitButton
2. Configure target: Hierarchy path "Canvas/Panel/SubmitButton" position (200, 100)
3. Execute batch operation
4. Verify only buttons at exact path moved
5. Verify other buttons with same name but different paths unchanged

### Scenario 5: Preview Functionality
1. Configure multiple targets with some matching and some not
2. Click Preview Matches
3. Verify console shows correct matches without modifying scenes
4. Verify scenes remain unmodified and clean

### Scenario 6: Safety Features
1. Make unsaved changes to a scene
2. Attempt batch operation
3. Verify save prompt appears
4. Test cancellation of save prompt
5. Test proceeding with save
6. Verify operation respects user choice

### Scenario 7: Preserve Options
1. Test Preserve Z: 
   - Set buttons to different Z values
   - Disable Preserve Z, set position
   - Verify all Z set to 0
   - Enable Preserve Z, set position
   - Verify Z values unchanged
2. Test Preserve Size:
   - Set buttons to different sizes
   - Disable Preserve Size, set new size
   - Verify all sizes changed to new value
   - Enable Preserve Size, set new size
   - Verify sizes unchanged

### Scenario 8: Error Conditions
1. No targets configured - should show warning
2. Empty target name/path - should show warning
3. Non-existent scene folder - should handle gracefully
4. Scene missing RectTransform - should show warning but continue
5. Cancel during operation - should stop safely

## Success Criteria
- All UI objects move/resize to exact specified values
- Scene-specific sprites, properties, and components remain completely unchanged
- No prefabs created or used
- Undo support functional
- Scene saving/restoration works correctly
- Preview shows accurate counts without modification
- Operation summary reports correct statistics
- Error conditions handled gracefully with user feedback

## Performance Notes
- Test with 10+ scenes to verify reasonable performance
- Verify progress bar updates correctly
- Verify cancellation responsiveness