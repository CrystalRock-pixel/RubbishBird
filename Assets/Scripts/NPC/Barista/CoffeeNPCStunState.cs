using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 眩晕状态：NPC 在被击退后保持静止一段时间。
/// </summary>
public class CoffeeNPCStunState : CoffeeNPCStateBase
{
    private float _stunTimer;

    public CoffeeNPCStunState(StateMachine stateMachine, CoffeeNPC npc) : base(stateMachine, npc) { }

    public override void OnEnter()
    {
        base.OnEnter();
        //Debug.Log("进入眩晕状态");

        // 停止任何移动
        Npc.Move(Vector3.zero, 0);

        // 设置计时器
        _stunTimer = Npc.stunDuration;

        // 可以在这里添加眩晕动画或视觉效果 (例如：更改 Sprite 或颜色)
        Npc.spriteRenderer.color = Color.blue;
    }

    public override void OnUpdate()
    {
        _stunTimer -= Time.deltaTime;

        if (_stunTimer <= 0)
        {
            //Debug.Log("眩晕结束，切换回巡逻状态。");
            // 眩晕结束，重新进入 Patrol 状态
            StateMachine.ChangeState(new CoffeeNPCPatrolState(StateMachine, Npc));
        }

        // 注意：在 StunState 中，我们不需要检查视野或移动。
        // NPC 只是等待计时器结束。
    }

    public override void OnExit()
    {
        base.OnExit();
        // 恢复正常视觉效果
        Npc.spriteRenderer.color = Color.white;
    }
}
