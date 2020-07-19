using System.Collections.Generic;
using System.Linq;
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

    public int Width => _width;

    public int Height => _height;

    private int _height;
    private EntityManager _entityManager;
    private GameObject _tilePrefab;
    private Sprite[] _tileSprites;
    private GameObject _selected;

    private Dictionary<int2, Entity> _tiles = new Dictionary<int2, Entity>();
    private List<GameObject> _gems = new List<GameObject>();
    private Dictionary<int2, List<int>> _firstGemsDictionary = new Dictionary<int2, List<int>>();

    private Entity _player;

    public Entity Player => _player;

    public Board(int width, int height, EntityManager entityManager, GameObject tilePrefab, Sprite[] tileSprites,
        GameObject selected)
    {
        this._width = width;
        this._height = height;
        this._entityManager = entityManager;
        this._tilePrefab = tilePrefab;
        this._tileSprites = tileSprites;
        this._selected = selected;

        this.InitPlayer();
        this.FillFirstGems();
        this.InitGemPool();
        this.InitTiles();
    }

    private void InitPlayer()
    {
        this._player = _entityManager.CreateEntity();
        _entityManager.SetName(this._player, "player");
    }

    private void FillFirstGems()
    {
        List<int> possibleTypes = new List<int>();
        for (int i = 0; i < this._tileSprites.Length; i++)
        {
            possibleTypes.Add(i);
        }

        Dictionary<int2, int> gemsDictionary = new Dictionary<int2, int>();
        for (int x = 0; x < _width; x += 1)
        {
            for (int y = 0; y < _height; y += 1)
            {
                int2 position = new int2(x, y);
                int2 positionLeft = this.GetPositionInDirection(position, Direction.LEFT);
                int2 positionDown = this.GetPositionInDirection(position, Direction.DOWN);


                List<int> currPossibleTypes = new List<int>();
                currPossibleTypes.AddRange(possibleTypes);

                if (gemsDictionary.ContainsKey(positionLeft))
                {
                    currPossibleTypes.Remove(gemsDictionary[positionLeft]);
                }

                if (gemsDictionary.ContainsKey(positionDown))
                {
                    currPossibleTypes.Remove(gemsDictionary[positionDown]);
                }

                gemsDictionary[position] = currPossibleTypes[Random.Range(0, currPossibleTypes.Count)];
            }
        }

        foreach (KeyValuePair<int2, int> gemKV in gemsDictionary)
        {
            int2 key = gemKV.Key;
            key.y = this._height - 1;

            if (!this._firstGemsDictionary.ContainsKey(key))
            {
                this._firstGemsDictionary[key] = new List<int>();
            }

            this._firstGemsDictionary[key].Add(gemKV.Value);
        }
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
                gem.name = "pool_" + this._gems.Count;
                gem.SetActive(false);
                this._gems.Add(gem);
            }
        }
    }

    public int GetFreeGemInstancePosition()
    {
        int i = 0;
        foreach (GameObject gem in this._gems)
        {
            if (!gem.activeInHierarchy)
            {
                gem.transform.position = new Vector3(1000, 1000, 0);
                gem.SetActive(true);
                break;
            }

            i += 1;
        }

        return i;
    }

    public void FreeGem(int pool)
    {
        this._gems[pool].SetActive(false);
    }

    // Gem
    public int RandomizeColor(int pool, int2 position)
    {
        int type = -1;
        if (this._firstGemsDictionary.ContainsKey(position) && this._firstGemsDictionary[position].Count > 0)
        {
            type = this._firstGemsDictionary[position][0];
            this._firstGemsDictionary[position].RemoveAt(0);
        }
        else
        {
            type = Random.Range(0, this._tileSprites.Length);
        }


        SpriteRenderer spriteRenderer = this._gems[pool].GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = this._tileSprites[type];

        return type;
    }

    public void SetGemPosition(int pool, float2 position)
    {
        GameObject gem = this._gems[pool];

        gem.transform.position = new Vector3(position.x, position.y, 0.0f);
    }

    public void SetGemScale(int pool, float2 scale)
    {
        GameObject gem = this._gems[pool];
        gem.transform.localScale = new Vector3(scale.x, scale.y, 1.0f);
    }

    public void SetGemOnTop(int pool)
    {
        GameObject gem = this._gems[pool];
        Vector3 position = gem.transform.position;
        position.z = -1;
        gem.transform.position = position;
    }

    public void SetGemOnBottom(int pool)
    {
        GameObject gem = this._gems[pool];
        Vector3 position = gem.transform.position;
        position.z = 0;
        gem.transform.position = position;
    }

    public void HideSelect()
    {
        this._selected.SetActive(false);
    }

    public void SetSelectPosition(int pool)
    {
        this._selected.SetActive(true);
        this._selected.transform.position = new Vector3(this._gems[pool].transform.position.x,
            this._gems[pool].transform.position.y, -2);
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
        return this._tiles[position];
    }

    public bool HasTileAtPosition(int2 position)
    {
        return this._tiles.ContainsKey(position);
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

    public bool AllTilesFilled()
    {
        foreach (KeyValuePair<int2, Entity> tile in this._tiles)
        {
            if (_entityManager.HasComponent<EmptyTileComponent>(tile.Value))
            {
                return false;
            }

            TileComponent tileComponent = _entityManager.GetComponentData<TileComponent>(tile.Value);
            if (_entityManager.HasComponent<TargetPositionComponent>((Entity) tileComponent.gem))
            {
                return false;
            }
        }

        return true;
    }
    
    public bool AllTilesFilledAndChecked()
    {
        foreach (KeyValuePair<int2, Entity> tile in this._tiles)
        {
            if (_entityManager.HasComponent<EmptyTileComponent>(tile.Value))
            {
                return false;
            }

            TileComponent tileComponent = _entityManager.GetComponentData<TileComponent>(tile.Value);
            if (_entityManager.HasComponent<TargetPositionComponent>((Entity) tileComponent.gem) || _entityManager.HasComponent<CheckComponent>((Entity) tileComponent.gem))
            {
                return false;
            }
        }

        return true;
    }
}