using Runtime.Configs.Model;
using System;
using UnityEngine;
namespace Runtime.Configs
{
    [Serializable]
    public class PaletteEntry
    {
        public DiscColorType Type;
        public Color BaseColor = Color.white;
        public Color ShadeColor = Color.gray;
        public Color SpecularColor = Color.white;
    }

    [CreateAssetMenu(menuName = "BouncyBalls/Color Palette", fileName = "ColorPalette")]
    public class ColorPaletteSO : ScriptableObject
    {
        public PaletteEntry[] Discs;
        public PaletteEntry[] Balls;

        public PaletteEntry GetDisc(DiscColorType color) => Find(Discs, color, nameof(Discs));
        public PaletteEntry GetBall(DiscColorType color) => Find(Balls, color, nameof(Balls));

        private static PaletteEntry Find(PaletteEntry[] entries, DiscColorType color, string listName)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].Type == color)
                {
                    return entries[i];
                }
            }

            throw new InvalidOperationException($"ColorPalette.{listName} has no entry for {color}.");
        }
    }
}
