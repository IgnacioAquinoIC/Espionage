using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
public class PlayerMovement : MonoBehaviour
{
    public new Rigidbody rigidbody;
    public float speed = 8f;
    //public float jump = 8f;
    //bool canJump = false;
    public Camera playerCamera;
    public float mouseSensitivity = 50f;
    private float mouseX;
    public bool movementLocked = false;
    public bool inBarrel = false;
    public Camera currentBarrelCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }

        Debug.Log("PlayerMovement started");
        Debug.Log(rigidbody);
        Debug.Log("Player Camera: " + playerCamera);
    }

    // Update is called once per frame
    void Update()
    {
        if(!movementLocked)
        {
            if (Keyboard.current.wKey.IsPressed())
            {
                rigidbody.AddForce(transform.forward * Time.fixedDeltaTime * speed, ForceMode.Impulse);
            }

            if (Keyboard.current.aKey.IsPressed())
            {
                rigidbody.AddForce(-transform.right * Time.fixedDeltaTime * speed, ForceMode.Impulse);
            }

            if (Keyboard.current.sKey.IsPressed())
            {
                rigidbody.AddForce(-transform.forward * Time.fixedDeltaTime * speed, ForceMode.Impulse);
            }

            if (Keyboard.current.dKey.IsPressed())
            {
                rigidbody.AddForce(transform.right * Time.fixedDeltaTime * speed, ForceMode.Impulse);
            }

            //if (Keyboard.current.spaceKey.IsPressed()) Not needed
            //{
                //if (canJump)
                //{
                    //canJump = false;
                    //transform.SetParent(null);
                    //rigidbody.AddForce(Vector3.up * Time.fixedDeltaTime * jump, ForceMode.Impulse);
                //}
            //}

            if (Mouse.current.rightButton.isPressed)
            {
                mouseX = Mouse.current.delta.x.ReadValue();
            }
            else
            {
                mouseX = 0f;
            }
        }
    }
    private void FixedUpdate()
    {
        if (mouseX == 0f)
            return;

        if (inBarrel && currentBarrelCamera != null)
        {
            currentBarrelCamera.transform.Rotate(
                0f,
                mouseX * mouseSensitivity * Time.fixedDeltaTime,
                0f
            );
        }
        else
        {
            Quaternion rotation = rigidbody.rotation *
                Quaternion.Euler(
                    0f,
                    mouseX * mouseSensitivity * Time.fixedDeltaTime,
                    0f
                );

            rigidbody.MoveRotation(rotation);
        }
    }
    //private void OnCollisionEnter(Collision collision)
    //{
        //if (collision.gameObject.CompareTag("Platform"))
        //{
            //canJump = true;
        //}
    //}
    //private void OnCollisionStay(Collision collision)
    //{
         //if (collision.gameObject.CompareTag("Platform"))
         //{
            //foreach (ContactPoint contact in collision.contacts)
            //{
                //if (contact.normal.y > 0.5f)
                //{
                    //canJump = true;
                //}
            //}
        //}
    //}
    //private void OnCollisionExit(Collision collision)
    //{
        //if (collision.gameObject.CompareTag("Platform"))
        //{
            //canJump = false;
        //}
    //}
}
