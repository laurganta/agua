using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class ScreenBehaviourBase : MonoBehaviour
    {
        [SerializeField] private Button _backButton;
        
        public Action OnBack;
        
        protected virtual void Awake()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        protected virtual void OnBackButtonClicked()
        {
            OnBack?.Invoke();
        }
    }
}