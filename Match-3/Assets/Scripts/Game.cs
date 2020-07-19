using System.Collections;
using System.Collections.Generic;
using Systems;
using Unity.Entities;
using UnityEngine;

public class Game : MonoBehaviour
{
    public int boardWidth;
    public int boardHeight;
    public float gemVelocity;
    public GameObject tilePrefab;
    public GameObject selected;
    public Sprite[] tileSprites;
    public Camera mainCamera;
    
    void Start()
    {
        this.selected.SetActive(false);
        mainCamera.transform.position = new Vector3((this.boardWidth * 0.5f) - 0.5f, (this.boardHeight * 0.5f) - 0.5f, -10);
        
        // TODO: Remove debug stuff
        // for (int x = 0; x < this.boardWidth; x += 1)
        // {
        //     for (int y = 0; y < this.boardHeight; y += 1)
        //     {
        //         Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        //     }
        // }

        var world = World.DefaultGameObjectInjectionWorld;
        EntityManager entityManager = world.EntityManager;
        
        Board board = new Board(this.boardWidth, this.boardHeight, entityManager, this.tilePrefab, this.tileSprites, this.selected);
        
        world.GetOrCreateSystem<FillSystem>().Init(board);
        world.GetOrCreateSystem<NewGemSystem>().Init(board);
        world.GetOrCreateSystem<MovementSystem>().Init(board, this.gemVelocity);
        world.GetOrCreateSystem<SelectSystem>().Init(board, this.mainCamera);
        world.GetOrCreateSystem<SwapSystem>().Init(board);
        world.GetOrCreateSystem<CheckSystem>().Init(board);
    }
}
