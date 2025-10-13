using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    Player player;
    Animator animator;
    private void Start()
    {
        player = Player.Instance;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //if (player.isMoving)
        //{
        //    animator.SetBool("IsMoving", true);
        //    animator.SetFloat("InputX", player.horizontalInput);
        //    animator.SetFloat("InputZ", player.verticalInput);
        //    Debug.Log(player.horizontalInput + " " + player.verticalInput);
        //}
        //else
        //{
        //    animator.SetBool("IsMoving", false);
        //}
    }
}
