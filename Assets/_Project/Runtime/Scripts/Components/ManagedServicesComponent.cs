using Runtime.Core;
using Unity.Entities;
namespace Runtime.Components
{
    public class ManagedServicesComponent : IComponentData
    {
        public ServiceContainer Container;
    }
}
