public interface IBoardState
{
    void Enter(BoardManager boardManager); //Called when the state is entered.
    void Update(BoardManager boardManager);//Called every frame while in this state.
    void Exit(BoardManager boardManager);//Called when the state is exited.
}