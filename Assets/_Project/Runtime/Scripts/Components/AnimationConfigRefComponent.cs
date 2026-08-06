using Runtime.Configs.Model;
using Unity.Entities;
namespace Runtime.Components
{
    public struct AnimationConfigRefComponent : IComponentData
    {
        public BlobAssetReference<AnimationConfigBlob> ConfigBlob;
    }
}
