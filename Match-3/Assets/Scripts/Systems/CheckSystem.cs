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

            bool shouldCheck = false;
            Entities.WithAll<GemComponent, CheckComponent>().WithNone<NewGemComponent, TargetPositionComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref PositionComponent positionComponent
            ) =>
            {
                EntityManager.RemoveComponent<CheckComponent>(entity);
                shouldCheck = true;
            });

            if (!shouldCheck)
            {
                return;
            }
            
            Dictionary<int2, Entity> gemsToDestroy = new Dictionary<int2, Entity>();
            for (int x = 0; x < this._board.Width; x += 1)
            {
                for (int y = 0; y < this._board.Height; y += 1)
                {
                    this.CheckDirection(new int2(x, y), Board.Direction.UP, ref gemsToDestroy);
                    this.CheckDirection(new int2(x, y), Board.Direction.RIGHT, ref gemsToDestroy);
                }
            }
            
            if (gemsToDestroy.Count > 0)
            {
                foreach (KeyValuePair<int2,Entity> gemKV in gemsToDestroy)
                {
                    EntityManager.AddComponent<DestroyComponent>(gemKV.Value);
                }
            }
        }

        private Entity GetGemAtPosition(int2 position)
        {
            Entity tile = this._board.GetTileAtPosition(position);
            Entity gem = (Entity) EntityManager.GetComponentData<TileComponent>(tile).gem;

            return gem;
        }
        
        private int GetGemTypeAtPosition(int2 position)
        {
            Entity gem = this.GetGemAtPosition(position);
            int type = EntityManager.GetComponentData<TypeComponent>(gem).type;

            return type;
        }

        private void CheckDirection(int2 position, Board.Direction dir, ref Dictionary<int2, Entity> gemsToDestroy)
        {
            List<int2> positionsToDestroy = new List<int2>();
            int type = this.GetGemTypeAtPosition(position);
            int2 positionEvaluate = new int2(position.x, position.y);
            int checkedType = -1;
            int count = 0;
            do
            {
                count += 1;
                positionsToDestroy.Add(positionEvaluate);
                checkedType = -1;
                
                // Get next
                Entity? tileInDir = this._board.GetTileInDirection(positionEvaluate, dir);
                if (tileInDir != null)
                {
                    positionEvaluate = this._board.GetPositionInDirection(positionEvaluate, dir);
                    checkedType = this.GetGemTypeAtPosition(positionEvaluate);
                }
            } while (type == checkedType);

            if (count >= 3)
            {
                foreach (int2 positionToDestroy in positionsToDestroy)
                {
                    gemsToDestroy[positionToDestroy] = this.GetGemAtPosition(positionToDestroy);
                }
            }
        }
    }
}