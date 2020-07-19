using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct WorldScaleComponent : IComponentData
    {
        public float2 scale;
    }
}