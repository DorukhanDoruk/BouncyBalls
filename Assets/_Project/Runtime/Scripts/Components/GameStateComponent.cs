using Runtime.Components.Model;
using Unity.Entities;
namespace Runtime.Components
{
    public struct GameStateComponent : IComponentData
    {
        public GameState GameState;
    }
}
