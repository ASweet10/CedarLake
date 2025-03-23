using UnityEngine;

public class CopCarLights : MonoBehaviour
{
    [SerializeField] Light redLight;
    [SerializeField] Light blueLight;
    [SerializeField] int speed;

    Vector3 redTemp;
    Vector3 blueTemp;

    void Update() {
        HandleSirenEffect();
    }

    void HandleSirenEffect() {
        redTemp.y += 300 * Time.deltaTime;
        blueTemp.y -= 300 * Time.deltaTime;

        redLight.transform.eulerAngles = redTemp;
        blueLight.transform.eulerAngles = blueTemp;
    }
}
