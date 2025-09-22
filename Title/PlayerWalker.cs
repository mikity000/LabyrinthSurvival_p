using UnityEngine;

public class PlayerWalker : MonoBehaviour
{
    [SerializeField] private Animator anim;

    void Start()
    {
        anim.SetBool("Walk", true);
    }
}
