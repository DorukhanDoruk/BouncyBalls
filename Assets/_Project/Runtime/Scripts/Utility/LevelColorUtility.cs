using Runtime.Configs.Model;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
namespace Runtime.Utility
{
    public static class LevelColorUtility
    {
        private static readonly List<Color> _colorByDiscColorValue = new List<Color>()
        {
            Color.red, 
            Color.blue,
            Color.green,
            Color.yellow,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color GetColorOfDiskByDiscColorType_Unsafe(DiscColorType discColorType)
        {
            return _colorByDiscColorValue[(int)discColorType];
        }
    }
}
