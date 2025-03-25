using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private float speed = 1f;
    private PhotonView view;
    public float rotatespeed;
    public ConfigurableJoint hipJoint;
    private float targetAngle;
    private float mouseY;
    public GameObject focuspoint;
    private bool curserlocked;
   // public TextMeshProUGUI name_text;
    private string playername;

    void Start()
    {
       // view = GetComponent<PhotonView>();
        Cursor.lockState = CursorLockMode.Locked;
      //  playername = view.Owner.NickName;
      //  name_text.text = playername;
    }

    // Update is called once per frame
    void Update()
    {
        
            Move();
            MouseLook();
    }

    void Move()
    {
        Vector2 inputVector = new Vector2(0, 0);
        if (Input.GetKey(KeyCode.W))
        {
            inputVector.y = +1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputVector.y = -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputVector.x = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputVector.x = +1;
        }
        if (inputVector != Vector2.zero)
        {
            inputVector = inputVector.normalized;
            targetAngle = Mathf.Atan2(inputVector.x, inputVector.y) * Mathf.Rad2Deg + focuspoint.transform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotatespeed);
            hipJoint.targetRotation = Quaternion.Euler(0, -angle, 0f);
            Vector3 movDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            transform.position += speed * Time.deltaTime * movDir;
        }
        mouseY -= Input.GetAxis("Mouse Y") * rotatespeed;
        mouseY = Mathf.Clamp(mouseY, -20, 20);
    }
    void MouseLook()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) curserlocked = (curserlocked) ? false : true;
        Cursor.lockState = curserlocked ? CursorLockMode.Locked : CursorLockMode.None;
        if (curserlocked)
        {
            focuspoint.transform.eulerAngles = new Vector3(ClampAngle(focuspoint.transform.eulerAngles.x + Input.GetAxis("Mouse Y") * 2, -30f, 35f), focuspoint.transform.eulerAngles.y, 0);
            focuspoint.transform.parent.Rotate(transform.up * Input.GetAxis("Mouse X") * 150 * Time.deltaTime);
        }
    }
    protected float ClampAngle(float angle, float min, float max)
    {

        angle = NormalizeAngle(angle);
        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180)
        {
            angle += 360;
        }

        min = NormalizeAngle(min);
        if (min > 180)
        {
            min -= 360;
        }
        else if (min < -180)
        {
            min += 360;
        }

        max = NormalizeAngle(max);
        if (max > 180)
        {
            max -= 360;
        }
        else if (max < -180)
        {
            max += 360;
        }

        // Aim is, convert angles to -180 until 180.
        return Mathf.Clamp(angle, min, max);
    }
    protected float NormalizeAngle(float angle)
    {
        while (angle > 360)
            angle -= 360;
        while (angle < 0)
            angle += 360;
        return angle;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            gameManager.instance.namePanel.SetActive(true);
            gameManager.instance.WinnerName.text = view.Owner.NickName;
            Cursor.lockState = CursorLockMode.None;
            this.enabled = false;
            
        }
    }
}
