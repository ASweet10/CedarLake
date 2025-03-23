using UnityEngine;

public class PickUpObjects : MonoBehaviour
{
    GameController gameController;
    FirstPersonHighlights fpHighlights;
    MouseLook mouseLook;
    [SerializeField] GameObject heldObjUI;
    [SerializeField] GameObject player;
    [SerializeField] Transform objectHoldPos;

    [SerializeField] float throwForce = 300f;
    float rotationSensitivity = 1f; // object rotation speed in relation to mouse movement
    GameObject heldObj;
    Rigidbody heldObjRb;
    public bool canDrop = true;
    int layerNumber;
    void Start() {
        layerNumber = LayerMask.NameToLayer("holdObject");
        gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        fpHighlights = GameObject.FindGameObjectWithTag("Player").GetComponent<FirstPersonHighlights>();
        mouseLook = gameObject.GetComponent<MouseLook>();
    }
    void Update() {
        HandleObjectLogic();
    }

    void HandleObjectLogic() {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (heldObj == null) {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 5f)){
                    if (hit.transform.gameObject.tag == "Pickup") {
                        HandlePickUpObject(hit.transform.gameObject);
                    }
                }
            }
        } else if(Input.GetKeyDown(KeyCode.G)) {
            if(canDrop == true) {
                StopClipping(); //prevents object from clipping through walls
                DropObject();
            }
        }

        if (heldObj != null) {
            gameController.holdingGasStationItem = true;
            MoveObject(); //keep object position at holdPos
            RotateObject();
            if (Input.GetKeyDown(KeyCode.Mouse0) && canDrop == true) {
                StopClipping();
                ThrowObject();
            }
        }
    }

    public void HandlePickUpObject(GameObject obj) {
        if (obj.GetComponent<Rigidbody>()) {
            heldObj = obj;
            heldObjRb = obj.GetComponent<Rigidbody>();
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = objectHoldPos.transform; //parent object to holdposition
            heldObj.layer = layerNumber; //change the object layer to the holdLayer
            //make sure object doesnt collide with player, it can cause weird bugs
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
            heldObjUI.SetActive(true);
        }
    }
    public void DropObject() {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0; //object assigned back to default layer
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null; //unparent object
        heldObj = null; //undefine game object

        gameController.holdingGasStationItem = false;
        heldObjUI.SetActive(false);
        fpHighlights.ClearHighlighted();
    }

    void MoveObject() {
        //keep object position the same as the holdPosition position
        heldObj.transform.position = objectHoldPos.transform.position;
    }
    void RotateObject() {
        if (Input.GetKey(KeyCode.R)) {
            canDrop = false; // Disable throw while rotating
            mouseLook.CanRotateMouse = false;

            float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
            float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;
            //rotate the object depending on mouse X-Y Axis
            heldObj.transform.Rotate(Vector3.down, XaxisRotation);
            heldObj.transform.Rotate(Vector3.right, YaxisRotation);
        }
        else {
            mouseLook.CanRotateMouse = true;
            canDrop = true;
        }
    }
    void ThrowObject() {
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce); // Add force to object before undefining it
        heldObj = null;

        gameController.holdingGasStationItem = false;
        heldObjUI.SetActive(false);
    }
    void StopClipping() { //function only called when dropping/throwing
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); //distance from holdPos to the camera
        //have to use RaycastAll as object blocks raycast in center screen
        //RaycastAll returns array of all colliders hit within the cliprange
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        //if the array length is greater than 1, meaning it has hit more than just the object we are carrying
        if (hits.Length > 1)
        {
            //change object position to camera position 
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); //offset slightly downward to stop object dropping above player 
            //if your player is small, change the -0.5f to a smaller number (in magnitude) ie: -0.1f
        }
    }
}