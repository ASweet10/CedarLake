using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float radius;
    public float angle;
    public GameObject playerRef;

    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;

    public bool canSeePlayer = false;

    private void Update() {
        StartCoroutine(FOVCoroutine());
    }

    private IEnumerator FOVCoroutine(){
        while (true) {
            yield return new WaitForSeconds(0.2f);
            CheckFOV();
        }
    }

    private void CheckFOV() {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, targetMask);

        if(rangeChecks.Length != 0) {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directionToTarget) < angle / 2) {

                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){
                    canSeePlayer = true;
                } else {
                    canSeePlayer = false;
                }
            } else {
                canSeePlayer = false;
            }
        }
        else if(canSeePlayer){
            canSeePlayer = false;
        }
    }
}