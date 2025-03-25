using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    public static CameraMovement instance;
    public PhotonView view;
    private void Start()
    {
        instance = this;
    }
    private void Update()
    {
        if(view.IsMine)
        {
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y, player.transform.position.z);
        }
    }

}
