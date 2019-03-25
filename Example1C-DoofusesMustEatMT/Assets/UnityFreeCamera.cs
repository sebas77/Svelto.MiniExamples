using UnityEngine;

public class UnityFreeCamera : MonoBehaviour {

    Rigidbody rb;

    float lookSensitivity = 50;
    float speed           = 300f;

    float rotationX = 0.0f;
    float rotationY = 0.0f;

    // Use this for initialization
    void Start () {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody> ();
    }
    
    // Update is called once per frame
    void Update () {
        if (Input.GetKey(KeyCode.LeftControl) == false)
        {
            rotationX += Input.GetAxis("Mouse X") * lookSensitivity * Time.deltaTime;
            rotationY += Input.GetAxis("Mouse Y") * lookSensitivity * Time.deltaTime;
            rotationY =  Mathf.Clamp(rotationY, -90, 90);

            transform.localRotation =  Quaternion.AngleAxis(rotationX, Vector3.up);
            transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);

            rb.velocity = Vector3.zero;
            rb.velocity += (transform.forward * Input.GetAxis("Vertical") +
                            Vector3.up * (Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f) +
                            Vector3.down * (Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.0f) +
                            transform.right * Input.GetAxis("Horizontal")) * Time.deltaTime * speed;
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}