using Runtime.Core;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace _Project.Runtime.Scripts.Configs
{
    public enum EaseType : byte
    {
        Linear,
        InQuad,
        OutQuad,
    }

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

    [Serializable]
    public struct TweenDef
    {
        // Only for fixed time tween.
        public float Duration;
        public EaseType EaseType;
        public AnimationCurve Curve;
        public int SampleCount;

        public static TweenDef Default = new TweenDef
        {
            Duration = 0.25f, EaseType = EaseType.Linear,
            Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f), SampleCount = 32,
        };
    }

    [CreateAssetMenu(menuName = "BouncyBalls/Animation Config", fileName = "AnimationConfig")]
    public class AnimationConfigSO : ScriptableObject
    {
        [Header("Jump")]
        public TweenDef JumpArc = new TweenDef
        {
            Curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)), SampleCount = 48,
        };

        [Header("Squash & Stratch")]
        public TweenDef JumpStratch = TweenDef.Default;
        public TweenDef LandSquash = TweenDef.Default;
    }

    [CreateAssetMenu(menuName = "TaleMonster/Ball Config", fileName = "BallConfig")]
    public class BallConfigSO : ScriptableObject
    {
        public float JumpSpeed = 10f;
        public float ArchHeightPerUnit = 1f;
        public float MaxArchHeight = 3f;

        public float InPlaceBounceHeight = 1f;
        public float InPlaceBounceTime = 0.3f;

        public float MinLaunchInterval = 0.15f;
        public float MaxStretch = 1.35f;
        public float MinSquash = 0.7f;

        public byte MaxActiveBalls = 5;
        public byte MaxDockBalls = 5;
    }

    // We cannot use tweenDef at burst + job.
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
            int i = (int)f + 1;
            int j = math.min(i, Samples.Length - 1);
            return math.lerp(Samples[i], Samples[j], f - i);
        }
    }

    public struct AnimationConfigBlob
    {
        public TweenBlob JumpArc;
        public TweenBlob JumpStratch;
        public TweenBlob LandSquash;
    }
    
    public struct BallConfig : IComponentData
    {
        public float JumpSpeed;
        public float ArchHeightPerUnit;
        public float MaxArchHeight;

        public float InPlaceBounceHeight;
        public float InPlaceBounceTime;

        public float MinLaunchInterval;
        public float MaxStretch;
        public float MinSquash;

        public byte MaxActiveBalls;
        public byte MaxDockBalls;
    }

    public class GameConfigAuthoring : MonoBehaviour
    {
        public AnimationConfigSO Animation;
        public BallConfigSO Ball;

        private class GameConfigBaker : Baker<GameConfigAuthoring>
        {
            public override void Bake(GameConfigAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                DependsOn(authoring.Animation);
                DependsOn(authoring.Ball);

                var builder = new BlobBuilder(Allocator.Temp);
                ref var root = ref builder.ConstructRoot<AnimationConfigBlob>();

                BuildTween(ref builder, ref root.JumpArc, authoring.Animation.JumpArc);
                BuildTween(ref builder, ref root.JumpStratch, authoring.Animation.JumpStratch);
                BuildTween(ref builder, ref root.LandSquash, authoring.Animation.LandSquash);

                var blob = builder.CreateBlobAssetReference<AnimationConfigBlob>(Allocator.Persistent);
                AddBlobAsset(ref blob, out _);
                AddComponent(entity, new AnimationConfigRef { ConfigBlob = blob });

                var b = authoring.Ball;
                AddComponent(entity, new BallConfig
                {
                    JumpSpeed            = b.JumpSpeed,
                    ArchHeightPerUnit    = b.ArchHeightPerUnit,
                    MaxArchHeight        = b.MaxArchHeight,
                    InPlaceBounceHeight = b.InPlaceBounceHeight,
                    InPlaceBounceTime   = b.InPlaceBounceTime,
                    MinLaunchInterval   = b.MinLaunchInterval,
                    MaxStretch          = b.MaxStretch,
                    MinSquash           = b.MinSquash,
                    MaxActiveBalls      = b.MaxActiveBalls,
                    MaxDockBalls        = b.MaxDockBalls
                });
            }

            static void BuildTween(ref BlobBuilder builder, ref TweenBlob dst, in TweenDef src)
            {
                bool useCurve = src.Curve != null && src.Curve.length > 1;

                dst.Duration = src.Duration;
                dst.EaseType = src.EaseType;

                int n = useCurve ? math.clamp(src.SampleCount, 2, 128) : 1;
                var samples = builder.Allocate(ref dst.Samples, n);

                if (!useCurve)
                {
                    samples[0] = 0f;
                    return;
                }

                for (int i = 0; i < n; i++)
                {
                    samples[i] = src.Curve.Evaluate((float)i / (n - 1));
                }
            }
        }
    }
}
