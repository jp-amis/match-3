using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    public struct CheckComponent : IComponentData
    {
        public int2 originPosition;
    }
}