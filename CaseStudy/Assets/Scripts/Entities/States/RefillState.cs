using System;
using UnityEngine;
using DG.Tweening;

//Implements the refill game state, responsible for refilling the grid with new blocks.
public class RefillState : IBoardState
{
    #region Variables
    private float stateTimer;
    private RefillManager refillManager;
    private GridManager gridManager;
    #endregion

    public void Enter(BoardManager boardManager)//Initializes refill manager and starts the refill process.
    {
        stateTimer = 0.1f;
        refillManager = new RefillManager(boardManager);
        refillManager.Refill();
    }

    public void Update(BoardManager boardManager) //Manages the state timer and state transition.
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            boardManager.gameStateManager.ChangeState(BoardState.Checking);
            boardManager.groupManager.UpdateCombosAndSpritesForAffectedBlocks();
        }
    }

    public void Exit(BoardManager boardManager)
    {
    }
}