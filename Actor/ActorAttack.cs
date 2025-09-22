using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorAttack : MonoBehaviour {
    public Animator anim;
    public float animationLength;
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int hurtHash = Animator.StringToHash("Hurt");
    [SerializeField] private LayerMask enemyMask;
    private float time = 0.0f;
    private ActorParamsController selfParams;

    private void Start() {
        selfParams = GetComponent<ActorParamsController>();
    }

    // ˆê‘Ì‚Ì“G‚ÉUŒ‚
    public void Attack(GameObject target) {
        anim.SetTrigger(attackHash);
        if (target == null)
            return;
        target.GetComponent<Animator>().SetTrigger(hurtHash);
        DamageOpponent(target);
    }

    //•¡”‚Ì“G‚ÉUŒ‚
    public void AttackMulti(ActorMovement actorMovement) {
        anim.SetTrigger(attackHash);
        //‰“‹——£UŒ‚‘ÎÛ‚Ì“G‚ğhits‚É‘ã“ü
        Vector3 newPos = DirUtil.GetNewPos(actorMovement.currentPos, actorMovement.direction, selfParams.parameter.longLv + 1);
        List<RaycastHit2D> hits = Physics2D.LinecastAll(actorMovement.currentPos, newPos, enemyMask).ToList();
        //”ÍˆÍUŒ‚‘ÎÛ‚Ì“G‚ğhits‚É‘ã“ü
        float dirAngle = DirUtil.GetDirAngle(actorMovement.direction);
        int rangeLv = selfParams.parameter.rangeLv;
        FanShapeCast(actorMovement.currentPos, 45 * rangeLv, rangeLv, dirAngle, hits);

        if (hits.Count == 0)
            return;
        foreach (RaycastHit2D enemy in hits) {
            enemy.collider.gameObject.GetComponent<Animator>().SetTrigger(hurtHash);
            DamageOpponent(enemy.collider.gameObject);
        }
    }

    // UŒ‚ƒAƒjƒ[ƒVƒ‡ƒ“‚ª‘±‚¢‚Ä‚¢‚éŠÔ‚ÍAttackingAI‚í‚ê‚ÎAttackEnd‚ğ•Ô‚·
    public State Attacking() {
        time += Time.deltaTime;
        if (time > animationLength) {
            time = 0.0f;
            return State.AttackEnd;
        }
        return State.Attacking;
    }

    //–Ú“IÀ•W‚É‘Šè‚ª‚¢‚ê‚Îƒ_ƒ[ƒW‚ğ—^‚¦‚é
    public void DamageOpponent(GameObject target) {
        if (target == null) 
            return;
        ActorParamsController targetParams = target.GetComponent<ActorParamsController>();
        targetParams.BeAttacked(selfParams);
    }

    //îŒ`‚ÉRayCast
    public List<RaycastHit2D> FanShapeCast(Vector3 origin, float angle, float resolution, float dirAngle, List<RaycastHit2D> hits) {
        for (float tmpAngle = -angle; tmpAngle <= angle; tmpAngle += angle / resolution) {
            Vector3 dir = Quaternion.AngleAxis(tmpAngle - dirAngle, Vector3.forward) * Vector3.up;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, 1, enemyMask);
            if (hit && !hits.Any(v => v.collider == hit.collider))
                hits.Add(hit);
        }
        return hits;
    }
}