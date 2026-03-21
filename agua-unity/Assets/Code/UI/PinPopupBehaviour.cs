using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class PinPopupBehaviour : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _confirmButton;
        
        private void Awake()
        {
            ServiceLocator.AddInstance(this);
            _root.SetActive(false);
        }

        public void Show(Action<string> onSubmit, Action onCancel)
        {
            _root.SetActive(true);
            _inputField.text = string.Empty;
            
            _confirmButton.onClick.RemoveAllListeners();
            _confirmButton.onClick.AddListener(() =>
            {
                _root.SetActive(false);
                onSubmit?.Invoke(_inputField.text);
            });
            
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(() =>
            {
                _root.SetActive(false);
                onCancel?.Invoke();
            });
        }
    }
}