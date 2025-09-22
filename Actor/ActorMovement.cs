using UnityEngine;
public class ActorMovement : MonoBehaviour {
    public Animator anim;
    [HideInInspector] public Vector3 currentPos; //現在いるマス
    public EDir direction = EDir.Up;
    [SerializeField] private SpriteRenderer sr;

    public float maxPerFrame;
    private float complementFrame;
    private int currentFrame = 0;
    [HideInInspector] public Vector3 newPos; //目的地のマス
    private DungeonGenerator dg;
    private readonly int walkHash = Animator.StringToHash("Walk");

    void Start() {
        currentPos = transform.position;
        dg = GetComponentInParent<DungeonGenerator>();
        newPos = currentPos;
    }

    //Moving状態のときに入る   
    public State Walking() {
        //目的地に到着したらMoveEnd(1マスごと)
        if (currentPos.Equals(newPos) && currentFrame == 0)
            return State.MoveEnd;
        //フレームの完了がまだなら現在座標、完了なら目的座標を代入
        currentPos = Move(currentPos, newPos, ref currentFrame);
        return State.Moving;
    }

    //歩行アニメーションを止める
    public void Stop() {
        if (anim.GetBool(walkHash)) {
            anim.SetBool(walkHash, false);
        }
    }

    //フレームの完了がまだなら現在座標、完了なら目的座標を代入
    private Vector3 Move(Vector3 currentPos, Vector3 newPos, ref int currentFrame) {
        currentFrame++;
        complementFrame = maxPerFrame / Time.smoothDeltaTime;
        float t = currentFrame / complementFrame;
        float x = currentPos.x + (newPos.x - currentPos.x) * t;
        float y = currentPos.y + (newPos.y - currentPos.y) * t;
        transform.position = new Vector3(x, y);
        anim.SetBool(walkHash, x != 0 || y != 0);
        //現在のフレームが完了フレームを超えたら目的座標を代入
        if (complementFrame <= currentFrame) {
            currentFrame = 0;
            transform.position = newPos;
            return newPos;
        }
        return currentPos;
    }

    //MoveBegin状態のときに入る
    public void Walk() {
        if (currentFrame > 0) 
            return;
        currentPos = Move(currentPos, newPos, ref currentFrame);
    }


    //方向キーに合わせてflipXも変更する
    public void SetDirection(EDir d) {
        direction = d;
        DirUtil.ChageFlipX(d, sr);
    }

    //移動開始できるかどうか
    public bool IsMoveBegin() {
        //移動先に障害物が無ければ目的座標取得
        newPos = DirUtil.MoveIf(dg, currentPos, direction);
        //現在座標と目的座標が同じなら移動しない
        if (currentPos.Equals(newPos)) 
            return false;
        return true;
    }
}