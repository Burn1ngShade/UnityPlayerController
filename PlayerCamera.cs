using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [System.NonSerialized]
    public bool freezeCamera;

    [Header("Rotation")]
    [SerializeField] private float horizontalSpeed = 1f;
    [SerializeField] private float verticalSpeed = 1f;
    [Space]
    [SerializeField] private float minXRotation;
    [SerializeField] private float maxXRotation;

    private float xRotation = 0.0f;
    private float yRotation = 0.0f;
    
    private Camera cam;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        cam = Camera.main;
    }

    void Update()
    {
        if (!freezeCamera)
        {
            float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -Mathf.Abs(minXRotation), Mathf.Abs(maxXRotation));

            transform.eulerAngles = new Vector3(0.0f, yRotation, 0.0f);
            cam.transform.localEulerAngles = new Vector3(xRotation, 0.0f, 0.0f);
        }
    }
}
