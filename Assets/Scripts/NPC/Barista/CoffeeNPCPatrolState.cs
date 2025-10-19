using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巡逻状态：按指定路线行走（循环/不循环）。
/// </summary>
public class CoffeeNPCPatrolState : CoffeeNPCStateBase
{
    private int _currentWaypointIndex = 0;
    private Vector3 _targetWaypoint;

    public CoffeeNPCPatrolState(StateMachine stateMachine, CoffeeNPC npc) : base(stateMachine, npc) { }

    public override void OnEnter()
    {
        base.OnEnter();
        // 初始化巡逻点
        if (Npc.patrolPath != null && Npc.patrolPath.Count > 0)
        {
            //_currentWaypointIndex = 0;
            _currentWaypointIndex = FindNearestWaypointIndex();
            _targetWaypoint = Npc.patrolPath[_currentWaypointIndex];
        }
    }

    public int FindNearestWaypointIndex()
    {
        int nearestIndex = 0;
        float nearestSqrDistance = float.MaxValue;
        Vector3 npcPosition = Npc.transform.position;
        for (int i = 0; i < Npc.patrolPath.Count; i++)
        {
            float sqrDistance = (Npc.patrolPath[i] - npcPosition).sqrMagnitude;
            if (sqrDistance < nearestSqrDistance)
            {
                nearestSqrDistance = sqrDistance;
                nearestIndex = i;
            }
        }
        return nearestIndex;
    }

    public override void OnUpdate()
    {
        // 检查是否切换到追击状态 (功能 3)
        // ⚠️ 注意：这个检查也可以放在 CoffeeNPC 的 Update 中，作为统一的全局切换条件。
        if (Npc.IsPlayerInFieldOfView())
        {
            StateMachine.ChangeState(new CoffeeNPCChaseState(StateMachine, Npc));
            return;
        }

        // 路线行走逻辑 (功能 2)
        if (Npc.patrolPath == null || Npc.patrolPath.Count == 0) return;

        Vector3 direction = _targetWaypoint - Npc.transform.position;
        direction.y = 0; // 保持水平移动

        if (direction.sqrMagnitude < 0.5f * 0.5f) // 接近目标点
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= Npc.patrolPath.Count)
            {
                if (Npc.isLoopingPath)
                {
                    _currentWaypointIndex = 0; // 循环
                }
                else
                {
                    _currentWaypointIndex = Npc.patrolPath.Count - 1;
                    // 非循环：可以切换到 IdleState
                    // StateMachine.ChangeState(new IdleState(StateMachine, Npc));
                    Npc.Move(Vector3.zero, 0); // 停下
                    return;
                }
            }
            _targetWaypoint = Npc.patrolPath[_currentWaypointIndex];
        }

        // 移动到目标点
        Npc.Move(direction.normalized, Npc.patrolSpeed);
    }
}
