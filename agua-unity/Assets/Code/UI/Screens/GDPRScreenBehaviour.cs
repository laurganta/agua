using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class GDPRScreenBehaviour : ScreenBehaviourBase
    {
        [SerializeField] private Button _acceptButton;

        public Action OnAccept;

        protected override void Awake()
        {
            base.Awake();
            _acceptButton.onClick.AddListener(OnAcceptClicked);
        }
        
        private void OnAcceptClicked()
        {
            OnAccept?.Invoke();
        }

    }
}