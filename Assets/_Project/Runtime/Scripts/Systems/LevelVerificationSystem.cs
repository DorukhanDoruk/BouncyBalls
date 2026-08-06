using Runtime.Components;
using Runtime.Components.Model;
using System.Text;
using Unity.Entities;
using UnityEngine;

namespace Runtime.Systems
{
    public partial class LevelVerificationSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<BallConfigComponent>();
        }

        protected override void OnStartRunning()
        {
            var stickQuery = SystemAPI.QueryBuilder().WithAll<Stick>().Build();
            Debug.Log($"[{nameof(LevelVerificationSystem)}] Stick count = {stickQuery.CalculateEntityCount()}");

            var sb = new StringBuilder();

            foreach (var (stick, discs) in SystemAPI.Query<RefRO<Stick>, DynamicBuffer<DiscElement>>())
            {
                sb.Clear();
                for (int i = 0; i < discs.Length; i++)
                {
                    sb.Append(discs[i].Color);
                    if (i < discs.Length - 1) sb.Append(", ");
                }

                var s = stick.ValueRO;
                Debug.Log($"[{nameof(LevelVerificationSystem)}] Stick {s.Index} pos={s.Position} discs[{discs.Length}] = {sb}");
            }

            if (!SystemAPI.TryGetSingletonBuffer<PathElement>(out var path))
            {
                Debug.LogError($"[{nameof(LevelVerificationSystem)}] PathElement buffer not found.");
                return;
            }

            sb.Clear();
            for (int i = 0; i < path.Length; i++)
            {
                sb.Append(path[i].StickIndex);
                if (i < path.Length - 1) sb.Append(" -> ");
            }

            Debug.Log($"[{nameof(LevelVerificationSystem)}] Path[{path.Length}] = {sb}");
        }

        protected override void OnUpdate() { }
    }
}
