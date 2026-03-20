using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class CopyTextBehaviour : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textToCopy;
        [SerializeField] private Button _button;

        private void Awake()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            GUIUtility.systemCopyBuffer = _textToCopy.text;
            
            AndroidUtils.Show($"Copied \"{_textToCopy.text}\" to clipboard");
        }
    }
}