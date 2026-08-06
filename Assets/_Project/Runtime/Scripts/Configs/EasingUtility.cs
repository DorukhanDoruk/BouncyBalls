using Runtime.Configs.Model;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Runtime.Configs
{
    public static class Easing
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Evaulate(EaseType type, float t)
        {
            t = math.saturate(t);
            switch (type)
            {
                case EaseType.Linear:
                    return t;
                case EaseType.InQuad:
                    return t * t;
                case EaseType.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                default:
                    return t;
            }
        }
    }
}
