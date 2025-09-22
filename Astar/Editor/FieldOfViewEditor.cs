using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pathfinding))]
public class FieldOfViewEditor : Editor
{
    private void OnSceneGUI()
    {
        Pathfinding pf = (Pathfinding)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(pf.transform.position, Vector3.forward, Vector3.right, 360, pf.radius);

        (Vector3 left, Vector3 right) viewAngle = GetViewAngle(pf);

        Handles.color = Color.yellow;
        Handles.DrawLine(pf.transform.position, pf.transform.position + viewAngle.left * pf.radius);
        Handles.DrawLine(pf.transform.position, pf.transform.position + viewAngle.right * pf.radius);

        if (pf.isDetect)
        {
            Handles.color = Color.green;
            Handles.DrawLine(pf.transform.position, pf.target.transform.position);
        }
    }

    Vector3 viewR;
    Vector3 viewL;
    private (Vector3, Vector3) GetViewAngle(Pathfinding pf)
    {
        //“G‚ÌˆÚ“®•ûŒüæ“¾(‰©ü‚ª”’ü‚©‚ço‚È‚¢‚æ‚¤‚É³‹K‰»)
        Vector3 dir = pf.finalPath.Count > 0 ? (pf.finalPath[0].worldPoint - pf.transform.position).normalized : Vector3.zero;
        //ˆÚ“®•ûŒü‚©‚ç¶‰E‚Ì‹–ì‚ğİ’è
        viewL = dir != Vector3.zero ? Quaternion.Euler(0, 0, -pf.angle) * dir : viewL;
        viewR = dir != Vector3.zero ? Quaternion.Euler(0, 0, pf.angle) * dir : viewR;
        return (viewL, viewR);
    }
}
