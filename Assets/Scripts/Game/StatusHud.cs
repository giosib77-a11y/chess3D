using UnityEngine;

namespace Chess.Game
{
    /// <summary>Top-left OnGUI text: status line, optional error line, help line.</summary>
    public class StatusHud : MonoBehaviour
    {
        private const string HelpText = "Click: select/move  TAB: view  R: restart";

        private string _status = "";
        private string _error = "";
        private GUIStyle _statusStyle;
        private GUIStyle _errorStyle;
        private GUIStyle _helpStyle;

        public void Show(string status, string error)
        {
            _status = status ?? "";
            _error = error ?? "";
        }

        private void OnGUI()
        {
            // GUI.skin is only valid inside OnGUI, so styles are built lazily here.
            if (_statusStyle == null)
            {
                _statusStyle = CreateStyle(20, Color.white);
                _errorStyle = CreateStyle(18, new Color(1f, 0.85f, 0.2f));
                _helpStyle = CreateStyle(16, new Color(0.85f, 0.85f, 0.85f));
            }

            float width = Screen.width - 20f;
            float y = 10f;
            GUI.Label(new Rect(10f, y, width, 28f), _status, _statusStyle);
            y += 28f;
            if (!string.IsNullOrEmpty(_error))
            {
                GUI.Label(new Rect(10f, y, width, 26f), _error, _errorStyle);
                y += 26f;
            }
            GUI.Label(new Rect(10f, y, width, 24f), HelpText, _helpStyle);
        }

        private static GUIStyle CreateStyle(int fontSize, Color color)
        {
            var style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, fontStyle = FontStyle.Bold };
            style.normal.textColor = color;
            return style;
        }
    }
}
