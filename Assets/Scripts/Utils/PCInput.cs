using System.Collections;
using UnityEngine;

public class PCInput : MonoBehaviour
{
    // Debuging code, only for editor
#if UNITY_EDITOR

    [SerializeField]
    private float sensitivityX = 5F;
    [SerializeField]
    private float sensitivityY = 5F;
    [SerializeField]
    private float MAXLOOKSPEED = 5;
    [SerializeField]
    private float minimumX = -360F;
    [SerializeField]
    private float maximumX = 360F;
    [SerializeField]
    private float minimumY = -85F;
    [SerializeField]
    private float maximumY = 85F;
    private float rotationX = 0F;
    private float rotationY = 0F;
    private Quaternion originalRotation;
    private string hmode = "Mouse X";
    private string vmode = "Mouse Y";

    private bool WASDenabled = false;

    [SerializeField]
    private float MoveSpeed = 0.1f;
    private float MoveSpeed2;
    [SerializeField]
    private float ShiftSpeed = 0.5f;
    [SerializeField]
    private float SlowSpeed = 0.05f;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        MoveSpeed2 = MoveSpeed;

        transform.position = Camera.main.transform.position;
        originalRotation = Quaternion.identity;

        // Here we calculate the starting angles of the camera's forward vector.
        // This just keeps the camera looking in the same direction as in the editor.
        Vector3 a = Camera.main.transform.forward;
        a.y = 0;
        rotationX = Mathf.Atan2(Camera.main.transform.forward.x, Camera.main.transform.forward.z) * Mathf.Rad2Deg;
        rotationY = Mathf.Atan2(Camera.main.transform.forward.y, a.magnitude) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.identity;
        Camera.main.transform.localRotation = Quaternion.identity;

        UpdateXandY(); // initializes our variables before rendering the first frame
    }

    private void UpdateXandY()
    {
        if (Input.GetAxis(hmode) * sensitivityX > MAXLOOKSPEED)
        {
            rotationX += MAXLOOKSPEED;
        }
        else
        {
            rotationX += Input.GetAxis(hmode) * sensitivityX;
        }
        if (Input.GetAxis(vmode) * sensitivityY > MAXLOOKSPEED)
        {
            rotationY += MAXLOOKSPEED;
        }
        else
        {
            rotationY += Input.GetAxis(vmode) * sensitivityY;
        }
        rotationX = ClampAngle(rotationX, minimumX, maximumX);
        rotationY = ClampAngle(rotationY, minimumY, maximumY);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
        transform.localRotation = originalRotation * xQuaternion * yQuaternion;

        Camera.main.transform.localRotation = transform.rotation;
    }

    void Update()
    {
        if (WASDenabled)
        {
            // Camera movement like in UnityEditor
            float CurrentSpeed = MoveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) // fast speed can only be gotten by holding down left shift
            {
                CurrentSpeed = ShiftSpeed;
            }
            if (Input.GetKeyDown(KeyCode.Space)) // toggles normal speed between slow and normal
            {
                if(MoveSpeed == SlowSpeed)
                {
                    MoveSpeed = MoveSpeed2;
                }
                else
                {
                    MoveSpeed = SlowSpeed;
                }
            }
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * CurrentSpeed;
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.position += transform.up * CurrentSpeed;
            }
            if (Input.GetKey(KeyCode.Q))
            {
                transform.position -= transform.up * CurrentSpeed;
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * CurrentSpeed;
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * CurrentSpeed;
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * CurrentSpeed;
            }
        }

        // RMB held down
        if (Input.GetMouseButton(1))
        {
            WASDenabled = true;
            UpdateXandY();
        }
        else
        {
            WASDenabled = false;
        }
        Camera.main.transform.position = transform.position;
    }
        
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
        {
            angle += 360F;
        }
        if (angle > 360F)
        {
            angle -= 360F;
        }
        return Mathf.Clamp(angle, min, max);
    }

#endif
}
