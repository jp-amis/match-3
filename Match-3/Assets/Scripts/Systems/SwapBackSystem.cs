using System;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(CheckSystem))]
    public class SwapBackSystem : ComponentSystem
    {
        private Board _board;

        public void Init(Board board)
        {
            this._board = board;
        }
        
        protected override void OnUpdate()
        {
            Entities.WithAll<GemComponent, SwappingComponent>().WithNone<DestroyComponent, CheckComponent, TargetPositionComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref SwappingComponent swappingComponent
            ) =>
            {
                if (EntityManager.HasComponent<DestroyComponent>(swappingComponent.swapPair))
                {
                    EntityManager.RemoveComponent<SwappingComponent>(entity);
                    return;
                }
                
                EntityManager.AddComponent<TargetPositionComponent>(entity);
                EntityManager.SetComponentData(entity, new TargetPositionComponent {position = swappingComponent.originPosition});

                EntityManager.RemoveComponent<SwappingComponent>(entity);

                Entity tile = this._board.GetTileAtPosition(swappingComponent.originPosition);
                EntityManager.SetComponentData(tile, new TileComponent {gem = entity});
            });
        }
    }
}