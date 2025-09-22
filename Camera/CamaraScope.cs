using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraScope : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.size = GameObject.Find("GameManager").GetComponent<DungeonGenerator>().MapSize;
    }
}
