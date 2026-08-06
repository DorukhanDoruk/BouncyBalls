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

            if (!SystemAPI.TryGetSingletonBuffer<GridColumnRefElement>(out var columnRefs))
            {
                Debug.LogError($"[{nameof(LevelVerificationSystem)}] GridColumnRefElement buffer not found.");
                return;
            }

            Debug.Log($"[{nameof(LevelVerificationSystem)}] Grid column count = {columnRefs.Length}");

            for (int c = 0; c < columnRefs.Length; c++)
            {
                var ballQueue = EntityManager.GetBuffer<GridBallElement>(columnRefs[c].Value);

                sb.Clear();
                for (int b = 0; b < ballQueue.Length; b++)
                {
                    var ball = EntityManager.GetComponentData<BallComponent>(ballQueue[b].Value);
                    sb.Append($"{ball.Color}({ball.Remaining})");
                    if (b < ballQueue.Length - 1) sb.Append(", ");
                }

                Debug.Log($"[{nameof(LevelVerificationSystem)}] Column {c} front->back [{ballQueue.Length}] = {sb}");
            }
        }

        protected override void OnUpdate() { }
    }
}
