using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gridオブジェクトにアタッチ
/// ダンジョンをグリッドで分割するクラス
/// </summary>
public class Grid : MonoBehaviour
{
    [SerializeField] private LayerMask wallMask;//レイヤーがWallになっている部分だけ黄色にするための変数
    [SerializeField] private bool canDiagonal = false;//斜め移動可能ならインスペクターからTrue
    private Vector2 gridSize;//grid表示になる範囲
    public float nodeRadius;//grid表示1ノードの半径
    [SerializeField] private float distanceBetweenNodes;//grid表示によるマスの隙間の大きさ

    Node[,] nodeArray;//A Starアルゴリズムが使用するノードの2次元配列
    //private Pathfinding pathfinding;//Gizmo用

    private async void Start()
    {
        gridSize = GameObject.Find("GameManager").GetComponent<DungeonGenerator>().MapSize;
        await UniTask.WaitUntil(() => DungeonGenerator.isFinish, cancellationToken: this.GetCancellationTokenOnDestroy());
        CreateNodeArray();
    }

    //nodeArray作成
    private void CreateNodeArray()
    {
        nodeArray = new Node[(int)gridSize.x, (int)gridSize.y];
        //grid表示左下頂点の座標取得
        Vector2 bottomLeft = -gridSize / 2;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                //それぞれのノードのワールド座標取得
                Vector3 worldPoint = new Vector3(bottomLeft.x + x, bottomLeft.y + y);
                //ノードが壁だったらTrue
                bool isWall = Physics2D.OverlapCircle(worldPoint, nodeRadius / 2, wallMask);
                nodeArray[x, y] = new Node(isWall, worldPoint, x, y);
            }
        }
    }

    //ワールド座標の位置にあるノードを取得
    public Node GetNodeFromWorldPosition(Vector2 worldPos)
    {
        //grid表示範囲におけるワールド座標位置の割合(0～1)
        float xRatio = Mathf.InverseLerp(-gridSize.x / 2, gridSize.x / 2, worldPos.x);
        float yRatio = Mathf.InverseLerp(-gridSize.y / 2, gridSize.y / 2, worldPos.y);

        //grid表示された範囲の何ノード目か
        int x = Mathf.RoundToInt(gridSize.x * xRatio);
        int y = Mathf.RoundToInt(gridSize.y * yRatio);

        return nodeArray[x, y];
    }

    //指定されたノード周辺のノードのListを取得する
    public List<Node> GetNeighboringNodes(Node currentNode)
    {
        //currentNode周辺のノードを格納するList
        List<Node> neighboringList = new List<Node>();
        int currentX = currentNode.gridX;
        int currentY = currentNode.gridY;

        //currentNodeの上下左右のノードをneighboringListに格納
        neighboringList.Add(nodeArray[currentX + 1, currentY]);
        neighboringList.Add(nodeArray[currentX - 1, currentY]);
        neighboringList.Add(nodeArray[currentX, currentY + 1]);
        neighboringList.Add(nodeArray[currentX, currentY - 1]);

        //斜め移動可能な場合
        if (canDiagonal)
        {
            //currentNodeの右上・左上・右下・左下のノードをneighboringListに格納
            if(!nodeArray[currentX, currentY + 1].isWall && !nodeArray[currentX + 1, currentY].isWall)
                neighboringList.Add(nodeArray[currentX + 1, currentY + 1]);
            if (!nodeArray[currentX, currentY + 1].isWall && !nodeArray[currentX - 1, currentY].isWall)
                neighboringList.Add(nodeArray[currentX - 1, currentY + 1]);
            if (!nodeArray[currentX, currentY - 1].isWall && !nodeArray[currentX + 1, currentY].isWall)
                neighboringList.Add(nodeArray[currentX + 1, currentY - 1]);
            if (!nodeArray[currentX, currentY - 1].isWall && !nodeArray[currentX - 1, currentY].isWall)
                neighboringList.Add(nodeArray[currentX - 1, currentY - 1]);
        }

        return neighboringList;
    }

    //枠線を描画
    /*private void OnDrawGizmos()
    {
        //インスペクタから指定したサイズのワイヤーキューブを描画
        Gizmos.DrawWireCube(transform.position, new Vector2(gridSize.x, gridSize.y));

        if (nodeArray == null)
            return;

        foreach (Node n in nodeArray)
        {
            Gizmos.color = n.isWall ? new Color(1, 0.92f, 0.016f, 0.0f) : new Color(1, 1, 1, 0.0f);
            //現在のノードがfinalPathにある場合
            if (pathfinding.finalPath != null && pathfinding.finalPath.Contains(n))
                Gizmos.color = new Color(1, 0, 0, 0.3f);

            Gizmos.DrawCube(n.worldPoint, Vector2.one * (nodeRadius * 2 - distanceBetweenNodes));
        }
    }*/
}
