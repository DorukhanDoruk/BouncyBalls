using Runtime.Config;
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
        public GameObject DockPrefab;

        [Header("Disc Stack Shift")]
        public float DiscShiftDuration = 0.25f;
        public DG.Tweening.Ease DiscShiftEase = DG.Tweening.Ease.OutCubic;

        [Header("Stick Dip")]
        public float StickDipAmount = 0.15f;
        public float StickDipDuration = 0.25f;

        [Header("Disk Shatter")]
        public GameObject DiscPiecePrefab;
        public Mesh[] DiscPieceMeshes;

        public DiscShatterSettings DiscShatter = new DiscShatterSettings
        {
            Gravity = 18f,
            OutwardSpeed = 1f,
            UpwardSpeed = 3f,
            SpinSpeed = 540f,
            EndScale = 0.5f,
            ShrinkDuration = 0.5f,
            DissolveDelay = 2f,
            DissolveDuration = 0.25f
        };

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
