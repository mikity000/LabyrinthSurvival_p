using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerOperation : ActorOperation {
    //押されたキーによって状態分岐
    public override State GetNextState(ActorMovement actorMovement) {
        //キーが押されていない
        if (!Input.anyKey) 
            return State.KeyInput;
        //UI以外の画面をクリックまたはタップしたら攻撃開始
        if (Input.mousePresent && Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject()
        || Input.touchCount > 0 && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return State.AttackBegin;

        EDir d = DirUtil.GetDirKey();
        //方向キーが押されていたら
        if (d != EDir.Pause) {
            actorMovement.SetDirection(d);
            //移動先に障害物が無ければ移動開始
            if (actorMovement.IsMoveBegin()) 
                return State.MoveBegin;
        }
        return State.KeyInput;
    }

}