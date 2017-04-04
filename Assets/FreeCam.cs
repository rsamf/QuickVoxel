using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]

public class FreeCam : MonoBehaviour
{
    //rotation
    [Range(0f, 500f)]
    public float mSensitivity;
    [Range(0f, 500f)]
    public float mOffset;
    private float xRot;
    private float yRot;
    //movement
    public Vector3 speed;
    [Range(0f, 100f)]
    public float boost;
    public bool limitSpeed;

    // Use this for initialization
    void Start()
    {
        xRot = 0f;
        yRot = 0f;
    }

    // Update is called once per frame
    void Update()
    {

        xRot -= Input.GetAxis("Mouse Y") * (mSensitivity + mOffset) * Time.deltaTime;
        yRot += Input.GetAxis("Mouse X") * (mSensitivity + mOffset) * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90, 90);
        if (mSensitivity != 0) transform.rotation = Quaternion.Euler(xRot, yRot, 0);
        float speedLimitFwd = speed.z * Input.GetAxis("Vertical") /
            (speed.x * Input.GetAxis("Horizontal"));
        float speedLimitStrafe = 1f / speedLimitFwd;
        float speedLimitIncline = 0;
        if (!limitSpeed)
        {
            speedLimitFwd = 1f;
            speedLimitStrafe = 1f;
            speedLimitIncline = 1f;
        }
        transform.position += transform.forward *
            (Input.GetAxis("Vertical") + (boost * Input.GetAxis("Sprint") * Input.GetAxis("Vertical")))
            * Time.deltaTime * speed.z * speedLimitFwd;
        transform.position += transform.right *
            (Input.GetAxis("Horizontal") + (boost * Input.GetAxis("Sprint") * Input.GetAxis("Horizontal")))
            * Time.deltaTime * speed.x * speedLimitStrafe;
        transform.position += transform.up *
            (Input.GetAxis("FreeCamVertical") + (boost * Input.GetAxis("Sprint") * Input.GetAxis("FreeCamVertical")))
            * Time.deltaTime * speed.y * speedLimitIncline;
    }
}