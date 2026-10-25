using Chess.Core;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Square to world mapping: square (file f, rank r) center = (f, 0, r).</summary>
    public static class BoardCoordinates
    {
        public static Vector3 BoardCenter => new Vector3(3.5f, 0f, 3.5f);

        public static Vector3 SquareToWorld(Square sq, float height = 0f)
        {
            return new Vector3(sq.File, height, sq.Rank);
        }

        /// <summary>White moves toward +z, black toward -z.</summary>
        public static Vector3 ForwardFor(Side side)
        {
            return side == Side.White ? Vector3.forward : Vector3.back;
        }
    }
}
