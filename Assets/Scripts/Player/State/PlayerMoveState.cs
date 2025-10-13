using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : StateBase
{
    Player player => Player.Instance;
    protected override Animator animator => player.animator;
    public PlayerMoveState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        animator.SetBool("IsMoving", true);
    }

    public override void OnExit()
    {
        base.OnExit();
        animator.SetBool("IsMoving", false);
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(player.horizontalInput == 0 && player.verticalInput == 0 ||player.rb.velocity.magnitude<=0.1f)
        {
            StateMachine.ChangeState(player.idleState);
        }
        animator.SetFloat("InputX", player.horizontalInput);
        animator.SetFloat("InputZ", player.verticalInput);
    }
}
