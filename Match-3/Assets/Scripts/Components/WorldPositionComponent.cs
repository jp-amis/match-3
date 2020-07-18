using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct WorldPositionComponent : IComponentData
    {
        public float2 position;
    }
}