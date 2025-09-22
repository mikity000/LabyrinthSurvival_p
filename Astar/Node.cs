using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ノード情報を保持するクラス
/// </summary>
public class Node {

    public int gridX;//NodeArrayの第一添字(bottomLeftから数えたx座標)
    public int gridY;//NodeArrayの第二添字(bottomLeftから数えたy座標)
    public bool isWall;//ノードが壁かを示すフラグ
    public Vector3 worldPoint;//ノードのワールド座標
    public Node parentNode;//最短経路を辿れるように、前のノードを代入
    public int gCost;//次のノードに移動するためのGコスト
    public int hCost;//次のノードからゴールに移動するためのHコスト

    public int fCost { get { return gCost + hCost; } }//最短経路で移動するためのFコスト

    public Node(bool isWall, Vector2 worldPoint, int gridX, int gridY)
    {
        this.isWall = isWall;//このノードが壁かどうか
        this.worldPoint = worldPoint;//ノードのワールド座標
        this.gridX = gridX;//NodeArrayの第一添字(bottomLeftから数えたx座標)
        this.gridY = gridY;//NodeArrayの第二添字(bottomLeftから数えたy座標)
    }

}
