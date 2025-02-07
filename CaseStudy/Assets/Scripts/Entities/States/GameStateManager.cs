using System;
using System.Collections.Generic;
using UnityEngine;

//Manages the game state using a state machine pattern.
public class GameStateManager
{
    #region Variables
    private BoardManager boardManager;
    private IBoardState currentState;
    private Dictionary<BoardState, IBoardState> states;

    public Action<BoardState> OnStateChanged;
    #endregion


    public GameStateManager(BoardManager boardManager)
    {
        this.boardManager = boardManager;

        states = new Dictionary<BoardState, IBoardState> // Initialize states dictionary with all game states
        {
            { BoardState.Idle, new IdleState() },
            { BoardState.Removing, new RemovingState() },
            { BoardState.Gravity, new GravityState() },
            { BoardState.Refill, new RefillState() },
            { BoardState.Checking, new CheckingState() },
            { BoardState.Shuffling, new ShufflingState() }
        };
        currentState = states[BoardState.Idle];  // Set initial state to Idle
        currentState.Enter(boardManager);  // Enter initial state

        // Subscribe to state events for inter-state communication
        ((IdleState)states[BoardState.Idle]).OnBlockClicked += HandleBlockClicked;
        ((RemovingState)states[BoardState.Removing]).OnBlocksRemoved += HandleBlocksRemoved;
        ((ShufflingState)states[BoardState.Shuffling]).OnShuffleComplete += HandleShuffleComplete;
    }
    private void HandleBlockClicked(Block clickedBlock) //Handles the OnBlockClicked event from IdleState to initiate block removal.
    {
        boardManager.OnBlockClicked(clickedBlock); 
    }

    private void HandleBlocksRemoved() //Handles the OnBlocksRemoved event from RemovingState to transition to GravityState.
    {
        ChangeState(BoardState.Gravity);
    }

    private void HandleShuffleComplete() //Handles the OnShuffleComplete event from ShufflingState to transition to IdleState.
    {
        ChangeState(BoardState.Idle);
    }

    public void ChangeState(BoardState newState) //Changes the current game state to a new state.
    {
        currentState.Exit(boardManager);
        currentState = states[newState];
        currentState.Enter(boardManager);

        OnStateChanged?.Invoke(newState); // Invoke state changed event
    }

    public void Update() //Updates the current game state, calling the Update method of the current state.
    {
        currentState.Update(boardManager);
    }
    public IBoardState CurrentState { get { return currentState; } } //Gets the current game state.
    public Dictionary<BoardState, IBoardState> States { get { return states; } } //Gets the dictionary of all game states.
}