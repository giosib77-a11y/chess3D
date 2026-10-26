using TMPro;
using UnityEngine;

namespace Chess.Game
{
    /// <summary>Status and error lines shown in TMP objects of the HUD canvas.</summary>
    public class StatusHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text errorText;

        private void Awake()
        {
            if (statusText == null)
                Debug.LogError("[StatusHud] 'statusText' is not assigned.", this);
            if (errorText == null)
                Debug.LogError("[StatusHud] 'errorText' is not assigned.", this);
            else
                errorText.gameObject.SetActive(false);
        }

        public void Show(string status, string error)
        {
            if (statusText != null)
                statusText.text = status ?? "";

            if (errorText != null)
            {
                string value = error ?? "";
                errorText.text = value;
                errorText.gameObject.SetActive(value.Length > 0);
            }
        }
    }
}
