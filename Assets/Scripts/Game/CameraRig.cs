using Chess.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Chess.Game
{
    /// <summary>One camera, two computed poses: commander (behind the king) and tactical (above the board). TAB toggles.</summary>
    public class CameraRig : MonoBehaviour
    {
        public enum CameraView
        {
            Commander,
            Tactical
        }

        [SerializeField] private float transitionSpeed = 5f;
        [SerializeField] private float tacticalHeight = 9f;
        [SerializeField] private float tacticalDistance = 8f;
        [SerializeField] private float commanderEyeHeight = 3.0f;
        [SerializeField] private float commanderBackOffset = 1.6f;
        [SerializeField] private float commanderPitch = 30f;
        [SerializeField] private bool returnToCommanderAfterMove = true;
        [SerializeField] private bool startInCommanderView = true;

        private CameraView _view;
        private bool _viewInitialized;
        private bool _hasPose;
        private Square _kingSquare;
        private Side _side = Side.White;

        public CameraView CurrentView
        {
            get
            {
                EnsureViewInitialized();
                return _view;
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
                Toggle();

            if (!_hasPose)
                return;

            ComputeTarget(out Vector3 pos, out Quaternion rot);
            float t = 1f - Mathf.Exp(-transitionSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, pos, t);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, t);
        }

        public void SetCommander(Square kingSquare, Side side)
        {
            EnsureViewInitialized();
            _kingSquare = kingSquare;
            _side = side;
            if (!_hasPose)
            {
                _hasPose = true;
                ComputeTarget(out Vector3 pos, out Quaternion rot);
                transform.SetPositionAndRotation(pos, rot);
            }
        }

        public void SetView(CameraView view)
        {
            EnsureViewInitialized();
            _view = view;
        }

        public void Toggle()
        {
            SetView(CurrentView == CameraView.Commander ? CameraView.Tactical : CameraView.Commander);
        }

        public void NotifyHumanMoveMade()
        {
            if (returnToCommanderAfterMove && CurrentView == CameraView.Tactical)
                SetView(CameraView.Commander);
        }

        private void EnsureViewInitialized()
        {
            if (_viewInitialized)
                return;
            _viewInitialized = true;
            _view = startInCommanderView ? CameraView.Commander : CameraView.Tactical;
        }

        private void ComputeTarget(out Vector3 position, out Quaternion rotation)
        {
            Vector3 forward = BoardCoordinates.ForwardFor(_side);
            if (_view == CameraView.Commander)
            {
                position = BoardCoordinates.SquareToWorld(_kingSquare, commanderEyeHeight)
                    - forward * commanderBackOffset;
                rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(commanderPitch, 0f, 0f);
            }
            else
            {
                Vector3 center = BoardCoordinates.BoardCenter;
                position = center - forward * tacticalDistance + Vector3.up * tacticalHeight;
                rotation = Quaternion.LookRotation(center - position);
            }
        }
    }
}
