using Chess.Core;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Tells raycasts which board square a tile or piece belongs to.</summary>
    public class SquareRef : MonoBehaviour
    {
        public Square Square { get; set; }
    }
}
