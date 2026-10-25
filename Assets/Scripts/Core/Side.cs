namespace Chess.Core
{
    public enum Side : byte { White = 0, Black = 1 }

    public static class SideExtensions
    {
        public static Side Opposite(this Side side)
        {
            return side == Side.White ? Side.Black : Side.White;
        }
    }
}
