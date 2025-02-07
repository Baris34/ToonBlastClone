using System;
using UnityEngine;
using DG.Tweening;

//Implements the gravity game state, applying gravity to blocks and transitioning to refill state.
public class GravityState : IBoardState
{
    #region Variables
    private float stateTimer;
    private GravityManager gravityManager;
    #endregion

    public void Enter(BoardManager boardManager) //Initializes gravity manager and applies gravity.
    {
        stateTimer = 0.3f;
        gravityManager= new GravityManager(boardManager);
        gravityManager.ApplyGravity();
    }
    public void Update(BoardManager boardManager) //Manages the state timer and state transition.
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            boardManager.gameStateManager.ChangeState(BoardState.Refill);
        }
    }
    public void Exit(BoardManager boardManager) 
    {
    }
}