using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveState : PlayerStateBase
{
    public PlayerMoveState(StateMachine stateMachine) : base(stateMachine)
    {
    }

    private float horizontalInput => player.horizontalInput;
    private float verticalInput=> player.verticalInput;

    private float moveSpeed => player.moveSpeed;
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

        // 计算移动方向向量
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;

        // 应用移动
        rb.velocity = new Vector3(moveDirection.x * moveSpeed, rb.velocity.y, moveDirection.z * moveSpeed);

        if (horizontalInput == 0 && verticalInput == 0 ||rb.velocity.magnitude<=0.1f)
        {
            StateMachine.ChangeState(player.idleState);
        }

        if (Input.GetKeyDown(KeyCode.Space)&&isGrounded)
        {
           StateMachine.ChangeState(player.jumpState);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            StateMachine.ChangeState(player.featherState);
        }

        animator.SetFloat("InputX",horizontalInput);
        animator.SetFloat("InputZ",verticalInput);
    }
}
