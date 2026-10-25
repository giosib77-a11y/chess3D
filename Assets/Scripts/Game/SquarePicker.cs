using System;
using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
    /// <summary>Turns left mouse clicks into board squares via a physics raycast.</summary>
    public class SquarePicker : MonoBehaviour
    {
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private float maxDistance = 200f;

        /// <summary>Raised on every left click; null when nothing with a SquareRef was hit.</summary>
        public event Action<Square?> Clicked;

        private void Update()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
                return;

            Camera cam = raycastCamera != null ? raycastCamera : Camera.main;
            if (cam == null)
                return;

            Square? square = null;
            Ray ray = cam.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                SquareRef squareRef = hit.collider.GetComponentInParent<SquareRef>();
                if (squareRef != null)
                    square = squareRef.Square;
            }
            Clicked?.Invoke(square);
        }
    }
}
