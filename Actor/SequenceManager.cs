using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SequenceManagerにアタッチ
/// プレイヤーと敵を状態に応じて動かす
/// </summary>
public class SequenceManager : MonoBehaviour {
    private ActorAction playerAction;
    [SerializeField] private GameObject enemyParent;
    private List<ActorAction> attackEnemies = new List<ActorAction>();
    private List<ActorAction> moveEnemies = new List<ActorAction>();
    private List<ActorAction> operatedEnemies = new List<ActorAction>();
    private bool isDeterminedEnemyBehavior = false;

    void Start() {
        playerAction = GameObject.FindGameObjectWithTag("Player").GetComponent<ActorAction>();
    }

    void Update() {
        State pState = playerAction.GetState();
        if (pState == State.KeyInput || pState == State.AttackBegin || pState == State.Attacking) {
            playerAction.Proc();
            AllEnemyStopWalkingAnimation();
            return;
        }
        if (pState == State.TurnEnd) {
            AllOperatedProc();
            isDeterminedEnemyBehavior = false;
            operatedEnemies.Clear();
            return;
        }
        if (pState == State.AttackEnd && !isDeterminedEnemyBehavior) {
            DetermineAllEnemyBehaviour();
            return;
        }
        if (attackEnemies.Count < 1 && moveEnemies.Count < 1 && (pState == State.AttackEnd || pState == State.MoveEnd)) {
            AllOperatedProc();
            return;
        }
        if (pState == State.MoveBegin) {
            playerAction.Proc();
            DetermineAllEnemyBehaviour();
            isDeterminedEnemyBehavior = true;
            return;
        }
        if (pState == State.AttackEnd && attackEnemies.Count < 1) {
            ProcOperatedEnemy(moveEnemies, State.MoveEnd);
            return;
        }
        if (pState == State.AttackEnd && isDeterminedEnemyBehavior) {
            EnemyAttack();
            return;
        }
        if (pState == State.MoveEnd && moveEnemies.Count < 1) {
            EnemyAttack();
            return;
        }
        if (isDeterminedEnemyBehavior) {
            if (pState == State.Moving) 
                playerAction.Proc();
            ProcOperatedEnemy(moveEnemies, State.MoveEnd);
            return;
        }
    }

    // 動作したキャラクター全員の更新メソッドを呼び出す
    private void AllOperatedProc() {
        playerAction.Proc();
        foreach (ActorAction enemyAction in operatedEnemies) {
            enemyAction.Proc();
        }
    }

    // 全敵の動作決定
    private void DetermineAllEnemyBehaviour() {
        //プレイヤーに近い敵から行動するよう、近い順にソート
        List<ActorAction> enemies = enemyParent.GetComponentsInChildren<ActorAction>().ToList();
        Vector3 pPos = playerAction.GetComponent<ActorMovement>().currentPos;
        Comparison<ActorAction> p = (a, b) =>
        {
            Vector3 aPos = a.GetComponent<ActorMovement>().currentPos;
            Vector3 bPos = b.GetComponent<ActorMovement>().currentPos;
            float p_a = Mathf.Abs(aPos.x - pPos.x) + Mathf.Abs(aPos.y - pPos.y);
            float p_b = Mathf.Abs(bPos.x - pPos.x) + Mathf.Abs(bPos.y - pPos.y);
            return (int)(p_a - p_b);
        };
        enemies.Sort(p);

        foreach (ActorAction enemyAction in enemies) {
            enemyAction.Proc();
            State eState = enemyAction.GetState();
            if (eState == State.AttackBegin) {
                attackEnemies.Add(enemyAction);
                operatedEnemies.Add(enemyAction);
            } else if (eState == State.MoveBegin) {
                moveEnemies.Add(enemyAction);
                operatedEnemies.Add(enemyAction);
                enemyAction.Proc();
            }
        }
        attackEnemies.Reverse();
        moveEnemies.Reverse();
        isDeterminedEnemyBehavior = true;
    }

    // 敵を動作させる
    private void ProcOperatedEnemy(List<ActorAction> enemies, State targetState, bool isAll = true) {
        for (int i = enemies.Count - 1; i >= 0; i--) {
            enemies[i].Proc();
            State eState = enemies[i].GetState();
            if (eState == targetState) {
                enemies.RemoveAt(i);
                continue;
            }
            if (!isAll) 
                break;
        }
    }

    // 全敵の歩行アニメーションを止める
    private void AllEnemyStopWalkingAnimation() {
        foreach (ActorAction enemyAction in enemyParent.GetComponentsInChildren<ActorAction>()) {
            enemyAction.StopWalkingAnimation();
        }
    }

    // 敵一体攻撃させ、残りの敵は次のフレームで攻撃
    private void EnemyAttack() {
        ProcOperatedEnemy(attackEnemies, State.AttackEnd, false);
        playerAction.StopWalkingAnimation();
        AllEnemyStopWalkingAnimation();
    }
}