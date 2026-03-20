using UnityEngine;

namespace CleverEdge
{
    public class WinLoseTextBehaviour : MonoBehaviour
    { 
        private static readonly int WinHash = Animator.StringToHash("Win");
        private static readonly int LoseHash = Animator.StringToHash("Lose");
        
        [SerializeField] Animator _animator;
        [SerializeField] private GameObject _winText;
        [SerializeField] private GameObject _loseText;

        public void SetWin(bool win)
        {
            if (win)
                _animator.SetTrigger(WinHash);
            else
                _animator.SetTrigger(LoseHash);
            
            _winText.SetActive(win);
            _loseText.SetActive(win == false);
        }
    }
}