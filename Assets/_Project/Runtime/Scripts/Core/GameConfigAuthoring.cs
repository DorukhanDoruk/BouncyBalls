using Runtime.Components;
using Runtime.Configs;
using Runtime.Configs.Model;
using System;
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
                    HopStretch = ToTween(a.HopStretch, nameof(a.HopStretch)),
                    LandSquash = ToTween(a.LandSquash, nameof(a.LandSquash)),
                    BallVanish = ToTween(a.BallVanish, nameof(a.BallVanish)),
                    GridColumnAdvance = ToTween(a.GridColumnAdvance, nameof(a.GridColumnAdvance)),
                    DockSlotSettle = ToTween(a.DockSlotSettle, nameof(a.DockSlotSettle)),
                    DiscStackShift = ToTween(a.DiscStackShift, nameof(a.DiscStackShift)),
                    StickDip = ToTween(a.StickDip, nameof(a.StickDip)),
                    StickDipAmount = a.StickDipAmount,
                    DiscPieceGravity = a.DiscPieceGravity,
                    DiscPieceOutwardSpeed = a.DiscPieceOutwardSpeed,
                    DiscPieceUpwardSpeed = a.DiscPieceUpwardSpeed,
                    DiscPieceSpinSpeed = a.DiscPieceSpinSpeed,
                    DiscPieceEndScale = a.DiscPieceEndScale,
                    DiscPieceScale = ToTween(a.DiscPieceScale, nameof(a.DiscPieceScale)),
                    DiscPieceDissolveDelay = a.DiscPieceDissolveDelay,
                    DiscPieceDissolve = ToTween(a.DiscPieceDissolve, nameof(a.DiscPieceDissolve)),
                });

                var b = authoring.Ball;
                AddComponent(entity, new BallConfigComponent
                {
                    HopSpeed = Positive(b.HopSpeed, nameof(b.HopSpeed)),
                    LaunchSpeedMultiplier = Positive(b.LaunchSpeedMultiplier, nameof(b.LaunchSpeedMultiplier)),
                    DockReturnSpeedMultiplier = Positive(b.DockReturnSpeedMultiplier, nameof(b.DockReturnSpeedMultiplier)),
                    ArchHeightPerUnit = b.ArchHeightPerUnit,
                    MaxArchHeight = b.MaxArchHeight,
                    InPlaceBounceHeight = b.InPlaceBounceHeight,
                    InPlaceBounceTime = Positive(b.InPlaceBounceTime, nameof(b.InPlaceBounceTime)),
                    MinLaunchInterval = b.MinLaunchInterval,
                    MaxStretch = b.MaxStretch,
                    MinSquash = b.MinSquash,
                    MaxActiveBalls = b.MaxActiveBalls,
                    MaxDockBalls = b.MaxDockBalls,
                    LoopModeSpeedMultiplier = Positive(b.LoopModeSpeedMultiplier, nameof(b.LoopModeSpeedMultiplier)),
                });
            }

            private static Tween ToTween(in TweenDef source, string field)
            {
                return new Tween
                {
                    Duration = Positive(source.Duration, field),
                    EaseType = source.EaseType,
                };
            }

            // Each of these divides something at runtime. A zero would quietly turn positions
            // into NaN and strand the entity, so it is rejected here where the data comes in.
            private static float Positive(float value, string field)
            {
                if (value <= 0f)
                {
                    throw new InvalidOperationException($"[{nameof(GameConfigAuthoring)}] {field} must be greater than zero, was {value}.");
                }

                return value;
            }
        }
    }
}
