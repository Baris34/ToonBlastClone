using System;

//Implements the checking game state, responsible for checking for deadlock situations.
public class CheckingState : IBoardState
{
    public void Enter(BoardManager boardManager)
    {
    }

    public void Update(BoardManager boardManager) //Checks for deadlock and handles state transitions.
    {
        bool noMoves = boardManager.deadlockSystem.CheckNoMoves(boardManager.grid, boardManager);
        if (noMoves)
        {
            boardManager.gameStateManager.ChangeState(BoardState.Shuffling);
        }
        else
        {
            boardManager.gameStateManager.ChangeState(BoardState.Idle);
        }
    }

    public void Exit(BoardManager boardManager)
    {
    }
}