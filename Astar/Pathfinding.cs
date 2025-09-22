using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// エネミープレファブにアタッチ
/// A*アルゴリズムで得られるパスを取得するクラス
/// </summary>
public class Pathfinding : MonoBehaviour {

    private Grid grid;
    [HideInInspector] public Transform target;
    private PlayerCD playerCD;
    private EnemyCD enemyCD;
    [HideInInspector] public List<Transform> waypoints = new List<Transform>();
    [HideInInspector] public Transform nextWaypoint;
    public List<Node> finalPath = new List<Node>(); //プレイヤーまたはwaypointまでのパス
    private DungeonGenerator dg;

    //視野調節用
    [Range(0, 180)] public float angle;
    public float radius;
    [SerializeField] private LayerMask wallMask;
    [HideInInspector] public bool isDetect;

    private void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<Grid>();
        target = GameObject.FindGameObjectWithTag("Player").transform;
        playerCD = target.GetComponent<PlayerCD>();
        enemyCD = GetComponent<EnemyCD>();
        waypoints = GameObject.FindGameObjectsWithTag("Waypoint").ToList().ConvertAll(v => v.transform);
        nextWaypoint = waypoints[Random.Range(0, waypoints.Count)];
        dg = GetComponentInParent<DungeonGenerator>();
    }

    public Vector3 ChoosePathFlexibly()
    {
        //プレイヤーを発見したら目的地をプレイヤーに設定する
        if (IsDetect())
            FindPath(transform.position, target.position);
        //巡回中に目的地に到着したら、次の目的地を設定する
        else if ((transform.position - nextWaypoint.position).sqrMagnitude == 0) { 
            nextWaypoint = waypoints[Random.Range(0, waypoints.Count)];
            FindPath(transform.position, nextWaypoint.position);
        }
        //プレイヤーが近くにいない場合、設定した目的地に進む
        else
            FindPath(transform.position, nextWaypoint.position);
        return finalPath[0].worldPoint;
    }

    private bool IsDetect()
    {
        //敵とプレイヤーが同じ部屋に居れば発見
        if (enemyCD.stayingRoomName == playerCD.stayingRoomName)
            return isDetect = true;

        Vector3 dirToTarget = target.position - transform.position;
        if (dirToTarget.sqrMagnitude <= radius * radius && finalPath.Count > 1)
        {
            Vector3 dirToMove = finalPath[1].worldPoint - transform.position;
            bool isInView = Vector3.Angle(dirToMove, dirToTarget) < angle;
            //「発見中」かつ「黄線内またはプレイヤーとの距離が5以下」なら発見中を継続
            if (isDetect && (isInView || dirToTarget.sqrMagnitude <= 25))
                return isDetect;

            bool isWall = Physics2D.Linecast(transform.position, target.position, wallMask);
            //「黄線内」かつ「間に壁がない」ならプレイヤーを発見
            if (isInView && !isWall)
                return isDetect = true;
        }
        return isDetect = false;
    }

    public void ShuffleWayPoints(ActorMovement actorMovement) {
        Vector3 newPos = DirUtil.GetNewPos(actorMovement.currentPos, actorMovement.direction);
        Vector3 dir = newPos - actorMovement.currentPos;
        foreach (Transform waypoint in waypoints.Shuffle()) {
            nextWaypoint = waypoint;
            Vector3 latestPos = ChoosePathFlexibly();
            if ((latestPos - actorMovement.currentPos) != dir)
                break;
        }
    }

    public void FindPath(Vector2 startPos, Vector2 targetPos)
    {
        Node startNode = grid.GetNodeFromWorldPosition(startPos);//Startのノード取得
        Node targetNode = grid.GetNodeFromWorldPosition(targetPos);//Targetのノード取得

        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while(openList.Count > 0)
        {
            Node currentNode = openList[0];
            //openListの要素数が2以上ならループ
            for (int i = 1; i < openList.Count; i++)
            {
                //2要素目以降のノードのFコストが現在のノードのFコスト未満である場合
                if (openList[i].fCost < currentNode.fCost)
                    currentNode = openList[i];
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode == targetNode)
                GetFinalPath(startNode, targetNode);

            //currentNode周辺のノードでループ
            foreach (Node neighboringNode in grid.GetNeighboringNodes(currentNode))
            {
                //「neighboringNodeが壁」または「既にチェック済み」の場合
                if (neighboringNode.isWall || closedList.Contains(neighboringNode))
                    continue;

                //移動コストの計算(敵がいたら+10)
                int moveCost = currentNode.gCost + GetManhattenDistance(currentNode, neighboringNode);
                moveCost += dg.GetExistActor(currentNode.worldPoint) == null ? 0 : 10;

                //「moveCostがGコストより小さい」または「openListにneighborNodeがない」場合
                if (moveCost < neighboringNode.gCost || !openList.Contains(neighboringNode))
                {
                    neighboringNode.gCost = moveCost;
                    neighboringNode.hCost = GetManhattenDistance(neighboringNode, targetNode);
                    neighboringNode.parentNode = currentNode;//パスを辿るためノードの親を設定

                    if (!openList.Contains(neighboringNode))
                        openList.Add(neighboringNode);
                }
            }

        }
    }

    private void GetFinalPath(Node startNode, Node targetNode)
    {
        Node currentNode = targetNode;
        //TargetからStartまで親ノードを経由してfinalPathを作成していくループ
        while (currentNode != startNode)
        {
            finalPath.Add(currentNode);
            currentNode = currentNode.parentNode;
        }
        //StartからTargetのパスになるように逆にする
        finalPath.Reverse();
    }

    private int GetManhattenDistance(Node currentNode, Node neighborNode)
    {
        int x = Mathf.Abs(currentNode.gridX - neighborNode.gridX);//x1-x2
        int y = Mathf.Abs(currentNode.gridY - neighborNode.gridY);//y1-y2
        return x + y;
    }
}
