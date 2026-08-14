using System;
using Runtime.Level.Model;
using UnityEngine;
namespace Runtime.Config
{
    [Serializable]
    public struct PaletteEntry
    {
        public ObjectColor Id;
        public Color Tint;
    }
}
