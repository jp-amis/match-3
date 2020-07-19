using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct SwappingComponent : IComponentData
    {
        public int2 originPosition;
        public Entity swapPair;
    }
}