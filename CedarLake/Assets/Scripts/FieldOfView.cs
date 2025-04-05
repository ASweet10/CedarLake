using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public float innerRadius;
    public float outerRadius;
    public float angle;

    public GameObject playerRef;

    [SerializeField] LayerMask targetMask;
    [SerializeField] LayerMask obstacleMask;

    public bool canSeePlayer = false;
    public bool canSeeSilhouette = false;

    private void Update() {
        StartCoroutine(FOVCoroutine());
    }

    private IEnumerator FOVCoroutine(){
        while (true) {
            yield return new WaitForSeconds(0.2f);
            CheckInnerFOV();
            CheckOuterFOV();
        }
    }

    private void CheckInnerFOV() {
        Collider[] innerRangeChecks = Physics.OverlapSphere(transform.position, innerRadius, targetMask);

        if(innerRangeChecks.Length != 0){

            Transform target = innerRangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directionToTarget) < angle / 2) {

                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){
                    canSeePlayer = true;
                    //Debug.Log("Can see player");
                } else {
                    canSeePlayer = false;
                }
            } else {
                canSeePlayer = false;
            }
        }
        else if(canSeePlayer){  // canSeePlayer was true but nothing in RangeChecks now
            canSeePlayer = false;
        }
    }

    private void CheckOuterFOV() {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, outerRadius, targetMask);

        if(rangeChecks.Length != 0){

            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if(Vector3.Angle(transform.forward, directionToTarget) < angle / 2) {

                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if(!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask)){
                    canSeeSilhouette = true;
                    //Debug.Log("Is that you, Francis?");
                } else {
                    canSeeSilhouette = false;
                }
            } else {
                canSeeSilhouette = false;
            }
        }
        else if(canSeePlayer){  // canSeePlayer was true but nothing in RangeChecks now
            canSeeSilhouette = false;
        }
    }
}
