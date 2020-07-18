using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct TargetPositionComponent : IComponentData
    {
        public int2 position;
    }
}