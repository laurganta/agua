using System;
using UnityEngine;

namespace CleverEdge
{
    public class GDPRStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private GDPRScreenBehaviour _screenBehaviour;

        public Action OnBack;
        public Action OnAccept;
        
        private void Awake()
        {
            _screenBehaviour.OnBack += OnBackClick;
            _screenBehaviour.OnAccept += OnAcceptClick;
        }
        
        private void OnDestroy()
        {
            _screenBehaviour.OnBack -= OnBackClick;
            _screenBehaviour.OnAccept -= OnAcceptClick;
        }
        
        private void OnBackClick()
        {
            OnBack?.Invoke();
        }
        
        private void OnAcceptClick()
        {
            OnAccept?.Invoke();
        }

        private void OnEnable()
        {
            _screenBehaviour.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            if (_screenBehaviour)
                _screenBehaviour.gameObject.SetActive(false);
        }
    }
}