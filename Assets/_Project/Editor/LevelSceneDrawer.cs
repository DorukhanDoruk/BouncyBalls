using System;
using Runtime.Config;
using Runtime.Core;
using Runtime.Level.Config;
using Runtime.Level.Model;
using Runtime.Simulation;
using UnityEditor;
using UnityEngine;

namespace BouncyBalls.Editor
{
    public static class LevelSceneDrawer
    {
        public static void Draw(LevelData level, LevelLayoutSO layoutConfig, GameConfigSO gameConfig,
            Func<ObjectColor, Color> tint, bool snap)
        {
            var layout = layoutConfig.ToRuntime();
            var config = gameConfig.ToRuntime();

            DrawSticks(level, layout, config, tint, snap);
            DrawPath(level, layout, config);
            DrawGrid(level, layout, tint);
            DrawDock(layout, config);
        }

        private static void DrawSticks(LevelData level, LevelLayout layout, GameConfig config,
            Func<ObjectColor, Color> tint, bool snap)
        {
            for (int i = 0; i < level.Sticks.Count; i++)
            {
                var stick = level.Sticks[i];
                var basePosition = stick.Position + layout.StickOrigin;

                EditorGUI.BeginChangeCheck();
                var moved = Handles.PositionHandle(basePosition, Quaternion.identity);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(level, "Move Stick");
                    stick.Position = Snap(moved - layout.StickOrigin, snap);
                    EditorUtility.SetDirty(level);
                }

                Handles.color = Color.white;
                Handles.Label(Surface(stick, layout, config) + Vector3.up * 0.6f, $"#{i}");

                DrawDiscs(stick, basePosition, config, tint);
            }
        }

        // Same layout as StickView: the stack hangs from the top, extras sink below the base.
        private static void DrawDiscs(StickData stick, Vector3 basePosition, GameConfig config, Func<ObjectColor, Color> tint)
        {
            float top = stick.ShownDiscCount * config.DiscHeight;

            for (int i = 0; i < stick.DiscColors.Count; i++)
            {
                float height = top - config.DiscHeight * (stick.DiscColors.Count - i - 0.5f);

                var color = tint(stick.DiscColors[i]);
                color.a = height < 0f ? 0.2f : 1f;

                Handles.color = color;
                Handles.DrawSolidDisc(basePosition + Vector3.up * height, Vector3.up, 0.5f);
            }
        }

        private static void DrawPath(LevelData level, LevelLayout layout, GameConfig config)
        {
            Handles.color = Color.white;

            for (int i = 0; i < level.Path.Count; i++)
            {
                if (!TryGetStick(level, level.Path[i], out var stick))
                {
                    continue;
                }

                var from = Surface(stick, layout, config);
                Handles.Label(from + Vector3.up * 0.25f, $"{i}");

                if (!TryGetStick(level, level.Path[(i + 1) % level.Path.Count], out var next))
                {
                    continue;
                }

                Handles.DrawAAPolyLine(3f, from, Surface(next, layout, config));
            }
        }

        private static void DrawGrid(LevelData level, LevelLayout layout, Func<ObjectColor, Color> tint)
        {
            int columnCount = level.GridColumns.Count;

            for (int column = 0; column < columnCount; column++)
            {
                var balls = level.GridColumns[column].GridBalls;

                for (int row = 0; row < balls.Count; row++)
                {
                    var slot = LevelLoader.GridBallSlot(column, columnCount, row, layout);

                    Handles.color = tint(balls[row].ColorId);
                    Handles.SphereHandleCap(0, slot, Quaternion.identity, 0.8f, EventType.Repaint);

                    Handles.color = Color.white;
                    Handles.Label(slot + Vector3.up * 0.5f, balls[row].Counter.ToString());
                }
            }
        }

        private static void DrawDock(LevelLayout layout, GameConfig config)
        {
            Handles.color = Color.white;

            for (int i = 0; i < config.DockCapacity; i++)
            {
                Handles.DrawWireDisc(LevelLoader.DockSlot(i, config.DockCapacity, layout), Vector3.up, 0.5f);
            }
        }

        private static bool TryGetStick(LevelData level, int index, out StickData stick)
        {
            bool valid = index >= 0 && index < level.Sticks.Count;
            stick = valid ? level.Sticks[index] : null;
            return valid;
        }

        private static Vector3 Surface(StickData stick, LevelLayout layout, GameConfig config)
        {
            return stick.Position + layout.StickOrigin + Vector3.up * (stick.ShownDiscCount * config.DiscHeight);
        }

        private static Vector3 Snap(Vector3 value, bool snap)
        {
            if (!snap)
            {
                return value;
            }

            return new Vector3(Mathf.Round(value.x * 2f) * 0.5f, Mathf.Round(value.y * 2f) * 0.5f, Mathf.Round(value.z * 2f) * 0.5f);
        }
    }
}
