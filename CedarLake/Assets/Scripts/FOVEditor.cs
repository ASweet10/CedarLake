using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FieldOfView))]
public class FOVEditor : Editor
{
    void OnSceneGUI(){
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.innerRadius);
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.outerRadius);

        Vector3 viewAngle01 = DirectionFromAngle(fov.transform.eulerAngles.y, fov.angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(fov.transform.eulerAngles.y, -fov.angle / 2);

        Handles.color = Color.yellow;
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.innerRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.innerRadius);

        Handles.color = new Color(0, 155, 155);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle01 * fov.outerRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngle02 * fov.outerRadius);

        if(fov.canSeePlayer){
            Handles.color = Color.red;
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
        }
        if(fov.canSeeSilhouette){
            Handles.color = new Color(155, 75, 75);
            Handles.DrawLine(fov.transform.position, fov.playerRef.transform.position);
        }
    }

    Vector3 DirectionFromAngle( float eulerY, float angle ){
        angle += eulerY;
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }
}
