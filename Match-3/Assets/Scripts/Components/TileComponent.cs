using Unity.Entities;

namespace Components
{
    public struct TileComponent : IComponentData
    {
        public Entity? gem;
    }
}