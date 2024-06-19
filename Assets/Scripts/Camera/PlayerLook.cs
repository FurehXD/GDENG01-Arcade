using DG.Tweening;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    public Transform playerOrientation;
    public Transform camHolder;
    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        // Lock the cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mouseSensitivity;

        // Calculating X axis rotation for looking up and down
        yRotation += mouseX;
        xRotation -= mouseY;
        
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limiting rotation to avoid flipping

        // Apply rotation for looking up and down
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
        playerOrientation.rotation = Quaternion.Euler(0, yRotation, 0f);
        // Apply rotation for turning left and right
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}
