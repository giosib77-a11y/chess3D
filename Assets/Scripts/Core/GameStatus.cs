namespace Chess.Core
{
    public enum GameStatus
    {
        InProgress,
        Checkmate,
        Stalemate,
        FiftyMoveRule,
        ThreefoldRepetition,
        InsufficientMaterial
    }

    public enum GameResult
    {
        None,
        WhiteWins,
        BlackWins,
        Draw
    }
}
