using System;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace CleverEdge
{
    public enum SwipeDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class SwipeDetectionController
    {
        private enum State
        {
            Idle,
            Swiping
        }

        private State _state = State.Idle;

        public Action<SwipeDirection> OnSwipe { get; set; }

        private Vector2 _startPosition;

        // Used to determine if the swipe is valid
        private const int MIN_SWIPE_DISTANCE = 50;

        private readonly CancellationTokenSource _disposeCancellationTokenSource;

        public void Update()
        {
            DetectSwipesForTouchScreen();
        }

        private void DetectSwipesForTouchScreen()
        {
            switch (_state)
            {
                case State.Idle:
                    if (Touchscreen.current.primaryTouch.phase.value == TouchPhase.Began)
                    {
                        _startPosition = Touchscreen.current.primaryTouch.position.value;
                        _state = State.Swiping;
                    }

                    break;
                case State.Swiping:
                    if (Touchscreen.current.primaryTouch.phase.value == TouchPhase.Ended)
                    {
                        var endPosition = Touchscreen.current.primaryTouch.position.value;
                        CountPossibleSwipe(endPosition);
                        _state = State.Idle;
                    }

                    break;
            }
        }

        private void CountPossibleSwipe(Vector2 endPosition)
        {
            var direction = endPosition - _startPosition;

            if (direction.magnitude > MIN_SWIPE_DISTANCE)
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    OnSwipe?.Invoke(direction.x > 0 ? SwipeDirection.Right : SwipeDirection.Left);
                else
                    OnSwipe?.Invoke(direction.y > 0 ? SwipeDirection.Up : SwipeDirection.Down);
        }

    }
}