using Components;
using Unity.Entities;
using UnityEngine;

namespace Systems
{
    public class FillSystem : ComponentSystem
    {
        private Board _board;
        public void Init(Board board)
        {
            this._board = board;
        }

        private EntityArchetype gemArchetype;

        protected override void OnCreate()
        {
            base.OnCreate();
            this.gemArchetype = EntityManager.CreateArchetype(
                typeof(GemComponent),
                typeof(TargetPositionComponent),
                typeof(NewGemComponent),
                typeof(CheckComponent)
            );
        }

        protected override void OnUpdate()
        {
            Entities.WithAll<TileComponent, EmptyTileComponent>().ForEach((
                Entity entity, 
                ref TileComponent tileComponent, 
                ref EmptyTileComponent emptyTileComponent,
                ref PositionComponent positionComponent) =>
            {
                Entity? entityOnTop = _board.GetTileInDirection(positionComponent.position, Board.Direction.UP);
                if (entityOnTop == null)
                {
                    // generate
                    Entity gem = EntityManager.CreateEntity(this.gemArchetype);
                    EntityManager.SetComponentData(gem, new TargetPositionComponent {position = positionComponent.position});

                    tileComponent.gem = gem;
                    
                    EntityManager.RemoveComponent<EmptyTileComponent>(entity);
                }
                else if (!EntityManager.HasComponent<EmptyTileComponent>((Entity) entityOnTop))
                {
                    Entity? gem = EntityManager.GetComponentData<TileComponent>((Entity) entityOnTop).gem;
                    if (gem != null && !EntityManager.HasComponent<TargetPositionComponent>((Entity) gem))
                    {
                        EntityManager.SetComponentData((Entity) entityOnTop, new TileComponent {gem = null});
                        EntityManager.AddComponent<EmptyTileComponent>((Entity) entityOnTop);
                        
                        tileComponent.gem = gem;
                        EntityManager.AddComponent<TargetPositionComponent>((Entity) gem);
                        EntityManager.SetComponentData((Entity) gem, new TargetPositionComponent {position = positionComponent.position});
                        
                        EntityManager.RemoveComponent<EmptyTileComponent>(entity);
                    }
                }
            });
        }
    }
}