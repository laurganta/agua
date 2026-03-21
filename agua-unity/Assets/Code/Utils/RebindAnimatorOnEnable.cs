using UnityEngine;

public class RebindAnimatorOnEnable : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        _animator.Rebind();
    }
}
