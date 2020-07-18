using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(NewGemPositioningSystem))]
    public class MovementSystem : ComponentSystem
    {
        private float _velocity;
        
        private Board _board;
        
        public void Init(Board board, float velocity)
        {
            this._board = board;
            this._velocity = velocity;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<GemComponent, WorldPositionComponent, PositionComponent, TargetPositionComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref WorldPositionComponent worldPositionComponent,
                ref PositionComponent positionComponent,
                ref TargetPositionComponent targetPositionComponent
                ) =>
            {
                // Time.DeltaTime
                Board.Direction direction =
                    this._board.GetDirectionFrom(positionComponent.position, targetPositionComponent.position);

                float diff = 0.0f;
                
                if (direction == Board.Direction.DOWN)
                {
                    worldPositionComponent.position.y -= Time.DeltaTime * this._velocity;
                    diff = worldPositionComponent.position.y - targetPositionComponent.position.y;
                }
                else if (direction == Board.Direction.UP)
                {
                    worldPositionComponent.position.y += Time.DeltaTime * this._velocity;
                    diff = targetPositionComponent.position.y - worldPositionComponent.position.y;
                }
                else if (direction == Board.Direction.LEFT)
                {
                    worldPositionComponent.position.x -= Time.DeltaTime * this._velocity;
                    diff = worldPositionComponent.position.x - targetPositionComponent.position.x;
                }
                else if (direction == Board.Direction.RIGHT)
                {
                    worldPositionComponent.position.x += Time.DeltaTime * this._velocity;
                    diff = targetPositionComponent.position.x - worldPositionComponent.position.x;
                }
                

                if (diff <= 0.0f)
                {
                    worldPositionComponent.position.y = targetPositionComponent.position.y;
                    worldPositionComponent.position.x = targetPositionComponent.position.x;
                    positionComponent.position.y = targetPositionComponent.position.y;
                    positionComponent.position.x = targetPositionComponent.position.x;
                    EntityManager.RemoveComponent<TargetPositionComponent>(entity);
                }
                
                this._board.SetGemPosition(gemComponent.instancePool, worldPositionComponent.position);
            });
        }
    }
}