// 行動状態
public enum State {
    KeyInput,  // キー入力待ち。もしくは待機中

    // 攻撃
    AttackBegin,  // 開始
    Attacking,       // 実行中
    AttackEnd,    // 終了

    // 移動
    MoveBegin, // 開始
    Moving,      // 移動中
    MoveEnd,   // 完了

    TurnEnd,   // ターン終了
};