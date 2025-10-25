using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleState : PlayerStateBase
{
    public PlayerIdleState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        if(player.horizontalInput != 0 || player.verticalInput != 0)
        {
            StateMachine.ChangeState(player.moveState);
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            StateMachine.ChangeState(player.jumpState);
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            StateMachine.ChangeState(player.featherState);
        }
    }
}
