using Runtime.Configs.Model;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
namespace Runtime.Configs
{
    public static class Easing
    {
        private const float _backOvershoot = 1.70158f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Evaluate(EaseType type, float t)
        {
            t = math.saturate(t);

            switch (type)
            {
                case EaseType.InQuad:
                    return t * t;
                case EaseType.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                case EaseType.InOutQuad:
                    return t < 0.5f
                        ? 2f * t * t
                        : 1f - math.pow(-2f * t + 2f, 2f) * 0.5f;

                case EaseType.InCubic:
                    return t * t * t;
                case EaseType.OutCubic:
                    return 1f - math.pow(1f - t, 3f);
                case EaseType.InOutCubic:
                    return t < 0.5f
                        ? 4f * t * t * t
                        : 1f - math.pow(-2f * t + 2f, 3f) * 0.5f;

                case EaseType.InBack:
                    return (_backOvershoot + 1f) * t * t * t - _backOvershoot * t * t;
                case EaseType.OutBack:
                    return 1f + (_backOvershoot + 1f) * math.pow(t - 1f, 3f) + _backOvershoot * math.pow(t - 1f, 2f);
                case EaseType.InOutBack:
                    return InOutBack(t);

                default:
                    return t;
            }
        }

        private static float InOutBack(float t)
        {
            const float overshoot = _backOvershoot * 1.525f;

            return t < 0.5f
                ? math.pow(2f * t, 2f) * ((overshoot + 1f) * 2f * t - overshoot) * 0.5f
                : (math.pow(2f * t - 2f, 2f) * ((overshoot + 1f) * (2f * t - 2f) + overshoot) + 2f) * 0.5f;
        }
    }
}
