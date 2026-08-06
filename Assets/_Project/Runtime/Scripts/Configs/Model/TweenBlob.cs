using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Configs.Model
{
    public struct TweenBlob
    {
        public float Duration;
        public EaseType EaseType;
        public BlobArray<float> Samples;

        public float Evaulate(float t)
        {
            t = math.saturate(t);
            if (Samples.Length < 2)
            {
                return Easing.Evaulate(EaseType, t);
            }

            float f = t * (Samples.Length - 1);
            int i = (int)f;
            int j = math.min(i + 1, Samples.Length - 1);
            return math.lerp(Samples[i], Samples[j], f - i);
        }
    }
}
