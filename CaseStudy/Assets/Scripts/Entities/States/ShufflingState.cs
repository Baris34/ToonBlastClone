using System;
using UnityEngine;

//Implements the shuffling game state, responsible for shuffling blocks to resolve deadlock situations.
public class ShufflingState : IBoardState
{
    public Action OnShuffleComplete;

    public void Enter(BoardManager boardManager) //Starts the smart shuffle process in DeadlockSystem.
    {
        boardManager.deadlockSystem.SmartShuffle(boardManager.grid, boardManager);
    }

    public void Update(BoardManager boardManager) //Invokes the OnShuffleComplete event.
    {
        OnShuffleComplete?.Invoke();
    }

    public void Exit(BoardManager boardManager) { }
}