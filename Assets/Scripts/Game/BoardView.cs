using System.Collections.Generic;
using Chess.Core;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Creates the ground and the 64 tiles; highlights selection and legal targets.</summary>
    public class BoardView : MonoBehaviour
    {
        [SerializeField] private Material groundMaterial;
        [SerializeField] private Material tileMaterial;
        [SerializeField] private Material legalTargetMaterial;
        [SerializeField] private Material selectedMaterial;
        [SerializeField] private float groundSize = 30f;

        private readonly Renderer[] _tiles = new Renderer[64];

        private void Awake()
        {
            CreateGround();
            CreateTiles();
        }

        public void ShowSelection(Square selected, IEnumerable<Square> targets)
        {
            ClearHighlights();
            if (targets != null)
            {
                foreach (Square target in targets)
                    SetMaterial(target, legalTargetMaterial);
            }
            SetMaterial(selected, selectedMaterial);
        }

        public void ClearHighlights()
        {
            foreach (Renderer tile in _tiles)
            {
                if (tile != null)
                    tile.sharedMaterial = tileMaterial;
            }
        }

        private void SetMaterial(Square square, Material material)
        {
            Renderer tile = _tiles[square.Index];
            if (tile != null)
                tile.sharedMaterial = material;
        }

        private void CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.position = new Vector3(BoardCoordinates.BoardCenter.x, 0f, BoardCoordinates.BoardCenter.z);
            // Plane primitive is 10 x 10 units at scale 1.
            float scale = groundSize / 10f;
            ground.transform.localScale = new Vector3(scale, 1f, scale);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
        }

        private void CreateTiles()
        {
            for (int index = 0; index < 64; index++)
            {
                Square square = new Square(index);
                GameObject tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "Tile_" + square;
                tile.transform.SetParent(transform, false);
                tile.transform.position = BoardCoordinates.SquareToWorld(square, 0.01f);
                tile.transform.localScale = new Vector3(0.94f, 0.02f, 0.94f);
                tile.AddComponent<SquareRef>().Square = square;
                Renderer tileRenderer = tile.GetComponent<Renderer>();
                tileRenderer.sharedMaterial = tileMaterial;
                _tiles[index] = tileRenderer;
            }
        }
    }
}
