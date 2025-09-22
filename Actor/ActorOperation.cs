using UnityEngine;

public abstract class ActorOperation : MonoBehaviour {
    // Ÿ‚És‚¤—\’è‚Ìs“®ó‘Ô‚ğ•Ô‚·
    public abstract State GetNextState(ActorMovement actorMovement);
}