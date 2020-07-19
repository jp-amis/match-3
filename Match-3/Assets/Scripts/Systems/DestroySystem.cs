using System;
using System.Collections.Generic;
using System.Linq;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(CheckSystem))]
    [UpdateAfter(typeof(FillSystem))]
    public class DestroySystem : ComponentSystem
    {
        private float scaleVelocity = 2.0f;
        private Board _board;

        public void Init(Board board)
        {
            this._board = board;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<GemComponent, DestroyComponent, WorldScaleComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref PositionComponent positionComponent,
                ref WorldScaleComponent worldScaleComponent
            ) =>
            {
                worldScaleComponent.scale.x -= Time.DeltaTime * this.scaleVelocity;
                worldScaleComponent.scale.y = worldScaleComponent.scale.x;

                this._board.SetGemScale(gemComponent.instancePool, worldScaleComponent.scale);
                if (worldScaleComponent.scale.x <= 0.5f)
                {
                    this._board.FreeGem(gemComponent.instancePool);
                    Entity tile = this._board.GetTileAtPosition(positionComponent.position);
                    EntityManager.SetComponentData(tile, new TileComponent {gem = null});
                    EntityManager.AddComponent<EmptyTileComponent>(tile);
                    EntityManager.DestroyEntity(entity);
                }
            });
        }
    }
}