using System;
using UnityEngine;
using UnityEngine.UI;

namespace CleverEdge
{
    public class EndScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private Button _returnToMainMenuButton;
        
        public Action OnReturnToMainMenu;

        private void Awake()
        {
            _returnToMainMenuButton.onClick.AddListener(() => OnReturnToMainMenu?.Invoke());
        }
    }
}