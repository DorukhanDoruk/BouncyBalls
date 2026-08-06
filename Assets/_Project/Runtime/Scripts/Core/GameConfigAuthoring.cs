using Runtime.Components;
using Runtime.Configs;
using Runtime.Configs.Model;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Runtime.Core
{
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
                try
                {
                    ref var root = ref builder.ConstructRoot<AnimationConfigBlob>();

                    BuildTween(ref builder, ref root.JumpArc, authoring.Animation.JumpArc);
                    BuildTween(ref builder, ref root.JumpStratch, authoring.Animation.JumpStratch);
                    BuildTween(ref builder, ref root.LandSquash, authoring.Animation.LandSquash);

                    var blob = builder.CreateBlobAssetReference<AnimationConfigBlob>(Allocator.Persistent);
                    AddBlobAsset(ref blob, out _);
                    AddComponent(entity, new AnimationConfigRefComponent { ConfigBlob = blob });
                }
                finally
                {
                    builder.Dispose();
                }
                

                var b = authoring.Ball;
                AddComponent(entity, new BallConfigComponent
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
