using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 追击状态：追踪玩家。
/// </summary>
public class CoffeeNPCChaseState : CoffeeNPCStateBase
{
    public CoffeeNPCChaseState(StateMachine stateMachine, CoffeeNPC npc) : base(stateMachine, npc) { }

    private float _chaseTimer;
    private const float ATTACK_DISTANCE = 1.0f;
    public override void OnEnter()
    {
        base.OnEnter();
        //Debug.Log("进入追击状态");

        _chaseTimer = Npc.maxChaseTime;
    }

    public override void OnUpdate()
    {
        _chaseTimer -= Time.deltaTime;

        // 追击超时处理 (功能 4)
        if (_chaseTimer <= 0)
        {
            //Debug.Log("追击超时，放弃追击，切换回巡逻状态。");
            StateMachine.ChangeState(new CoffeeNPCPatrolState(StateMachine, Npc));
            return;
        }

        // 持续检测玩家是否还在视野内 (功能 3)
        if (Npc.playerTarget == null || !Npc.IsPlayerInFieldOfView())
        {
            // 玩家跑出视野范围或目标丢失，切换回巡逻
            StateMachine.ChangeState(new CoffeeNPCPatrolState(StateMachine, Npc));
            return;
        }

        // 追逐玩家逻辑 (功能 2)
        Vector3 targetDir = Npc.playerTarget.position - Npc.transform.position;
        targetDir.y = 0; // 仅在水平面追逐

        if (targetDir.sqrMagnitude > ATTACK_DISTANCE * ATTACK_DISTANCE)
        {
            Npc.Move(targetDir.normalized, Npc.chaseSpeed);
        }
        else  //进入攻击范围
        {
            // 1. 计算击退方向：从 NPC 指向玩家的方向
            Vector3 kickDirection = (Npc.playerTarget.position - Npc.transform.position).normalized;
            Player player = Player.Instance;
            player.TakeHit(kickDirection, Npc.playerKickForce);
            StateMachine.ChangeState(new CoffeeNPCStunState(StateMachine, Npc));
            return;
        }
    }

    public override void OnExit()
    {
        base.OnExit();
        //Debug.Log("退出追击状态");
    }
}
