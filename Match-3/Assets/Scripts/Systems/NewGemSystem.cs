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
            Entities.WithAllReadOnly<GemComponent, NewGemComponent, TargetPositionComponent>().ForEach((
                Entity entity,
                ref GemComponent gemComponent,
                ref TargetPositionComponent targetPositionComponent
                ) =>
            {
                int instancePool = _board.GetFreeGemInstancePosition();
                EntityManager.SetComponentData(entity, new GemComponent {instancePool = instancePool});
                
                int type = this._board.RandomizeColor(instancePool);
                EntityManager.SetComponentData(entity, new TypeComponent {type = type});
                
                EntityManager.AddComponent(entity, typeof(PositionComponent));
                EntityManager.AddComponent(entity, typeof(WorldPositionComponent));
                int2 initialPosition = this._board.GetPositionInDirection(targetPositionComponent.position, Board.Direction.UP);
                EntityManager.SetComponentData(entity, new PositionComponent {position = initialPosition});
                EntityManager.SetComponentData(entity, new WorldPositionComponent {position = initialPosition});

                this._board.SetGemPosition(gemComponent.instancePool, initialPosition);
                
                EntityManager.RemoveComponent<NewGemComponent>(entity);
            });
        }
    }
}