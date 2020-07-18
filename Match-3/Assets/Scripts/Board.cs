
using System.Collections.Generic;
using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board
{
    public enum Direction
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        NONE
    };
    
    private int _width;
    private int _height;
    private EntityManager _entityManager;
    private GameObject _tilePrefab;
    private Sprite[] _tileSprites;
    private GameObject _selected;

    private Dictionary<int2, Entity> _tiles = new Dictionary<int2, Entity>();
    private List<GameObject> gems = new List<GameObject>();

    public Board(int width, int height, EntityManager entityManager, GameObject tilePrefab, Sprite[] tileSprites, GameObject selected)
    {
        this._width = width;
        this._height = height;
        this._entityManager = entityManager;
        this._tilePrefab = tilePrefab;
        this._tileSprites = tileSprites;
        this._selected = selected;
        
        this.InitGemPool();
        this.InitTiles();
    }

    private void InitTiles()
    {
        EntityArchetype tileArchetype = _entityManager.CreateArchetype(
            typeof(TileComponent), 
            typeof(PositionComponent),
            typeof(EmptyTileComponent)
        );
        for (int x = 0; x < _width; x += 1)
        {
            for (int y = 0; y < _height; y += 1)
            {
                Entity tile = _entityManager.CreateEntity(tileArchetype);
                _entityManager.SetName(tile, "Tile" + x + "x" + y);
                int2 tilePosition = new int2(x, y);
                _entityManager.SetComponentData(tile, new PositionComponent {position = tilePosition});

                _tiles[tilePosition] = tile;
            }   
        }
    }

    // Pool
    private void InitGemPool()
    {
        for (int x = 0; x < _width; x += 1)
        {
            for (int y = 0; y < _height; y += 1)
            {
                GameObject gem = GameObject.Instantiate(this._tilePrefab);
                gem.SetActive(false);
                this.gems.Add(gem);
            }
        }
    }

    public int GetFreeGemInstancePosition()
    {
        int i = 0;
        foreach (GameObject gem in this.gems)
        {
            if (!gem.activeInHierarchy)
            {
                gem.transform.position = new Vector3(1000, 1000, 0);
                SpriteRenderer spriteRenderer = gem.GetComponent<SpriteRenderer>();
                spriteRenderer.sprite = this._tileSprites[Random.Range(0, this._tileSprites.Length)];
                gem.SetActive(true);
                return i;
            }
            i += 1;
        }

        return 0;
    }
    
    // Gem
    public void SetGemPosition(int pool, float2 position)
    {
        GameObject gem = this.gems[pool];
        
        gem.transform.position = new Vector3(position.x, position.y, 0.0f);
    }

    public void HideSelect()
    {
        this._selected.SetActive(false);
    }
    public void SetSelectPosition(int pool)
    {
        this._selected.SetActive(true);
        this._selected.transform.position = this.gems[pool].transform.position;
    }
    
    // Helpers
    public int2 GetPositionInDirection(int2 position, Direction direction)
    {
        int2 positionInDirection = new int2(position.x, position.y);
        if (direction == Direction.UP)
        {
            positionInDirection.y += 1;
        } 
        else if (direction == Direction.DOWN)
        {
            positionInDirection.y -= 1;
        } 
        else if (direction == Direction.RIGHT)
        {
            positionInDirection.x += 1;
        }
        else if (direction == Direction.LEFT)
        {
            positionInDirection.x -= 1;
        }

        return positionInDirection;
    }
    public Entity? GetTileInDirection(int2 position, Direction direction)
    {
        int2 positionInDirection = this.GetPositionInDirection(position, direction);

        if (positionInDirection.x < 0 || positionInDirection.x >= this._width)
        {
            return null;
        }

        if (positionInDirection.y < 0 || positionInDirection.y >= this._height)
        {
            return null;
        }

        return this._tiles[positionInDirection];
    }

    public Direction GetDirectionFrom(int2 position, int2 targetPosition)
    {
        if (position.y > targetPosition.y)
        {
            return Direction.DOWN;
        }
        
        if (position.y < targetPosition.y)
        {
            return Direction.UP;
        }
        
        if (position.x < targetPosition.x)
        {
            return Direction.RIGHT;
        }
        
        if (position.x > targetPosition.x)
        {
            return Direction.LEFT;
        }

        return Direction.NONE;
    }

    public Entity GetTileAtPosition(int2 position)
    {
        return _tiles[position];
    }

    public bool IsPositionAdjacent(int2 pos1, int2 pos2)
    {
        if (pos1.Equals(pos2))
        {
            return false;
        }
        
        int2 diff = new int2(pos1.x - pos2.x, pos1.y - pos2.y);
        diff.x = Mathf.Abs(diff.x);
        diff.y = Mathf.Abs(diff.y);

        if (diff.x != 0 && diff.y != 0)
        {
            return false;
        }
        
        if (diff.x > 1 || diff.y > 1)
        {
            return false;
        }

        return true;
    }
}
