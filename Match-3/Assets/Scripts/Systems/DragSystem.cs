using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Systems
{
    [UpdateAfter(typeof(SelectSystem))]
    public class DragSystem : ComponentSystem
    {
        private float _thresholdIsDragging = 2.0f;
        private float _thresholdShouldSwap = 0.55f;
        private Board _board;
        private Camera _camera;

        public void Init(Board board, Camera camera)
        {
            this._board = board;
            this._camera = camera;
        }

        protected override void OnUpdate()
        {
            if (!this._board.AllTilesFilledAndChecked())
            {
                return;
            }
            
            if (EntityManager.HasComponent<IsSelectingComponent>(this._board.Player))
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = this._camera.ScreenToWorldPoint(Input.mousePosition);
                int2 position = new int2((int) Mathf.Ceil(worldPos.x - 0.5f), (int) Mathf.Ceil(worldPos.y - 0.5f));

                if (!this._board.HasTileAtPosition(position))
                {
                    return;
                }

                Entity tile = this._board.GetTileAtPosition(position);
                Entity? gem = EntityManager.GetComponentData<TileComponent>(tile).gem;

                if (gem != null && !EntityManager.HasComponent<TargetPositionComponent>((Entity) gem) &&
                    !EntityManager.HasComponent<DestroyComponent>((Entity) gem))
                {
                    EntityManager.AddComponent<OnTopComponent>((Entity) gem);

                    EntityManager.AddComponent<IsDraggingComponent>(this._board.Player);
                    float2 diff = new float2(worldPos.x - position.x, worldPos.y - position.y);

                    EntityManager.SetComponentData(this._board.Player,
                        new IsDraggingComponent
                        {
                            gem = (Entity) gem, pointerOrigin = Input.mousePosition, pointerDiff = diff,
                            verified = false
                        });
                }
            }

            if (EntityManager.HasComponent<IsDraggingComponent>(this._board.Player))
            {
                IsDraggingComponent isDraggingComponent =
                    EntityManager.GetComponentData<IsDraggingComponent>(this._board.Player);
                if (Input.GetMouseButtonUp(0))
                {
                    PositionComponent positionComponent =
                        EntityManager.GetComponentData<PositionComponent>(isDraggingComponent.gem);
                    EntityManager.AddComponent<TargetPositionComponent>(isDraggingComponent.gem);
                    EntityManager.SetComponentData(isDraggingComponent.gem,
                        new TargetPositionComponent {position = positionComponent.position});
                    
                    EntityManager.RemoveComponent<IsDraggingComponent>(this._board.Player);
                    return;
                }
                
                if (Mathf.Abs(Input.mousePosition.x - isDraggingComponent.pointerOrigin.x) > _thresholdIsDragging ||
                    Mathf.Abs(Input.mousePosition.y - isDraggingComponent.pointerOrigin.y) > _thresholdIsDragging)
                {
                    isDraggingComponent.verified = true;
                    EntityManager.SetComponentData(this._board.Player, isDraggingComponent);
                }

                if (isDraggingComponent.verified)
                {
                    GemComponent gemComponent = EntityManager.GetComponentData<GemComponent>(isDraggingComponent.gem);
                    PositionComponent positionComponent =
                        EntityManager.GetComponentData<PositionComponent>(isDraggingComponent.gem);
                    float2 newPos = this.NewDragPosition(positionComponent.position, isDraggingComponent.pointerOrigin,
                        isDraggingComponent.pointerDiff);
                    this._board.SetGemPosition(gemComponent.instancePool, newPos);

                    if (IsOnSwapThreshold(positionComponent.position, newPos))
                    {
                        Board.Direction direction = this.ShouldSwapToDirection(positionComponent.position, newPos);

                        if (direction != Board.Direction.NONE && this._board.HasTileAtPosition(this._board.GetPositionInDirection(positionComponent.position, direction)))
                        {
                            EntityManager.AddComponent<SelectedComponent>(
                                this._board.GetTileAtPosition(positionComponent.position));
                            EntityManager.AddComponent<SelectedComponent>((Entity) this._board.GetTileInDirection(positionComponent.position, direction));
                        }
                        else
                        {
                            EntityManager.AddComponent<TargetPositionComponent>(isDraggingComponent.gem);
                            EntityManager.SetComponentData(isDraggingComponent.gem,
                                new TargetPositionComponent {position = positionComponent.position});
                        }
                        
                        EntityManager.RemoveComponent<IsDraggingComponent>(this._board.Player);
                        EntityManager.AddComponent<InvalidMouseUpComponent>(this._board.Player);
                    }
                }
            }
        }

        private bool IsOnSwapThreshold(int2 gemPosition, float2 newPos)
        {
            float2 diff = new float2(Mathf.Abs(newPos.x - gemPosition.x), Mathf.Abs(newPos.y - gemPosition.y));
            if (diff.x > this._thresholdShouldSwap || diff.y > this._thresholdShouldSwap)
            {
                return true;
            }

            return false;
        }

        private Board.Direction ShouldSwapToDirection(int2 gemPosition, float2 newPos)
        {
            float2 diff = new float2(newPos.x - gemPosition.x, newPos.y - gemPosition.y);
            if (diff.x < this._thresholdShouldSwap * -1)
            {
                return Board.Direction.LEFT;
            }
            else if (diff.x > this._thresholdShouldSwap)
            {
                return Board.Direction.RIGHT;
            }
            else if (diff.y > this._thresholdShouldSwap)
            {
                return Board.Direction.UP;
            }
            else if (diff.y < this._thresholdShouldSwap * -1)
            {
                return Board.Direction.DOWN;
            }
            
            return Board.Direction.NONE;
        }

        private float2 NewDragPosition(int2 gemPosition, Vector3 pointerOrigin, float2 pointerDiff)
        {
            Vector3 worldPos = this._camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 originPos = this._camera.ScreenToWorldPoint(pointerOrigin);

            float2 newPos = new float2(worldPos.x - pointerDiff.x, worldPos.y - pointerDiff.y);

            float2 diffPos = new float2(originPos.x - newPos.x, originPos.y - newPos.y);
            if (Mathf.Abs(diffPos.x) > Mathf.Abs(diffPos.y))
            {
                newPos.y = gemPosition.y;
            }
            else
            {
                newPos.x = gemPosition.x;
            }

            return newPos;
        }
    }
}