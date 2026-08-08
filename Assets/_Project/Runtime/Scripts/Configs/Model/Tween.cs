namespace Runtime.Configs.Model
{
    public struct Tween
    {
        public float Duration;
        public EaseType EaseType;

        public float Evaluate(float t)
        {
            return Easing.Evaluate(EaseType, t);
        }
    }
}
