using System;
using System.Collections.Generic;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(MovementSystem))]
    public class SelectSystem : ComponentSystem
    {
        private Board _board;
        private Camera _camera;
        public void Init(Board board, Camera camera)
        {
            this._board = board;
            this._camera = camera;
        }

        protected override void OnUpdate()
        {
            Entity? selectedTile = null;
            Entities.WithAllReadOnly<TileComponent>().ForEach(
                (Entity entity) =>
                {
                    if (EntityManager.HasComponent<SelectedComponent>(entity))
                    {
                        selectedTile = entity;
                    }
                });
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = this._camera.ScreenToWorldPoint(Input.mousePosition);
                int2 position = new int2((int) Mathf.Ceil(worldPos.x - 0.5f), (int) Mathf.Ceil(worldPos.y - 0.5f));

                if (selectedTile != null && !this._board.IsPositionAdjacent(position, EntityManager.GetComponentData<PositionComponent>((Entity) selectedTile).position))
                {
                    return;
                }
                
                Entity tile = this._board.GetTileAtPosition(position);
                Entity? gem = EntityManager.GetComponentData<TileComponent>(tile).gem;
                
                if (gem != null && !EntityManager.HasComponent<TargetPositionComponent>((Entity) gem))
                {
                    EntityManager.AddComponent<SelectedComponent>(tile);
                    this._board.SetSelectPosition(EntityManager.GetComponentData<GemComponent>((Entity) gem)
                        .instancePool);
                }
            }
        }
    }
}