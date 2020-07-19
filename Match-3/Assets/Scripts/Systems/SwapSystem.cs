using System;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(SelectSystem))]
    public class SwapSystem : ComponentSystem
    {
        private Board _board;
        public void Init(Board board)
        {
            this._board = board;
        }
        
        protected override void OnUpdate()
        {
            EntityQueryBuilder queryBuilder = Entities.WithAll<TileComponent, SelectedComponent>();
            EntityQuery query = queryBuilder.ToEntityQuery();
            if (query.CalculateEntityCount() == 2)
            {
                NativeArray<Entity> tiles = query.ToEntityArray(Allocator.TempJob);

                Entity gem1 = (Entity) EntityManager.GetComponentData<TileComponent>(tiles[0]).gem;
                int2 position1 = EntityManager.GetComponentData<PositionComponent>(gem1).position;
                
                Entity gem2 = (Entity) EntityManager.GetComponentData<TileComponent>(tiles[1]).gem;
                int2 position2 = EntityManager.GetComponentData<PositionComponent>(gem2).position;

                EntityManager.AddComponent<CheckComponent>(gem1);
                EntityManager.AddComponent<SwappingComponent>(gem1);
                EntityManager.SetComponentData(gem1, new SwappingComponent {originPosition = position1, swapPair = gem2});
                EntityManager.AddComponent<TargetPositionComponent>(gem1);
                EntityManager.SetComponentData(gem1, new TargetPositionComponent {position = position2});
                
                EntityManager.AddComponent<CheckComponent>(gem2);
                EntityManager.AddComponent<SwappingComponent>(gem2);
                EntityManager.SetComponentData(gem2, new SwappingComponent {originPosition = position2, swapPair = gem1});
                EntityManager.AddComponent<TargetPositionComponent>(gem2);
                EntityManager.SetComponentData(gem2, new TargetPositionComponent {position = position1});

                EntityManager.SetComponentData(tiles[0], new TileComponent {gem = gem2});
                EntityManager.SetComponentData(tiles[1], new TileComponent {gem = gem1});
                
                EntityManager.RemoveComponent<SelectedComponent>(tiles[0]);
                EntityManager.RemoveComponent<SelectedComponent>(tiles[1]);
                
                EntityManager.RemoveComponent<IsSelectingComponent>(this._board.Player);

                this._board.HideSelect();

                tiles.Dispose();
            }
        }
    }
}