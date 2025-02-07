using System;
using UnityEngine;

//Implements the idle game state, waiting for user input
public class IdleState : IBoardState
{
    public Action<Block> OnBlockClicked;

    public void Enter(BoardManager boardManager) { }

    public void Update(BoardManager boardManager) //Handles user mouse input for block clicks.
    {
        if (Input.GetMouseButtonDown(0))
        {
            Block clickedBlock = boardManager.GetClickedBlock();
            if (clickedBlock != null)
            {
                OnBlockClicked?.Invoke(clickedBlock);
            }
        }
    }

    public void Exit(BoardManager boardManager) { }
}