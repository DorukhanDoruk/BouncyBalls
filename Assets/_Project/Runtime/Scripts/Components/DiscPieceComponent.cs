using Runtime.Configs.Model;
using Unity.Entities;
using Unity.Mathematics;
namespace Runtime.Components
{
    public struct DiscPieceComponent : IComponentData
    {
        public int MeshIndex;
        public DiscColorType Color;

        public float3 Position;
        public float3 Velocity;
        public float3 Rotation;
        public float3 AngularVelocity;

        public float GroundY;
        public float Elapsed;
    }
}
