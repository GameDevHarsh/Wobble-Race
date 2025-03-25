using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cam;

    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;

    [SerializeField] ProceduralLegsController proceduralLegs;

    [SerializeField] Rigidbody headRb;

    [SerializeField] float feetGroundCheckDist;

    ConfigurableJoint hipsCj;
    Rigidbody hipsRb;

    LayerMask groundMask;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotationForce;
    [SerializeField] float balanceForce;
    [SerializeField] float jumpForce;
    
    [SerializeField] float maxVelocityChange;

    bool isGrounded;
    bool isDead = false;

    float horizontal, vertical;

    [SerializeField] ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;
    public GameObject focuspoint;
    private bool curserlocked;
    public TextMeshProUGUI name_text;
    private string playername;
    private PhotonView view;
    [SerializeField] float airSpring;
    private void Start()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

        hipsRb = GetComponent<Rigidbody>();
        hipsCj = GetComponent<ConfigurableJoint>();

        //Saves the initial drives of each configurable joint
        for(int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");
        view = GetComponent<PhotonView>();
        playername = view.Owner.NickName;
        name_text.text = playername;

    }
    void Update()
    {
        if (!isDead)
        {
            proceduralLegs.GroundHomeParent();
            CheckGrounded();
            SetPlayerInputs();

            if (isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                    Jump();
            }
        }
    }
    void FixedUpdate()
    {
        if (isGrounded && !isDead)
        {
            StabilizeBody();
            Move();
            MouseLook();
        }    
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

    void SetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
    }

    void Move()
    {
        Vector3 move = new Vector3(horizontal, 0f, vertical);
        move = cam.TransformDirection(move);

        Vector3 targetVelocity = new Vector3(move.x, 0, move.z);
        targetVelocity *= moveSpeed;

        Vector3 velocity = hipsRb.linearVelocity;
        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
        velocityChange.y = 0;
        hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);

        float desiredAngle = 0;
        float rootAngle = transform.eulerAngles.y;

        if(targetVelocity.normalized != Vector3.zero)
        {
             desiredAngle = Quaternion.LookRotation(targetVelocity.normalized).eulerAngles.y;
        }

        float deltaAngle = Mathf.DeltaAngle(rootAngle, desiredAngle);

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            hipsRb.AddTorque(Vector3.up * deltaAngle * rotationForce, ForceMode.Acceleration);
        }
    }

    void CheckGrounded()
    {
        bool leftCheck = false;
        bool rightCheck = false;
        RaycastHit hit;

        if (Physics.Raycast(leftFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            leftCheck = true;

        if (Physics.Raycast(rightFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            rightCheck = true;

        if ((rightCheck || leftCheck) && !isGrounded)
        {
            SetDrives();
        }
        else if((!rightCheck && !leftCheck) && isGrounded)
        {
            Die(true);
        }
    }

    public void Die(bool respawn)
    {
        foreach (ConfigurableJoint cj in cjs)
        {
            cj.angularXDrive = inAirDrive;
            cj.angularYZDrive = inAirDrive;
        }

        hipsCj.angularYZDrive = hipsInAirDrive;
        hipsCj.angularXDrive = hipsInAirDrive;

        proceduralLegs.DisableIk();
        isGrounded = false;

        if (!respawn)
            isDead = true;

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
    void SetDrives()
    {
        for(int i = 0; i < cjs.Length; i++)
        {
            cjs[i].angularXDrive = jds[i];
            cjs[i].angularYZDrive = jds[i];

        }

        proceduralLegs.EnableIk();
        isGrounded = true;
    }
    void Jump()
    {
        hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
        hipsRb.AddTorque(new Vector3(750, 0));
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

}



