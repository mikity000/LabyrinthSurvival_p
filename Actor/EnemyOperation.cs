using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class EnemyOperation : ActorOperation {

    public Pathfinding pathfind;

    // プレイヤーがいる座標によって状態分岐
    public override State GetNextState(ActorMovement enemyMovement) {
        return AstarAI(enemyMovement);
    }

    //プレイヤーが近くにいれば攻撃開始、いなければ移動開始
    private State AstarAI(ActorMovement enemyMovement) {
        EDir d = GetPlayerDirection(enemyMovement);
        //プレイヤーがいなければ移動
        if (d == EDir.Pause) {
            d = GetNextMoveDir(enemyMovement);
            enemyMovement.SetDirection(d);
            //障害物が無ければ移動開始
            if (enemyMovement.IsMoveBegin()) 
                return State.MoveBegin;
            return State.KeyInput;
        }
        //プレイヤーがいれば攻撃開始
        enemyMovement.SetDirection(d);
        return State.AttackBegin;
    }

    //目的座標に近づく方向を取得
    private EDir GetNextMoveDir(ActorMovement enemyMovement) {
        Vector3 newPos = pathfind.ChoosePathFlexibly();
        return DirUtil.GetNextDir(newPos, enemyMovement.currentPos);
    }

    // 周囲のプレイヤーがいる方向を返す
    private EDir GetPlayerDirection(ActorMovement enemyMovement) {
        DungeonGenerator dg = GetComponentInParent<DungeonGenerator>();
        foreach (EDir d in Enum.GetValues(typeof(EDir))) {
            if (d == EDir.Pause) 
                continue;
            Vector3 newPos = DirUtil.GetNewPos(enemyMovement.currentPos, d);
            GameObject actor = dg.GetExistActor(newPos);
            if (actor == null) 
                continue;
            if (actor.CompareTag("Player")) 
                return d;
        }
        //周囲にプレイヤーがいなければPauseを返す
        return EDir.Pause;
    }
}