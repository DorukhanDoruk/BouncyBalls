using Runtime.Components;
using Runtime.Components.Model;
using Unity.Entities;
using UnityEngine;
namespace Runtime.Systems
{
    [UpdateAfter(typeof(ArrivalResolveSystem))]
    public partial class GameStateSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<GameStateComponent>();
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnUpdate()
        {
            if (SystemAPI.GetSingleton<GameStateComponent>().GameState != GameState.Playing)
            {
                return;
            }

            var stickRefs = SystemAPI.GetSingletonBuffer<StickRefElement>();

            if (AllDiscsCleared(stickRefs))
            {
                Finish(GameState.Won);
                return;
            }

            var config = SystemAPI.GetSingleton<BallConfigComponent>();
            var dockBalls = SystemAPI.GetSingletonBuffer<DockBallElement>();

            if (dockBalls.Length > config.MaxDockBalls)
            {
                Finish(GameState.Lost);
            }
        }

        private bool AllDiscsCleared(in DynamicBuffer<StickRefElement> stickRefs)
        {
            for (int i = 0; i < stickRefs.Length; i++)
            {
                if (EntityManager.GetBuffer<DiscElement>(stickRefs[i].Entity).Length > 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void Finish(GameState result)
        {
            SystemAPI.SetSingleton(new GameStateComponent { GameState = result });

            var request = SystemAPI.GetSingleton<LaunchRequestComponent>();
            request.Ball = Entity.Null;
            request.Locked = true;
            SystemAPI.SetSingleton(request);
        }
    }
}
