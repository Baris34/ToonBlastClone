using System; 
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
/*
 Implements the block removing state of the game.
 Handles the visual removal of blocks and triggers gravity state after completion.
*/
public class RemovingState : IBoardState
{
    #region Variables

    public Action OnBlocksRemoved;
    private float stateTimer;
    public List<Block> BlocksToRemove { get; set; }
    #endregion
    public RemovingState() 
    {
        BlocksToRemove = new List<Block>();
    }
    public void Enter(BoardManager boardManager) // Starts the block removal animation.
    {
        stateTimer = 0.3f;

        foreach (Block b in BlocksToRemove)
        {
            b.transform.DOScale(Vector3.zero, stateTimer).SetEase(Ease.InBack);
        }
    }

    public void Update(BoardManager boardManager) // Checks if the block removal animation is complete.
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0)
        {
            foreach (Block b in BlocksToRemove)
            {
                boardManager.RemoveBlockFromGrid(b);
            }
            BlocksToRemove.Clear();

            OnBlocksRemoved?.Invoke(); 
        }
    }

    public void Exit(BoardManager boardManager) { }
}