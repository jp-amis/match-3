using System;
using System.Collections.Generic;
using Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateBefore(typeof(MovementSystem))]
    public class CheckSystem : ComponentSystem
    {
        private Board _board;
        public void Init(Board board)
        {
            this._board = board;
        }
        
        protected override void OnUpdate()
        {
            if (!this._board.AllTilesFilled())
            {
                return;
            }
            
            Dictionary<int2, Entity> gemsToDestroy = new Dictionary<int2, Entity>();
            Entities.WithAll<GemComponent, CheckComponent>().WithNone<TargetPositionComponent>().ForEach((Entity entity) =>
            {
                
            });

            if (gemsToDestroy.Count > 0)
            {
                
            }
        }
    }
}