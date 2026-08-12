using Runtime.Level.Model;
using System;
using UnityEngine;
namespace Runtime.Core
{
    [CreateAssetMenu(menuName = "BouncyBalls/Visual Config", fileName = "VisualConfig")]
    public class VisualConfigSO : ScriptableObject
    {
        [Header("Palet")]
        public PaletteEntry[] Palette;

        [Header("Prefabs")]
        public GameObject BallPrefab;
        public GameObject StickPrefab;
        public GameObject DiscPrefab;

        [Header("Squash & Stretch")]
        public float SquashDuration = 0.12f;
        public Vector3 SquashScale = new Vector3(1.25f, 0.75f, 1.25f);
        public DG.Tweening.Ease SquashEase = DG.Tweening.Ease.OutQuad;

        [Header("Disk Shatter")]
        public float DiscPopDuration = 0.2f;
        public DG.Tweening.Ease DiscPopEase = DG.Tweening.Ease.InBack;

        [Header("Grid Shift")]
        public float GridShiftDuration = 0.25f;
        public DG.Tweening.Ease GridShiftEase = DG.Tweening.Ease.OutCubic;

        [Header("Dock Insert")]
        public float DockInsertDuration = 0.3f;
        public DG.Tweening.Ease DockInsertEase = DG.Tweening.Ease.OutBack;

        public Color GetColor(ObjectColor color)
        {
            for (int i = 0; i < Palette.Length; i++)
            {
                if (Palette[i].Id == color)
                {
                    return Palette[i].Tint;
                }
            }

            throw new ArgumentException($"{color} is missing from the VisualConfig palette.");
        }
    }
}
