using System.Collections.Generic;
using Runtime.Config;
using Runtime.Core;
using Runtime.Level.Config;
using Runtime.Level.Model;
using UnityEditor;
using UnityEngine;

namespace BouncyBalls.Editor
{
    public sealed class LevelEditorWindow : EditorWindow
    {
        private const string LevelFolder = "Assets/_Project/Runtime/Resources/Levels";
        private const float ColorFieldWidth = 90f;

        private LevelData _level;
        private SerializedObject _serialized;
        private Vector2 _scroll;
        private bool _snap = true;

        private VisualConfigSO _visualConfig;
        private LevelLayoutSO _levelLayout;
        private GameConfigSO _gameConfig;

        [MenuItem("BouncyBalls/Level Editor")]
        private static void Open()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnEnable()
        {
            _visualConfig = FindAsset<VisualConfigSO>();
            _levelLayout = FindAsset<LevelLayoutSO>();
            _gameConfig = FindAsset<GameConfigSO>();

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnGUI()
        {
            DrawToolbar();

            if (_level == null)
            {
                EditorGUILayout.HelpBox("Pick a level asset to edit.", MessageType.Info);
                return;
            }

            // Also covers the domain reload, which drops the SerializedObject but keeps the level.
            if (_serialized == null || _serialized.targetObject != _level)
            {
                _serialized = new SerializedObject(_level);
            }

            _serialized.Update();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSticks();
            DrawPath();
            DrawGrid();
            EditorGUILayout.EndScrollView();

            _serialized.ApplyModifiedProperties();

            if (Camera.main == null)
            {
                EditorGUILayout.HelpBox("No MainCamera in the open scene, so the stick area gizmo is hidden.", MessageType.Warning);
            }

            DrawValidation();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_level == null || _levelLayout == null || _gameConfig == null)
            {
                return;
            }

            LevelSceneDrawer.Draw(_level, _levelLayout, _gameConfig, Tint, _snap);
            Repaint();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUILayout.LabelField("Level", EditorStyles.miniLabel, GUILayout.Width(36));
                _level = (LevelData)EditorGUILayout.ObjectField(_level, typeof(LevelData), false);

                _snap = GUILayout.Toggle(_snap, "Snap", EditorStyles.toolbarButton, GUILayout.Width(44));

                if (GUILayout.Button("New", EditorStyles.toolbarButton, GUILayout.Width(44)))
                {
                    CreateLevel();
                }
            }
        }

        private void DrawSticks()
        {
            var sticks = _serialized.FindProperty(nameof(LevelData.Sticks));
            DrawSectionHeader("Sticks", sticks, false);

            int removeAt = -1;

            for (int i = 0; i < sticks.arraySize; i++)
            {
                var stick = sticks.GetArrayElementAtIndex(i);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"#{i}", EditorStyles.miniBoldLabel, GUILayout.Width(24));
                        EditorGUILayout.PropertyField(stick.FindPropertyRelative(nameof(StickData.Position)), GUIContent.none);
                        EditorGUILayout.LabelField("Shown", EditorStyles.miniLabel, GUILayout.Width(40));
                        EditorGUILayout.PropertyField(stick.FindPropertyRelative(nameof(StickData.ShownDiscCount)), GUIContent.none, GUILayout.Width(34));

                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            removeAt = i;
                        }
                    }

                    DrawDiscs(stick.FindPropertyRelative(nameof(StickData.DiscColors)));
                }
            }

            // Deferred, so the control count stays stable for the rest of this pass.
            if (removeAt >= 0)
            {
                sticks.DeleteArrayElementAtIndex(removeAt);
                RemoveFromPath(removeAt);
            }
        }

        private void RemoveFromPath(int stickIndex)
        {
            var path = _serialized.FindProperty(nameof(LevelData.Path));

            for (int i = path.arraySize - 1; i >= 0; i--)
            {
                var element = path.GetArrayElementAtIndex(i);

                if (element.intValue == stickIndex)
                {
                    path.DeleteArrayElementAtIndex(i);
                    continue;
                }

                if (element.intValue > stickIndex)
                {
                    element.intValue--;
                }
            }
        }

        private void DrawDiscs(SerializedProperty colors)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("bottom to top", EditorStyles.miniLabel, GUILayout.Width(80));

                for (int i = 0; i < colors.arraySize; i++)
                {
                    DrawColorField(colors.GetArrayElementAtIndex(i));
                }

                DrawSizeButtons(colors);
                GUILayout.FlexibleSpace();
            }
        }

        private void DrawPath()
        {
            var path = _serialized.FindProperty(nameof(LevelData.Path));
            DrawSectionHeader("Path", path, true);

            int stickCount = _serialized.FindProperty(nameof(LevelData.Sticks)).arraySize;
            var names = new string[stickCount];
            for (int i = 0; i < stickCount; i++)
            {
                names[i] = $"#{i}";
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                for (int i = 0; i < path.arraySize; i++)
                {
                    var element = path.GetArrayElementAtIndex(i);
                    element.intValue = EditorGUILayout.Popup(element.intValue, names, GUILayout.Width(44));
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawGrid()
        {
            var columns = _serialized.FindProperty(nameof(LevelData.GridColumns));
            DrawSectionHeader("Grid", columns, false);

            int removeAt = -1;

            for (int i = 0; i < columns.arraySize; i++)
            {
                var balls = columns.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(GridColumnData.GridBalls));

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"Column {i}", EditorStyles.miniBoldLabel);
                        DrawSizeButtons(balls);

                        if (GUILayout.Button("x", GUILayout.Width(20)))
                        {
                            removeAt = i;
                        }
                    }

                    for (int j = 0; j < balls.arraySize; j++)
                    {
                        var ball = balls.GetArrayElementAtIndex(j);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            DrawColorField(ball.FindPropertyRelative(nameof(GridBallData.ColorId)));
                            EditorGUILayout.PropertyField(ball.FindPropertyRelative(nameof(GridBallData.Counter)), GUIContent.none, GUILayout.Width(44));
                            GUILayout.FlexibleSpace();
                        }
                    }
                }
            }

            if (removeAt >= 0)
            {
                columns.DeleteArrayElementAtIndex(removeAt);
            }
        }

        private void DrawValidation()
        {
            var issues = Validate();

            if (issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Level looks valid.", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(string.Join("\n", issues), MessageType.Warning);
        }

        private List<string> Validate()
        {
            var issues = new List<string>();

            if (_level.Path.Count == 0)
            {
                issues.Add("Path is empty, balls have nowhere to go.");
            }

            for (int i = 0; i < _level.Path.Count; i++)
            {
                if (_level.Path[i] < 0 || _level.Path[i] >= _level.Sticks.Count)
                {
                    issues.Add($"Path step {i} points at stick #{_level.Path[i]}, which does not exist.");
                }
            }

            for (int i = 0; i < _level.Sticks.Count; i++)
            {
                var stick = _level.Sticks[i];

                if (stick.DiscColors.Count == 0)
                {
                    issues.Add($"Stick #{i} has no discs.");
                }

                if (stick.ShownDiscCount <= 0 || stick.ShownDiscCount > stick.DiscColors.Count)
                {
                    issues.Add($"Stick #{i} shows {stick.ShownDiscCount} of its {stick.DiscColors.Count} discs.");
                }
            }

            for (int i = 0; i < _level.GridColumns.Count; i++)
            {
                var balls = _level.GridColumns[i].GridBalls;

                if (balls.Count == 0)
                {
                    issues.Add($"Column {i} has no balls.");
                    continue;
                }

                for (int j = 0; j < balls.Count; j++)
                {
                    if (balls[j].Counter <= 0)
                    {
                        issues.Add($"Column {i} ball {j} has a counter of {balls[j].Counter}.");
                    }
                }
            }

            AppendColorBudget(issues);

            return issues;
        }

        // A ball only breaks its own colour, so the budget has to balance per colour, not in total.
        private void AppendColorBudget(List<string> issues)
        {
            var needed = new Dictionary<ObjectColor, int>();
            var carried = new Dictionary<ObjectColor, int>();

            for (int i = 0; i < _level.Sticks.Count; i++)
            {
                if (!_level.Path.Contains(i))
                {
                    continue;
                }

                var discs = _level.Sticks[i].DiscColors;
                for (int d = 0; d < discs.Count; d++)
                {
                    needed.TryGetValue(discs[d], out int count);
                    needed[discs[d]] = count + 1;
                }
            }

            for (int i = 0; i < _level.GridColumns.Count; i++)
            {
                var balls = _level.GridColumns[i].GridBalls;
                for (int j = 0; j < balls.Count; j++)
                {
                    carried.TryGetValue(balls[j].ColorId, out int count);
                    carried[balls[j].ColorId] = count + Mathf.Max(0, balls[j].Counter);
                }
            }

            foreach (var pair in needed)
            {
                carried.TryGetValue(pair.Key, out int hits);

                if (hits < pair.Value)
                {
                    issues.Add($"{pair.Key}: path holds {pair.Value} discs but balls carry only {hits} hits.");
                }
            }

            foreach (var pair in carried)
            {
                if (!needed.ContainsKey(pair.Key))
                {
                    issues.Add($"{pair.Key}: {pair.Value} hits carried but the path has no {pair.Key} disc.");
                }
            }
        }

        private void CreateLevel()
        {
            string path = EditorUtility.SaveFilePanelInProject("New Level", "Level_00", "asset", string.Empty, LevelFolder);

            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var level = CreateInstance<LevelData>();
            AssetDatabase.CreateAsset(level, path);
            AssetDatabase.SaveAssets();

            _level = level;
        }

        private void DrawColorField(SerializedProperty color)
        {
            var rect = GUILayoutUtility.GetRect(ColorFieldWidth, EditorGUIUtility.singleLineHeight, GUILayout.Width(ColorFieldWidth));
            var swatch = new Rect(rect.x, rect.y + 1f, 14f, rect.height - 2f);
            var popup = new Rect(rect.x + 18f, rect.y, rect.width - 18f, rect.height);

            EditorGUI.DrawRect(swatch, Tint((ObjectColor)color.enumValueIndex));
            color.enumValueIndex = (int)(ObjectColor)EditorGUI.EnumPopup(popup, (ObjectColor)color.enumValueIndex);
        }

        private static void DrawSectionHeader(string title, SerializedProperty list, bool shrinkable)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

                if (shrinkable)
                {
                    DrawSizeButtons(list);
                    return;
                }

                if (GUILayout.Button("+", GUILayout.Width(24)))
                {
                    list.arraySize++;
                }
            }
        }

        private static void DrawSizeButtons(SerializedProperty list)
        {
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                list.arraySize++;
            }

            if (list.arraySize > 0 && GUILayout.Button("-", GUILayout.Width(24)))
            {
                list.arraySize--;
            }
        }

        // VisualConfigSO.GetColor throws on a missing entry, which would fire every repaint.
        private Color Tint(ObjectColor color)
        {
            if (_visualConfig == null)
            {
                return Color.grey;
            }

            for (int i = 0; i < _visualConfig.Palette.Length; i++)
            {
                if (_visualConfig.Palette[i].Id == color)
                {
                    return _visualConfig.Palette[i].Tint;
                }
            }

            return Color.grey;
        }

        private static T FindAsset<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Length == 0 ? null : AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
