using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(FillSystem))]
    public class NewGemSystem : ComponentSystem
    {
        private Board _board;
        public void Init(Board board)
        {
            this._board = board;
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<GemComponent, NewGemComponent, TargetPositionComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref TargetPositionComponent targetPositionComponent
                ) =>
            {
                int instancePool = _board.GetFreeGemInstancePosition();
                gemComponent.instancePool = instancePool;
                
                EntityManager.AddComponent(entity, typeof(TypeComponent));
                int type = this._board.RandomizeColor(instancePool, targetPositionComponent.position);
                EntityManager.SetComponentData(entity, new TypeComponent {type = type});

                EntityManager.AddComponent(entity, typeof(PositionComponent));
                EntityManager.AddComponent(entity, typeof(WorldPositionComponent));
                int2 initialPosition = this._board.GetPositionInDirection(targetPositionComponent.position, Board.Direction.UP);
                EntityManager.SetComponentData(entity, new PositionComponent {position = initialPosition});
                EntityManager.SetComponentData(entity, new WorldPositionComponent {position = initialPosition});

                EntityManager.AddComponent(entity, typeof(WorldScaleComponent));
                float2 initialScale = new float2(1.0f, 1.0f);
                EntityManager.SetComponentData(entity, new WorldScaleComponent {scale = initialScale});
                
                this._board.SetGemPosition(gemComponent.instancePool, initialPosition);
                this._board.SetGemScale(gemComponent.instancePool, initialScale);
                
                EntityManager.RemoveComponent<NewGemComponent>(entity);
            });
        }
    }
}