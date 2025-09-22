using UnityEngine;

public class DirUtil : MonoBehaviour {

    private static GameObject lockOn;

    private void Start() {
        lockOn = transform.GetChild(0).gameObject;
    }

    //移動できるなら目的座標、できないなら現在座標を取得
    //敵の場合、周囲にプレイヤーがいて、移動先に壁がないことは確定
    public static Vector3 MoveIf(DungeonGenerator dg, Vector3 currentPos, EDir d) {
        //目的座標取得
        Vector3 newPos = GetNewPos(currentPos, d);
        Vector3 bottomLeft = new Vector3(newPos.x + dg.MapSize.x / 2, newPos.y + dg.MapSize.y / 2);
        GameObject self = dg.GetExistActor(currentPos);
        GameObject other = dg.GetExistActor(newPos);
        //移動先に壁があれば移動しない
        if (dg.IsWall(bottomLeft, d)) {
            if (other == null)
                lockOn.SetActive(false);
            else if (other.CompareTag("Enemy")) {
                lockOn.transform.position = newPos;
                lockOn.SetActive(true);
            }
            return currentPos;
        }
        //移動先に何もなければ移動する
        if (other == null) {
            if (self.CompareTag("Player"))
                lockOn.SetActive(false);
            return newPos;
        }
        //selfとother両方とも敵なら逆方向に進む
        if (self.CompareTag("Enemy") && other.CompareTag("Enemy")) {
            Pathfinding selfPathfind = self.GetComponent<Pathfinding>();
            selfPathfind.ShuffleWayPoints(self.GetComponent<ActorMovement>());
            Pathfinding otherPathfind = other.GetComponent<Pathfinding>();
            otherPathfind.ShuffleWayPoints(other.GetComponent<ActorMovement>());
            return currentPos;
        }
        if (self.CompareTag("Player")) {
            lockOn.transform.position = newPos;
            lockOn.SetActive(true);
        }
        return currentPos;
    }

   // 入力されたキーに対応する方向キーを返す
    public static EDir GetDirKey() {
        float x = SimpleInput.GetAxisRaw("Horizontal");
        float y = SimpleInput.GetAxisRaw("Vertical");
        return x == 0 && y == 1 ? EDir.Up :
                x == 1 && y == 1 ? EDir.RightUp :
                x == 1 && y == 0 ? EDir.Right :
                x == 1 && y == -1 ? EDir.RightDown :
                x == 0 && y == -1 ? EDir.Down :
                x == -1 && y == -1 ? EDir.LeftDown :
                x == -1 && y == 0 ? EDir.Left :
                x == -1 && y == 1 ? EDir.LeftUp :
                                    EDir.Pause;
    }

    // 目的座標に近づく方向を取得
    public static EDir GetNextDir(Vector3 nextPoint, Vector3 startPoint) {
        Vector3 dir = nextPoint - startPoint;
        return dir == Vector3.up ? EDir.Up :
                dir == new Vector3(1, 1) ? EDir.RightUp :
                dir == Vector3.right ? EDir.Right :
                dir == new Vector3(1, -1) ? EDir.RightDown :
                dir == Vector3.down ? EDir.Down :
                dir == new Vector3(-1, -1) ? EDir.LeftDown :
                dir == Vector3.left ? EDir.Left :
                dir == new Vector3(-1, 1) ? EDir.LeftUp :
                         EDir.Pause;
    }

    // 方向キーによってスプライトの向き変更
    public static void ChageFlipX(EDir d, SpriteRenderer sr) {
        sr.flipX = d switch {
            EDir.Right or EDir.RightUp or EDir.RightDown => false,
            EDir.Left or EDir.LeftUp or EDir.LeftDown => true,
            _ => sr.flipX,
        };
    }
    
    // 現在座標と方向キーを渡すと目的座標を取得
    public static Vector3 GetNewPos(Vector3 currentPos, EDir d, int longLv = 1) {
        Vector3 newPos = currentPos;
        return d switch {
            EDir.Up => newPos + Vector3.up * longLv,
            EDir.RightUp => newPos + new Vector3(1, 1) * longLv,
            EDir.Right => newPos + Vector3.right * longLv,
            EDir.RightDown => newPos + new Vector3(1, -1) * longLv,
            EDir.Down => newPos + Vector3.down * longLv,
            EDir.LeftDown => newPos + new Vector3(-1, -1) * longLv,
            EDir.Left => newPos + Vector3.left * longLv,
            EDir.LeftUp => newPos + new Vector3(-1, 1) * longLv,
            _ => newPos,
        };
    }

    public static float GetDirAngle(EDir d) {
        return d switch {
            EDir.Up => 0,
            EDir.RightUp => 45,
            EDir.Right => 90,
            EDir.RightDown => 135,
            EDir.Down => 180,
            EDir.LeftDown => 225,
            EDir.Left => 270,
            EDir.LeftUp => 315,
            _ => 0
        };
    }
}