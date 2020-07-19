using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Components
{
    public struct IsDraggingComponent : IComponentData
    {
        public Entity gem;
        public Vector3 pointerOrigin;
        public float2 pointerDiff;
        public bool verified;
    }
}