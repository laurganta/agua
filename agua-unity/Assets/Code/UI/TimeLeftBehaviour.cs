using System;
using UnityEngine;

namespace CleverEdge
{
    public class TimeLeftBehaviour : MonoBehaviour
    {
        public enum Type
        {
            MainMenu,
            RegisterMenu,
        }

        [SerializeField] private TMPro.TextMeshProUGUI _timerText;
        [SerializeField] private Type _type;
        
        private DateTime _targetTime;

        private void Awake()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void Initialize()
        {
            var matchDurationMinutes = ServiceLocator.GetInstance<GameManagerBehaviour>().MatchDurationMinutes;
            _targetTime = LeaderboardState.Provider.LastMatchStartTime.AddMinutes(matchDurationMinutes);
        }

        private void Update()
        {
            var timeLeft = _targetTime - DateTime.Now;
            
            _timerText.text = GetText(timeLeft);
        }

        private string GetText(TimeSpan timeLeft)
        {
            switch (_type)
            {
                case Type.MainMenu:
                    if (timeLeft.TotalSeconds < 0)
                    {
                        return _timerText.text = "00:00 until next can prize!";
                    }
            
                    return _timerText.text = $"{(timeLeft.Hours * 60 + timeLeft.Minutes):D2}:{timeLeft.Seconds:D2} until next can prize!";
                case Type.RegisterMenu:
                    if (timeLeft.TotalSeconds < 0)
                    {
                        return _timerText.text = "00:00 until next prize!";
                    }
            
                    return _timerText.text = $"{(timeLeft.Hours * 60 + timeLeft.Minutes):D2}:{timeLeft.Seconds:D2} until next prize!";
                default:
                    return string.Empty;
                
            }
        }
    }
}
