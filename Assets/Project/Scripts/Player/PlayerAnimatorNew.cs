using ToySiege.Player;
using UnityEngine;

public class PlayerAnimatorNew : MonoBehaviour
{
    [SerializeField] Animator _animator;
    public PlayerInputHandler Input;
    public PlayerController _playerController;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.MoveInput.x;
        float moveY = Input.MoveInput.y;

        if (Input.SprintHeld)
        {
            moveX = moveX * 2;
            moveY = moveY * 2;
        }

        _animator.SetFloat("moveX", moveX);
        _animator.SetFloat("moveY", moveY);
        
    }
}
