using UnityEngine;

public class ActorAction : MonoBehaviour {
    private ActorMovement actorMovement;
    private ActorAttack actorAttack;
    private ActorOperation actorOperation;
    private State state = State.KeyInput;
    private DungeonGenerator dg;
    private ActorParamsController actorParams;
    private Vector3 newPos => DirUtil.GetNewPos(actorMovement.currentPos, actorMovement.direction);

    void Start() {
        actorMovement = GetComponent<ActorMovement>();
        actorAttack = GetComponent<ActorAttack>();
        actorOperation = GetComponent<ActorOperation>();
        dg = GetComponentInParent<DungeonGenerator>();
        actorParams = GetComponent<ActorParamsController>();
    }

    // 独自の更新メソッド
    public void Proc() {
        switch (state) {
            case State.KeyInput: KeyInput(); 
                break;
            case State.AttackBegin: AttackBegin(); 
                break;
            case State.Attacking: Attacking(); 
                break;
            case State.AttackEnd: AttackEnd(); 
                break;
            case State.MoveBegin: MoveBegin(); 
                break;
            case State.Moving: Moving(); 
                break;
            case State.MoveEnd: MoveEnd(); 
                break;
            case State.TurnEnd: TurnEnd(); 
                break;
        }
    }

    // 現在の状態を返す
    public State GetState() => state;

    // KeyInputのとき呼ばれる    
    private void KeyInput() {
        //抽象メソッド実行して、次に行う状態を取得
        state = actorOperation.GetNextState(actorMovement);
        //移動開始以外なら歩行アニメーションを止める
        if (state != State.MoveBegin) 
            actorMovement.Stop();
    }

    // AttackBeginのとき呼ばれる
    private void AttackBegin() {
        //複数の敵に攻撃
        if(actorParams.parameter.longLv > 0 || actorParams.parameter.rangeLv > 0)
            actorAttack.AttackMulti(actorMovement);
        //一体の敵に攻撃
        else
        actorAttack.Attack(dg.GetExistActor(newPos));
        state = State.Attacking;
    }

    // Attackingのとき呼ばれる
    private void Attacking() {
        //攻撃アニメーションが続いている間はAttacking、終わればAttackEnd取得
        state = actorAttack.Attacking();
        if (state == State.AttackEnd) {
            //actorAttack.DamageOpponent(dg.GetExistActor(newPos));
        }
    }

    // AttackEndのとき呼ばれる
    private void AttackEnd() {
        state = State.TurnEnd;
    }

    // MoveBeginのとき呼ばれる
    private void MoveBegin() {
        //フレーム一回分移動する
        actorMovement.Walk();
        state = State.Moving;
    }

    // Movingのとき呼ばれる
    private void Moving() {
        //フレーム完了まで移動してからMoveEnd取得
        state = actorMovement.Walking();
    }

    // MoveEndのとき呼ばれる
    private void MoveEnd() {
        state = State.TurnEnd;
    }

    // ターン終了
    private void TurnEnd() {
        state = State.KeyInput;
        actorParams.RecoveryHp();
    }

    //歩行アニメーションをストップ
    public void StopWalkingAnimation() {
        actorMovement.Stop();
    }
}