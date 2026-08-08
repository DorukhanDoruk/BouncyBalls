using Runtime.Components;
using Runtime.Configs;
using Runtime.Configs.Model;
using Unity.Entities;
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

                var a = authoring.Animation;
                AddComponent(entity, new AnimationConfigComponent
                {
                    HopStretch = ToTween(a.HopStretch), 
                    LandSquash = ToTween(a.LandSquash),
                    BallVanish = ToTween(a.BallVanish),
                    GridColumnAdvance = ToTween(a.GridColumnAdvance), 
                    DockSlotSettle = ToTween(a.DockSlotSettle),
                    DiscStackShift = ToTween(a.DiscStackShift), 
                    StickDip = ToTween(a.StickDip),
                    StickDipAmount = a.StickDipAmount, 
                    DiscPieceGravity = a.DiscPieceGravity,
                    DiscPieceOutwardSpeed = a.DiscPieceOutwardSpeed,
                    DiscPieceUpwardSpeed = a.DiscPieceUpwardSpeed,
                    DiscPieceSpinSpeed = a.DiscPieceSpinSpeed,
                    DiscPieceEndScale = a.DiscPieceEndScale,
                    DiscPieceScale = ToTween(a.DiscPieceScale),
                    DiscPieceDissolveDelay = a.DiscPieceDissolveDelay,
                    DiscPieceDissolve = ToTween(a.DiscPieceDissolve),
                });

                var b = authoring.Ball;
                AddComponent(entity, new BallConfigComponent
                {
                    HopSpeed = b.HopSpeed,
                    LaunchSpeedMultiplier = b.LaunchSpeedMultiplier,
                    DockReturnSpeedMultiplier = b.DockReturnSpeedMultiplier,
                    ArchHeightPerUnit = b.ArchHeightPerUnit,
                    MaxArchHeight = b.MaxArchHeight, 
                    InPlaceBounceHeight = b.InPlaceBounceHeight,
                    InPlaceBounceTime = b.InPlaceBounceTime,
                    MinLaunchInterval = b.MinLaunchInterval,
                    MaxStretch = b.MaxStretch, 
                    MinSquash = b.MinSquash,
                    MaxActiveBalls = b.MaxActiveBalls,
                    MaxDockBalls = b.MaxDockBalls,
                    LoopModeSpeedMultiplier = b.LoopModeSpeedMultiplier,
                });
            }

            static Tween ToTween(in TweenDef src)
            {
                return new Tween { Duration = src.Duration, EaseType = src.EaseType };
            }
        }
    }
}
