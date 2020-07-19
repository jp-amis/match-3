using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(DragSystem))]
    public class ZOrderSystem : ComponentSystem
    {
        private Board _board;
        public void Init(Board board)
        {
            this._board = board;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<GemComponent, OnTopComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent 
            ) =>
            {
                if (EntityManager.HasComponent<IsDraggingComponent>(this._board.Player))
                {
                    this._board.SetGemOnTop(gemComponent.instancePool);
                }
                else
                {
                    this._board.SetGemOnBottom(gemComponent.instancePool);
                    EntityManager.RemoveComponent<OnTopComponent>(entity);
                }
            });
        }
    }
}