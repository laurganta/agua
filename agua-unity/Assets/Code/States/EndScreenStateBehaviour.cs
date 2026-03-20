using System;
using UnityEngine;

namespace CleverEdge
{
    public class EndScreenStateBehaviour : GameStateBehaviourBase
    {
        [SerializeField] private EndScreenBehaviour _endScreenBehaviour;
        
        public Action OnReturnToMainMenu;

        private void Awake()
        {
            _endScreenBehaviour.OnReturnToMainMenu = ReturnToMainMenu;
        }

        private void ReturnToMainMenu()
        {
            OnReturnToMainMenu?.Invoke();
        }

        private void OnEnable()
        {
            _endScreenBehaviour.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            _endScreenBehaviour.gameObject.SetActive(false);
        }
    }
}