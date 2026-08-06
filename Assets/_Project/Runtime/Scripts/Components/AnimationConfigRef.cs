using _Project.Runtime.Scripts.Configs;
using Unity.Entities;
namespace Runtime.Core
{
    public struct AnimationConfigRef : IComponentData
    {
        public BlobAssetReference<AnimationConfigBlob> ConfigBlob;
    }
}
