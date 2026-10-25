using Chess.Core;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Builds placeholder primitive pieces for a position. No animation.</summary>
    public class PiecesView : MonoBehaviour
    {
        private const float PieceBaseY = 0.02f;

        [SerializeField] private Material whiteMaterial;
        [SerializeField] private Material blackMaterial;

        private Transform _container;

        public void Sync(Position position)
        {
            if (_container != null)
                Destroy(_container.gameObject);

            _container = new GameObject("Pieces").transform;
            _container.SetParent(transform, false);

            for (int index = 0; index < 64; index++)
            {
                Square square = new Square(index);
                Piece piece = position.GetPiece(square);
                if (!piece.IsNone)
                    CreatePiece(square, piece);
            }
        }

        private void CreatePiece(Square square, Piece piece)
        {
            Material material = piece.Side == Side.White ? whiteMaterial : blackMaterial;
            var root = new GameObject("Piece_" + square + "_" + piece.Side + piece.Type);
            root.transform.SetParent(_container, false);
            root.transform.position = BoardCoordinates.SquareToWorld(square, PieceBaseY);
            root.AddComponent<SquareRef>().Square = square;

            switch (piece.Type)
            {
                case PieceType.Pawn:
                    // Cylinder primitive is 2 tall at scale 1, so height 0.5 = scale.y 0.25.
                    AddPart(root.transform, PrimitiveType.Cylinder, material, new Vector3(0f, 0.25f, 0f), new Vector3(0.5f, 0.25f, 0.5f));
                    break;
                case PieceType.Rook:
                    AddPart(root.transform, PrimitiveType.Cube, material, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.6f, 0.6f));
                    break;
                case PieceType.Knight:
                    AddPart(root.transform, PrimitiveType.Cube, material, new Vector3(0f, 0.35f, 0f), new Vector3(0.45f, 0.7f, 0.65f));
                    if (piece.Side == Side.Black)
                        root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    break;
                case PieceType.Bishop:
                    // Capsule primitive is 2 tall at scale 1.
                    AddPart(root.transform, PrimitiveType.Capsule, material, new Vector3(0f, 0.4f, 0f), new Vector3(0.5f, 0.4f, 0.5f));
                    break;
                case PieceType.Queen:
                    AddPart(root.transform, PrimitiveType.Cylinder, material, new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.3f, 0.6f));
                    AddPart(root.transform, PrimitiveType.Sphere, material, new Vector3(0f, 0.75f, 0f), new Vector3(0.4f, 0.4f, 0.4f));
                    break;
                default: // King
                    AddPart(root.transform, PrimitiveType.Cube, material, new Vector3(0f, 0.35f, 0f), new Vector3(0.6f, 0.7f, 0.6f));
                    AddPart(root.transform, PrimitiveType.Cube, material, new Vector3(0f, 0.9f, 0f), new Vector3(0.15f, 0.3f, 0.15f));
                    AddPart(root.transform, PrimitiveType.Cube, material, new Vector3(0f, 0.95f, 0f), new Vector3(0.35f, 0.1f, 0.15f));
                    break;
            }
        }

        private static void AddPart(Transform parent, PrimitiveType type, Material material, Vector3 localPosition, Vector3 localScale)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = localPosition;
            part.transform.localScale = localScale;
            part.GetComponent<Renderer>().sharedMaterial = material;
        }
    }
}
