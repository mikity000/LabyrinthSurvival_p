using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Gridオブジェクトにアタッチ
/// ステージ上にグリッドを表示する
/// </summary>
[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
public class DrawGrid : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verts;    //ポリゴンの頂点を入れる
    private int[] triangles;    //三角形を描く際に、頂点の描画順を指定する
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [SerializeField] private Material material;
    private Vector2 gridWorldSize;
    private float nodeRadius;
    [SerializeField] private float lineSize;

    void Start()
    {
        mesh = new Mesh();
        gridWorldSize = GameObject.Find("GameManager").GetComponent<DungeonGenerator>().MapSize;
        nodeRadius = GetComponent<Grid>().nodeRadius;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void Draw(bool isOnGrid)
    {
        if (!isOnGrid)
        {
            mesh.Clear();
            return;
        }

        //頂点の番号をsize分確保、縦横の線が一本ずつなくなるので+2を入れる、一本の線は頂点6つで表示させるので*6
        triangles = new int[(int)((gridWorldSize.x + gridWorldSize.y + 2) * 6)];
        //頂点の座標をsize分確保
        verts = new Vector3[(int)((gridWorldSize.x + gridWorldSize.y + 2) * 6)];

        //頂点番号を割り当て
        for (int i = 0; i < triangles.Length; i++)
            triangles[i] = i;

        int x = 0, y = 0;
        float adjustX = -gridWorldSize.x / 2 - nodeRadius;
        float adjustY = -gridWorldSize.y / 2 - nodeRadius;
        //縦線
        for (int i = 0; i < (gridWorldSize.x + 1) * 6; i += 6)
        {
            verts[i] = new Vector3(x + adjustX, adjustY);
            verts[i + 1] = new Vector3(x + adjustX, gridWorldSize.y + adjustY);
            verts[i + 2] = new Vector3(lineSize + x + adjustX, gridWorldSize.y + adjustY);
            verts[i + 3] = new Vector3(lineSize + x + adjustX, gridWorldSize.y + adjustY);
            verts[i + 4] = new Vector3(lineSize + x + adjustX, adjustY);
            verts[i + 5] = new Vector3(x + adjustX, adjustY);
            x++;
        }

        //横線
        for (int i = (int)((gridWorldSize.x + 1) * 6); i < (gridWorldSize.x + gridWorldSize.y + 2) * 6; i += 6)
        {
            verts[i] = new Vector3(adjustX, y + adjustY);
            verts[i + 1] = new Vector3(gridWorldSize.x + lineSize + adjustX, y + adjustY);
            verts[i + 2] = new Vector3(adjustX, y - lineSize + adjustY);
            verts[i + 3] = new Vector3(gridWorldSize.x + lineSize + adjustX, y + adjustY);
            verts[i + 4] = new Vector3(gridWorldSize.x + lineSize + adjustX, y - lineSize + adjustY);
            verts[i + 5] = new Vector3(adjustX, y - lineSize + adjustY);
            y++;
        }

        //作った頂点番号、座標データを作成したmeshに追加
        mesh.SetVertices(verts);
        mesh.SetTriangles(triangles, 0);

        //再計算()
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //再計算後に完成したMeshを追加
        meshFilter.mesh = mesh;
        //設定したMaterialを反映
        meshRenderer.material = material;
    }
}